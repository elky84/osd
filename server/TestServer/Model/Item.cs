using MasterData;
using MasterData.Table;
using NetworkShared;
using System;

namespace TestServer.Model
{
    public class Item : Object
    {
        public ulong Id { get; set; } // DB 키로 쓰일 예정

        public MasterData.Table.Item Master { get; private set; }

        public override ObjectType Type => ObjectType.Item;

        public static implicit operator FlatBuffers.Protocol.Response.Item.Model(Item item) => new FlatBuffers.Protocol.Response.Item.Model(item.Id, item.Name);

        public override string Name => Master.Id;

        public Item(ulong id, MasterData.Table.Item master)
        {
            Id = id;
            Master = master;
        }
    }

    public class Consume : Item
    {
        public Consume(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            if (Master.Type != ItemType.Consume)
                throw new Exception($"{master.Id} is not a Consume type.");
        }
    }

    public class Equipment : Item
    {
        public static implicit operator FlatBuffers.Protocol.Response.Equipment.Model(Equipment e) => new FlatBuffers.Protocol.Response.Equipment.Model(e.Id, e.Name, (int)e.EquipmentOption.Type);
        public MasterData.Table.EquipmentOption EquipmentOption { get; private set; }

        public Equipment(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            EquipmentOption = MasterTable.From<TableEquipmentOption>()[master.Id] ??
                throw new Exception($"{master.Id} is not defined in 'EquipmentOption' table.");
        }
    }

    public class Weapon : Equipment
    {
        public MasterData.Table.WeaponOption WeaponOption { get; private set; }

        public Weapon(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            WeaponOption = MasterTable.From<TableWeaponOption>()[master.Id] ??
                throw new Exception($"{master.Id} is not defined in 'WeaponOption' table.");

            if(EquipmentOption.Type != EquipmentType.Weapon)
                throw new Exception($"{master.Id} is not a Weapon type.");
        }
    }

    public class Shield : Equipment
    {
        public Shield(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            if(EquipmentOption.Type != EquipmentType.Shield)
                throw new Exception($"{master.Id} is not a Shield type.");
        }
    }

    public class Armor : Equipment
    {
        public Armor(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            if(EquipmentOption.Type != EquipmentType.Armor)
                throw new Exception($"{master.Id} is not a Armor type.");
        }
    }

    public class Shoes : Equipment
    {
        public Shoes(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            if(EquipmentOption.Type != EquipmentType.Shoes)
                throw new Exception($"{master.Id} is not a Shoes type.");
        }
    }

    public class Helmet : Equipment
    {
        public Helmet(ulong id, MasterData.Table.Item master) : base(id, master)
        {
            if(EquipmentOption.Type != EquipmentType.Helmet)
                throw new Exception($"{master.Id} is not a Helmet type.");
        }
    }
}
