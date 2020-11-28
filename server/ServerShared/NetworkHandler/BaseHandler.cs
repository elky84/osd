using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FlatBuffers;
using Serilog;
using ServerShared.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ServerShared.NetworkHandler
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FlatBufferEventAttribute : Attribute
    { }

    public class BaseHandler<SessionType> : ChannelHandlerAdapter where SessionType : BaseSession, new()
    {
        private Dictionary<Type, Delegate> _allocatorDict = new Dictionary<Type, Delegate>();
        private Dictionary<string, Type> _flatBufferDict = new Dictionary<string, Type>();
        private Dictionary<Type, Func<SessionType, IFlatbufferObject, bool>> _bindedEventDict = new Dictionary<Type, Func<SessionType, IFlatbufferObject, bool>>();
        private Dictionary<IChannelHandlerContext, SessionType> _sessionDict = new Dictionary<IChannelHandlerContext, SessionType>();

        protected BaseHandler()
        {
            BindFlatBufferAllocator("NetworkShared");
            BindEventHandler();
        }

        private void BindFlatBufferAllocator(string assemblyName)
        {
            var assembly = string.IsNullOrEmpty(assemblyName) ?
                Assembly.GetExecutingAssembly() :
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName);

            var flatBufferTypes = assembly.GetTypes().Where(x => x.GetInterface(nameof(IFlatbufferObject)) != null).ToList();

            foreach (var flatbType in flatBufferTypes)
            {
                var methods = flatbType.GetMethods();
                var allocator = methods.FirstOrDefault(x =>
                {
                    if (x.IsStatic == false)
                        return false;

                    var parameters = x.GetParameters();
                    if (parameters.Length != 1)
                        return false;

                    if (parameters.First().ParameterType != typeof(ByteBuffer))
                        return false;

                    if (x.ReturnType != flatbType)
                        return false;

                    return true;
                });

                var method = allocator.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(ByteBuffer), flatbType));
                _allocatorDict.Add(flatbType, method);
                _flatBufferDict.Add(flatbType.Name, flatbType);
            }
        }

        private void BindEventHandler()
        {
            var methods = this.GetType().GetMethods().Where(x =>
            {
                if (x.GetCustomAttribute<FlatBufferEventAttribute>() == null)
                    return false;

                if (x.ReturnType != typeof(bool))
                    return false;

                var parameters = x.GetParameters();
                if (parameters.Length != 2)
                    return false;

                if (parameters[0].ParameterType != typeof(SessionType))
                    return false;

                if (parameters[1].ParameterType.GetInterface(nameof(IFlatbufferObject)) == null)
                    return false;

                return true;
            });

            foreach (var method in methods)
            {

                try
                {
                    var parameters = method.GetParameters();
                    var flatBufferType = parameters[1].ParameterType;
                    var delegateType = Expression.GetDelegateType(parameters.Select(x => x.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
                    var createdDelegate = method.CreateDelegate(delegateType, this);
                    _bindedEventDict.Add(flatBufferType, new Func<SessionType, IFlatbufferObject, bool>((session, protocol) =>
                    {
                        return (bool)createdDelegate.DynamicInvoke(session, Convert.ChangeType(protocol, flatBufferType));
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
            _sessionDict.Add(context, new SessionType());
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            if (_sessionDict.ContainsKey(context))
                _sessionDict.Remove(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object byteBuffer)
        {
            if (_sessionDict.TryGetValue(context, out var session) == false)
            {
                Log.Logger.Error("Session Get Failed() {0}", context);
                return;
            }

            var buffer = byteBuffer as IByteBuffer;
            try
            {
                var size = buffer.ReadInt();
                var strLength = buffer.ReadByte();
                var flatBufferName = buffer.ReadString(strLength, System.Text.Encoding.Default);
                if (_flatBufferDict.TryGetValue(flatBufferName, out var flatBufferType) == false)
                    throw new Exception($"{flatBufferName} is not binded in event handler.");

                var bytes = new byte[size];
                buffer.ReadBytes(bytes);
                var result = Call(session, flatBufferType, bytes);
                if (result == false)
                {
                    _sessionDict.Remove(context);
                    context.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"{e}");
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Log.Logger.Error("Exception: " + exception);
            context.CloseAsync();
        }

        public bool Call<FlatBufferType>(SessionType session, byte[] bytes) where FlatBufferType : struct, IFlatbufferObject
        {
            try
            {
                if (_bindedEventDict.TryGetValue(typeof(FlatBufferType), out var bindedEvent) == false)
                    return false;

                if (_allocatorDict.TryGetValue(typeof(FlatBufferType), out var allocator) == false)
                    return false;

                return bindedEvent.Invoke(session, (FlatBufferType)allocator.DynamicInvoke(new ByteBuffer(bytes)));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Call(SessionType session, Type type, byte[] bytes)
        {
            try
            {
                if (_bindedEventDict.TryGetValue(type, out var bindedEvent) == false)
                    return false;

                if (_allocatorDict.TryGetValue(type, out var allocator) == false)
                    return false;

                return bindedEvent.Invoke(session, Convert.ChangeType(allocator.DynamicInvoke(new ByteBuffer(bytes)), type) as IFlatbufferObject);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}