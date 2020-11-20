using ClientShared.Config;
using ClientShared.NetworkHandler;
using DotNetty.Transport.Channels;
using Serilog;
using System.Threading;

namespace TestClient.Handler
{
    class PacketDispatcher : ClientDispatcher
    {
        protected override bool OnEnter(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header)
        {
            var enter = PopulateFromExtensionData<NetworkShared.Protocols.Response.Enter>(header.ExtensionData);
            Log.Logger.Information($"User: {enter.Index} Position: {enter.X}, {enter.Y}");

            if (enter.Name == ClientSettings.UserName)
            {
                UserIndex = enter.Index;
            }

            SendRandomMove(context);
            return true;
        }

        protected override bool OnLeave(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header)
        {
            var leave = PopulateFromExtensionData<NetworkShared.Protocols.Response.Leave>(header.ExtensionData);
            Log.Logger.Information($"User: {leave.UserIndex}");
            return true;
        }

        protected override bool OnMove(IChannelHandlerContext context, NetworkShared.Protocols.Response.Header header)
        {
            var move = PopulateFromExtensionData<NetworkShared.Protocols.Response.Move>(header.ExtensionData);
            Log.Logger.Information($"User: {move.PlayerIndex} Position: {move.X}, {move.Y}");

            // 패킷 핑퐁을 위한 테스트 코드. 
            Thread.Sleep(500);
            SendRandomMove(context);
            return true;
        }
    }
}
