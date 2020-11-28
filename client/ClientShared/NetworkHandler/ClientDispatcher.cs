using DotNetty.Transport.Channels;
using NetworkShared.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;

namespace ClientShared.NetworkHandler
{
    public abstract class ClientDispatcher
    {
        private static JsonSerializer JsonSerializer = new JsonSerializer();

        protected int UserIndex { get; set; }

        //public bool Call(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header)
        //{
        //    switch (header.Id)
        //    {
        //        case NetworkShared.Protocols.Id.Response.Enter:
        //            return OnEnter(context, header);
        //        case NetworkShared.Protocols.Id.Response.Leave:
        //            return OnLeave(context, header);
        //        case NetworkShared.Protocols.Id.Response.Move:
        //            return OnMove(context, header);
        //        default:
        //            Log.Logger.Error($"Not Impleted Yet Packet: {header.Id}");
        //            return false;
        //    }
        //}

        //protected abstract bool OnEnter(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header);

        //protected abstract bool OnLeave(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header);

        //protected abstract bool OnMove(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header);
    }
}
