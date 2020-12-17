using MasterData;
using MasterData.Table;
using NetworkShared;
using System;

namespace TestServer.Model
{
    public class Item : Object
    {
        public MasterData.Table.Item Master { get; private set; }

        public override ObjectType Type => ObjectType.Item;

        public override string Name => Master.Id;

        public Item(MasterData.Table.Item master)
        {
            Master = master;
        }
    }

    public class Consume : Item
    {
        public Consume(MasterData.Table.Item master) : base(master)
        {
            if (Master.Type != ItemType.Consume)
                throw new Exception($"{master.Id} is not a Consume type.");
        }
    }

    public class Equipment : Item
    { 
        public MasterData.Table.EquipmentOption EquipmentOption { get; private set; }

        public Equipment(MasterData.Table.Item master) : base(master)
        {
            EquipmentOption = MasterTable.From<TableEquipmentOption>()[master.Id] ??
                throw new Exception($"{master.Id} is not defined in 'EquipmentOption' table.");
        }
    }

    public class Weapon : Equipment
    {
        public MasterData.Table.WeaponOption WeaponOption { get; private set; }

        public Weapon(MasterData.Table.Item master) : base(master)
        {
            WeaponOption = MasterTable.From<TableWeaponOption>()[master.Id] ??
                throw new Exception($"{master.Id} is not defined in 'WeaponOption' table.");

            if(EquipmentOption.Type != EquipmentType.Weapon)
                throw new Exception($"{master.Id} is not a Weapon type.");
        }
    }

    public class Shield : Equipment
    {
        public Shield(MasterData.Table.Item master) : base(master)
        {
            if(EquipmentOption.Type != EquipmentType.Shield)
                throw new Exception($"{master.Id} is not a Shield type.");
        }
    }

    public class Armor : Equipment
    {
        public Armor(MasterData.Table.Item master) : base(master)
        {
            if(EquipmentOption.Type != EquipmentType.Armor)
                throw new Exception($"{master.Id} is not a Armor type.");
        }
    }

    public class Shoes : Equipment
    {
        public Shoes(MasterData.Table.Item master) : base(master)
        {
            if(EquipmentOption.Type != EquipmentType.Shoes)
                throw new Exception($"{master.Id} is not a Shoes type.");
        }
    }

    public class Helmet : Equipment
    {
        public Helmet(MasterData.Table.Item master) : base(master)
        {
            if(EquipmentOption.Type != EquipmentType.Helmet)
                throw new Exception($"{master.Id} is not a Helmet type.");
        }
    }
}
