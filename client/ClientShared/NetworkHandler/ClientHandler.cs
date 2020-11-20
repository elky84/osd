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
        readonly IByteBuffer initialMessage;

        public ClientDispatcher ClientDispatcher { get; set; }

        private Random Random { get; set; } = new Random();

        public ClientHandler()
        {
            this.initialMessage = new NetworkShared.Protocols.Request.Enter { UserName = ClientSettings.UserName }.ToByteBuffer();
        }

        public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;

            byte[] bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);

            var header = JsonConvert.DeserializeObject<NetworkShared.Protocols.Response.Header>(Encoding.UTF8.GetString(bytes, 0, bytes.Length));

            if (false == ClientDispatcher.Call(context, header))
            {
                Log.Logger.Information($"clientDispatcher.Call() failed. {context}]");
                return;
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Log.Logger.Error("Exception: " + exception);
            context.CloseAsync();
        }
    }
}