﻿using DotNetty.Transport.Channels;
using NetworkShared.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NetworkShared.Protocols.Types;
using Serilog;
using System;

namespace ClientShared.NetworkHandler
{
    public abstract class ClientDispatcher
    {
        private static JsonSerializer JsonSerializer = new JsonSerializer();

        protected int UserIndex { get; set; }

        public bool Call(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header)
        {
            switch (header.Id)
            {
                case NetworkShared.Protocols.Id.Response.Enter:
                    return OnEnter(context, header);
                case NetworkShared.Protocols.Id.Response.Leave:
                    return OnLeave(context, header);
                case NetworkShared.Protocols.Id.Response.Move:
                    return OnMove(context, header);
                default:
                    Log.Logger.Error($"Not Impleted Yet Packet: {header.Id}");
                    return false;
            }
        }

        static protected T PopulateFromExtensionData<T>(JObject extensionData) where T : new()
        {
            var value = new T();
            if (extensionData != null)
            {
                JsonSerializer.Populate(extensionData.CreateReader(), value);
            }
            return value;
        }


        protected void SendRandomMove(IChannelHandlerContext context)
        {
            Random rand = new Random();
            var direction = (DirectionType)(rand.Next() % 8);
            context.WriteAsync(new NetworkShared.Protocols.Request.Move { Direction = direction }.ToByteBuffer());
        }

        protected abstract bool OnEnter(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header);

        protected abstract bool OnLeave(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header);

        protected abstract bool OnMove(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header);
    }
}
