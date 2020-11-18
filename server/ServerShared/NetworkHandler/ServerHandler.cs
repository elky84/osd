using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using LightInject;
using Newtonsoft.Json;
using Serilog;
using ServerShared.Model;
using ServerShared.Service;
using ServerShared.Util;
using ServerShared.Worker;

namespace ServerShared.NetworkHandler
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        public SessionService SessionService { get; set; }

        public ServerDispatcher ServerDispatcher { get; set; }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            this.SessionService = ServerService.GetInstance<SessionService>();
            this.ServerDispatcher = ServerService.GetInstance<ServerDispatcher>();

            base.ChannelActive(context);
            SessionService.Add(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            SessionService.Remove(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object byteBuffer)
        {
            if (false == SessionService.Get(context, out Session session))
            {
                Log.Logger.Error("Session Get Failed() {0}", context);
                return;
            }

            var buffer = byteBuffer as IByteBuffer;
            var bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);

            var str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            var message = new Message
            {
                Header = JsonConvert.DeserializeObject<Protocols.Request.Header>(str),
                Session = session
            };

            ServerDispatcher.Push(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Log.Logger.Error("Exception: " + exception);
            context.CloseAsync();
        }
    }
}