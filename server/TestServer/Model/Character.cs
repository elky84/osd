using DotNetty.Transport.Channels;
using FlatBuffers.Protocol;
using KeraLua;
using NetworkShared;
using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestServer.Model
{
    public class ItemCollection
    { 
        public Character Owner { get; private set; }

        public Dictionary<ItemType, List<Item>> Inventory { get; private set; } = new Dictionary<ItemType, List<Item>>();
        public Dictionary<EquipmentType, Equipment> Equipments { get; private set; } = new Dictionary<EquipmentType, Equipment>();

        public ItemCollection(Character owner)
        {
            Owner = owner;

            foreach (var itemType in Enum.GetValues(typeof(ItemType)).Cast<ItemType>())
                Inventory.Add(itemType, new List<Item>());

            foreach (var equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                Equipments.Add(equipmentType, null);
        }

        public T Equip<T>(T equipment) where T : Equipment
        {
            var before = Equipments[equipment.EquipmentOption.Type];
            Equipments[equipment.EquipmentOption.Type] = equipment;

            Inventory.Remove(equipment);

            if (before != null)
                Inventory.Add(before);

            return before as T;
        }

        public Weapon Weapon
        {
            get => Equipments[EquipmentType.Weapon] as Weapon;
            set => Equip(value);
        }
        
        public Shield Shield
        {
            get => Equipments[EquipmentType.Shield] as Shield;
            set => Equip(value);
        }
        
        public Armor Armor
        {
            get => Equipments[EquipmentType.Armor] as Armor;
            set => Equip(value);
        }
        
        public Shoes Shoes
        {
            get => Equipments[EquipmentType.Shoes] as Shoes;
            set => Equip(value);
        }
        
        public Helmet Helmet
        {
            get => Equipments[EquipmentType.Helmet] as Helmet;
            set => Equip(value);
        }
    }

    public class Character : Life
    {
        public new interface IListener : Life.IListener
        { }

        public new IListener Listener { get; private set; }

        public IChannelHandlerContext Context { get; set; }

        public ItemCollection Items { get; private set; }

        public Lua DialogThread { get; set; }

        public int Damage { get; set; } = 30;

        public override ObjectType Type => ObjectType.Character;

        public Character()
        {
            Items = new ItemCollection(this);
        }

        public static int BuiltinDamage(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            lua.PushInteger(character.Damage);
            return 1;
        }

        public static int BuiltinYield(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            character.Damage = 10;
            return lua.Yield(1);
        }

        public static int BuiltinDialog(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var argc = lua.GetTop();
            var character = lua.ToLuable<Character>(1);
            var obj = lua.ToLuable<Object>(2);
            var message = lua.ToString(3);
            var next = argc >= 4 ? lua.ToBoolean(4) : true;
            var quit = argc >= 5 ? lua.ToBoolean(5) : true;

            _ = character.Context.Send(ShowDialog.Bytes(message, next, quit));
            return lua.Yield(1);
        }

        public static int BuiltinDialogList(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var obj = lua.ToLuable<Object>(2);
            var message = lua.ToString(3);
            var list = lua.ToStringList(4);

            _ = character.Context.Send(ShowListDialog.Bytes(message, list));
            return lua.Yield(1);
        }

        public static int BuiltinItems(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);

            lua.NewTable();
            foreach (var (item, i) in character.Items.Inventory.SelectMany(x => x.Value).Select((x, i) => (x, i)))
            {
                lua.PushLuable(item);
                lua.RawSetInteger(-2, i + 1);
            }

            return 1;
        }

        public static int BuiltinItemAdd(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var item = lua.ToLuable<Item>(2);

            character.Items.Inventory.Add(item);
            lua.PushLuable(item);
            return 1;
        }

        public void BindEvent(IListener listener)
        {
            base.BindEvent(listener);
            Listener = listener;
        }
    }

    public static class ItemCollectionExtension
    {
        public static Item Add(this Dictionary<ItemType, List<Item>> inventory, Item item)
        {
            inventory[item.Master.Type].Add(item);
            return item;
        }

        public static Item Remove(this Dictionary<ItemType, List<Item>> inventory, Item item)
        {
            inventory[item.Master.Type].Remove(item);
            return item;
        }
    }
}
