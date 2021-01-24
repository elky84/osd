using DotNetty.Transport.Channels;
using FlatBuffers.Protocol.Response;
using NetworkShared;
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
        public bool OnState(State response)
        {
            Log.Logger.Information($"OnState() {response.Sequence} {response.Position} {response.Jumping} {response.Velocity}");
            return true;
        }

        [FlatBufferEvent]
        public bool OnDialog(ShowDialog response)
        {
            Console.WriteLine($"Enabled next button : {response.Next}");
            Console.WriteLine($"Enabled quit button : {response.Quit}");
            Console.WriteLine($"Message : {response.Message}");

            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(ShowListDialog response)
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
        public bool OnEnter(Enter response)
        {
            for (int i = 0; i < response.PortalsLength; i++)
            {
                var portal = response.Portals(i).Value;
                Console.WriteLine($"Portal to {portal.Map} : {portal.Position?.X}, {portal.Position?.Y}");
            }

            for (int i = 0; i < response.ObjectsLength; i++)
            {
                var obj = response.Objects(i).Value;
                Console.WriteLine($"Object {i} : {obj.Name}({obj.Sequence}) => {(ObjectType)obj.Type}");
            }

            Console.WriteLine($"My sequence : {response.Sequence}");
            Console.WriteLine($"After position : {response.Position?.X}, {response.Position?.Y}");
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
        public bool OnShowCharacter(ShowCharacter response)
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

        [FlatBufferEvent]
        public bool OnItems(Items response)
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
