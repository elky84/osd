using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NetworkShared.Util;
using Newtonsoft.Json;
using ClientShared.Config;
using System.Collections.Generic;
using System.IO;
using Serilog;

namespace ClientShared.NetworkHandler
{

    public class ClientHandler : ChannelHandlerAdapter
    {
        public ClientDispatcher ClientDispatcher { get; set; }

        public ClientHandler()
        {
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {

        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;

            byte[] bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);

            //if (false == ClientDispatcher.Call(context, header))
            //{
            //    Log.Logger.Information($"clientDispatcher.Call() failed. {context}]");
            //    return;
            //}
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Log.Logger.Error("Exception: " + exception);
            context.CloseAsync();
        }
    }
}