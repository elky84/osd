using DotNetty.Transport.Channels;
using NetworkShared;
using NetworkShared.NetworkHandler;
using Serilog;
using System;
using TestClient.Model;

namespace TestClient
{
    public partial class GameHandler : BaseHandler<Character>
    {
        public Model.Character Character { get; set; }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        [FlatBufferEvent]
        public bool OnState(FlatBuffers.Protocol.Response.State response)
        {
            Log.Logger.Information($"OnState() {response.Sequence} {response.Position} {response.Jump} {response.Velocity}");
            return true;
        }

        [FlatBufferEvent]
        public bool OnDialog(FlatBuffers.Protocol.Response.ShowDialog response)
        {
            Console.WriteLine($"Enabled next button : {response.Next}");
            Console.WriteLine($"Enabled quit button : {response.Quit}");
            Console.WriteLine($"Message : {response.Message}");

            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(FlatBuffers.Protocol.Response.ShowListDialog response)
        {
            Console.WriteLine($"Message : {response.Message}");
            for (int i = 0; i < response.ListLength; i++)
            {
                var item = response.List(i);
                Console.WriteLine($"item {i} : {item}");
            }
            return true;
        }

        [FlatBufferEvent]
        public bool OnEnter(FlatBuffers.Protocol.Response.Enter response)
        {
            for (int i = 0; i < response.PortalsLength; i++)
            {
                var portal = response.Portals(i).Value;
                Console.WriteLine($"Portal to {portal.Map} : {portal.Position?.X}, {portal.Position?.Y}");
            }

            Console.WriteLine($"My sequence : {response.Character.Value.Sequence}");
            Character.Sequence = response.Character.Value.Sequence;

            Console.WriteLine($"After position : {response.Position?.X}, {response.Position?.Y}");
            Console.WriteLine($"After map name : {response.Map?.Name}");

            return true;
        }

        [FlatBufferEvent]
        public bool OnShow(FlatBuffers.Protocol.Response.Show response)
        {
            for (int i = 0; i < response.ObjectsLength; i++)
            {
                var obj = response.Objects(i).Value;
                Console.WriteLine($"Object {i} : {obj.Name}({obj.Sequence}) => {(ObjectType)obj.Type}");
            }


            for (int i = 0; i < response.CharactersLength; i++)
            {
                var character = response.Characters(i).Value;
                Console.WriteLine($"Object {i} : {character.Name}({character.Sequence}) => {(ObjectType)character.Sequence}");
            }


            return true;
        }

        [FlatBufferEvent]
        public bool OnLeave(FlatBuffers.Protocol.Response.Leave response)
        {
            for (int n = 0; n < response.SequenceLength; ++n)
                Console.WriteLine($"{response.Sequence(n)} is leave from current map.");
            return true;
        }

        [FlatBufferEvent]
        public bool OnItems(FlatBuffers.Protocol.Response.Items response)
        {
            for (int i = 0; i < response.EquipmentLength; i++)
            {
                var equipment = response.Equipment(i);
                var equipmentType = (EquipmentType)equipment?.Type;
                Console.WriteLine($"{equipmentType} : {equipment?.Name}");
            }

            for (int i = 0; i < response.InventoryLength; i++)
            {
                var item = response.Inventory(i);
                Console.WriteLine($"inventory {item?.Id} : {item?.Name}");
            }

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
