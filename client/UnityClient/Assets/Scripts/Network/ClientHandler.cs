using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FlatBuffers;
using FlatBuffers.Protocol;
using NetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class FlatBufferEventAttribute : Attribute
{ }

public class ClientHandler : SimpleChannelInboundHandler<IByteBuffer>
{
    public Action OnClose;

    private readonly Dictionary<Type, Delegate> _allocatorDict = new Dictionary<Type, Delegate>();

    private readonly Dictionary<string, Type> _flatBufferDict = new Dictionary<string, Type>();

    private readonly Dictionary<Type, Func<IFlatbufferObject, bool>> _bindedEventDict = new Dictionary<Type, Func<IFlatbufferObject, bool>>();

    public ClientHandler()
    {
        BindFlatBufferAllocator("NetworkShared");
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
            _flatBufferDict.Add(flatbType.FullName, flatbType);
        }
    }

    public void BindEventHandler<T>(T t) where T : class
    {
        var methods = typeof(T).GetMethods().Where(x =>
        {
            if (x.GetCustomAttribute<FlatBufferEventAttribute>() == null)
                return false;

            if (x.ReturnType != typeof(bool))
                return false;

            var parameters = x.GetParameters();
            if (parameters.Length != 1)
                return false;

            if (parameters[0].ParameterType.GetInterface(nameof(IFlatbufferObject)) == null)
                return false;

            return true;
        });

        foreach (var method in methods)
        {
            try
            {
                var parameters = method.GetParameters();
                var flatBufferType = parameters[0].ParameterType;
                var delegateType = Expression.GetDelegateType(parameters.Select(x => x.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
                var createdDelegate = method.CreateDelegate(delegateType, t);
                _bindedEventDict.Add(flatBufferType, new Func<IFlatbufferObject, bool>((protocol) =>
                {
                    return (bool)createdDelegate.DynamicInvoke(Convert.ChangeType(protocol, flatBufferType));
                }));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    public override void ChannelInactive(DotNetty.Transport.Channels.IChannelHandlerContext context)
    {
        base.ChannelInactive(context);
    }



    protected override void ChannelRead0(IChannelHandlerContext contex, IByteBuffer buffer)
    {
        try
        {
            var size = buffer.ReadInt();
            var strLength = buffer.ReadByte();
            var flatBufferName = buffer.ReadString(strLength, System.Text.Encoding.Default);
            if (_flatBufferDict.TryGetValue(flatBufferName, out var flatBufferType) == false)
                throw new Exception($"{flatBufferName} is not binded in event handler.");

            var bytes = new byte[size];
            buffer.ReadBytes(bytes);
            MainThreadDispatcher.Instance.Enqueue(() => Call(flatBufferType, bytes));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public bool Call<FlatBufferType>(byte[] bytes) where FlatBufferType : struct, IFlatbufferObject
    {
        try
        {
            if (_bindedEventDict.TryGetValue(typeof(FlatBufferType), out var bindedEvent) == false)
                return false;

            if (_allocatorDict.TryGetValue(typeof(FlatBufferType), out var allocator) == false)
                return false;

            return bindedEvent.Invoke((FlatBufferType)allocator.DynamicInvoke(new ByteBuffer(bytes)));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public bool Call(Type type, byte[] bytes)
    {
        try
        {
            if (_bindedEventDict.TryGetValue(type, out var bindedEvent) == false)
                return false;

            if (_allocatorDict.TryGetValue(type, out var allocator) == false)
                return false;

            return bindedEvent.Invoke(Convert.ChangeType(allocator.DynamicInvoke(new ByteBuffer(bytes)), type) as IFlatbufferObject);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
    {
        Debug.LogError(e.StackTrace);
        contex.CloseAsync();
    }

    public override void ChannelUnregistered(IChannelHandlerContext ctx)
    {
        base.ChannelUnregistered(ctx);
        Debug.LogError("Channel Closed!!");
        OnClose?.Invoke();
    }
}
