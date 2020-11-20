using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FlatBuffers;
using Serilog;
using ServerShared.Model;
using ServerShared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ServerShared.NetworkHandler
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FlatBufferEventAttribute : Attribute
    { }

    public abstract class BaseHandler<SessionType> : ChannelHandlerAdapter where SessionType : BaseSession, new()
    {
        private Dictionary<Type, Delegate> _allocatorDict = new Dictionary<Type, Delegate>();
        private Dictionary<Type, Func<SessionType, IFlatbufferObject, bool>> _bindedEventDict = new Dictionary<Type, Func<SessionType, IFlatbufferObject, bool>>();
        private Dictionary<IChannelHandlerContext, SessionType> _sessionDict = new Dictionary<IChannelHandlerContext, SessionType>();

        public ServerDispatcher ServerDispatcher { get; set; }

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
            this.ServerDispatcher = ServerService.GetInstance<ServerDispatcher>();

            base.ChannelActive(context);
            _sessionDict.Add(context, new SessionType());
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            if(_sessionDict.ContainsKey(context))
                _sessionDict.Remove(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object byteBuffer)
        {
            if(_sessionDict.TryGetValue(context, out var session) == false)
            {
                Log.Logger.Error("Session Get Failed() {0}", context);
                return;
            }

            var buffer = byteBuffer as IByteBuffer;
            var bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);

            //var str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            //var message = new Message
            //{
            //    Header = JsonConvert.DeserializeObject<NetworkShared.Protocols.Request.Header>(str),
            //    Session = session
            //};

            //ServerDispatcher.Push(message);
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
    }
}