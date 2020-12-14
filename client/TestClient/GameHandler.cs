using DotNetty.Transport.Channels;
using FlatBuffers.Protocol;
using NetworkShared.NetworkHandler;
using Serilog;
using System;
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
        public bool OnMove(Move response)
        {
            Log.Logger.Information($"OnMove() {response.Position.Value.X} {response.Position.Value.Y} {response.Direction} {response.Now}");
            return true;
        }

        [FlatBufferEvent]
        public bool OnStop(Stop response)
        {
            Log.Logger.Information($"OnStop() {response.Position.Value.X} {response.Position.Value.Y} {response.Now}");
            return true;
        }

        [FlatBufferEvent]
        public bool OnClick(Click response)
        {
            Log.Logger.Information($"OnClick()");
            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(SelectListDialog response)
        {
            Log.Logger.Information($"OnSelectListDialog()");
            return true;
        }

        [FlatBufferEvent]
        public bool OnEnter(Enter response)
        {
            for (int i = 0; i < response.PortalsLength; i++)
            {
                var portal = response.Portals(i).Value;
                Console.WriteLine($"Portal to {portal.Map} : {portal.Position?.X}, {portal.Position?.Y}");
            }

            for (int i = 0; i < response.ObjectsLength; i++)
            {
                var obj = response.Objects(i);
                Console.WriteLine($"Object {i} : {obj?.Name}({obj?.Sequence})");
            }

            Console.WriteLine($"My sequence : {response.Sequence}");
            Console.WriteLine($"After position : {response.Position.Value.X}, {response.Position.Value.Y}");
            Console.WriteLine($"After map name : {response.Map?.Name}");
            
            return true;
        }

        [FlatBufferEvent]
        public bool OnShow(Show response)
        {
            Console.WriteLine($"{response.Name}({response.Sequence}) is entered in current map.");
            return true;
        }

        [FlatBufferEvent]
        public bool OnLeave(Leave response)
        {
            Console.WriteLine($"{response.Sequence} is leave from current map.");
            return true;
        }

        protected override void OnConnected(IChannelHandlerContext context)
        {
            Character = new Character { Context = context };
        }

        protected override void OnDisconnected(IChannelHandlerContext context)
        {
        }
    }
}
