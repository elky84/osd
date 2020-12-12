using DotNetty.Transport.Channels;
using FlatBuffers.Protocol;
using NetworkShared.NetworkHandler;
using Serilog;
using TestClient.Model;

namespace TestClient
{
    public partial class GameHandler : BaseHandler<Character>
    {
        public Character Character { get; set; }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        [FlatBufferEvent]
        public bool OnMove(Move x)
        {
            Log.Logger.Information($"OnMove() {x.Position.Value.X} {x.Position.Value.Y} {x.Direction} {x.Now}");
            return true;
        }

        [FlatBufferEvent]
        public bool OnStop(Stop x)
        {
            Log.Logger.Information($"OnStop() {x.Position.Value.X} {x.Position.Value.Y} {x.Now}");
            return true;
        }

        [FlatBufferEvent]
        public bool OnClick(Click x)
        {
            Log.Logger.Information($"OnClick()");
            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(SelectListDialog x)
        {
            Log.Logger.Information($"OnSelectListDialog()");
            return true;
        }

        protected override void OnConnected(IChannelHandlerContext context)
        {
            Command("move", "Top");
            Command("stop");

            Character = new Character { Context = context };

            Send(Click.Bytes(0));
            Send(SelectListDialog.Bytes(3));
        }

        protected override void OnDisconnected(IChannelHandlerContext context)
        {
        }
    }
}
