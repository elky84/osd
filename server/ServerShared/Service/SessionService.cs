using DotNetty.Transport.Channels;
using LightInject;
using System.Collections.Generic;
using ServerShared.Model;
using Serilog;

namespace ServerShared.Service
{
    public class SessionService
    {
        [Inject]
        public ChannelService ChannelService { get; set; }

        private Dictionary<IChannelHandlerContext, Session> Sessions = new Dictionary<IChannelHandlerContext, Session>();

        public void Add(IChannelHandlerContext context)
        {
            Sessions.Add(context, new Session(context));
        }

        public void Remove(IChannelHandlerContext context)
        {
            if (false == Get(context, out Session session))
            {
                Log.Logger.Information("Session Get Failed {0}", context);
                return;
            }

            //ChannelService.Disconnect(session);
            Sessions.Remove(context);
        }

        public bool Get(IChannelHandlerContext context, out Session session)
        {
            return Sessions.TryGetValue(context, out session);
        }
    }
}
