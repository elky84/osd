using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using UnityEngine;

public class ClientHandler : SimpleChannelInboundHandler<IByteBuffer>
{
    public Action OnClose;

    //public delegate bool PacketDelegate(Protocols.Response.Header header);

    //private static readonly Dictionary<Protocols.Id.Response, PacketDelegate> PacketDelegates = new Dictionary<Protocols.Id.Response, PacketDelegate>();

    //public static void AddPacketDelegate(Protocols.Id.Response packetId, PacketDelegate packetDelegate)
    //{
    //    if (PacketDelegates.TryGetValue(packetId, out var value))
    //    {
    //        PacketDelegates[packetId] = value + packetDelegate;
    //    }
    //    else
    //    {
    //        PacketDelegates.Add(packetId, packetDelegate);
    //    }
    //}

    //public static void RemovePacketDelegate(Protocols.Id.Response packetId, PacketDelegate packetDelegate)
    //{
    //    if (PacketDelegates.TryGetValue(packetId, out var value))
    //    {
    //        PacketDelegates[packetId] = value - packetDelegate;
    //    }
    //}

    protected override void ChannelRead0(IChannelHandlerContext contex, IByteBuffer buffer)
    {
        try
        {
            byte[] bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);

            //var header = JsonConvert.DeserializeObject<Protocols.Response.Header>(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            //if (false == PacketDelegates.TryGetValue(header.Id, out var packetDelegate))
            //{
            //    Debug.LogError($"Id:{header.Id}. Not found PacketDelegate");
            //    return;
            //}

            //MainThreadDispatcher.Instance.Enqueue(() => packetDelegate.Invoke(header));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
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
