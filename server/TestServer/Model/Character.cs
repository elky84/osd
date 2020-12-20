using DotNetty.Transport.Channels;
using FlatBuffers.Protocol;
using KeraLua;
using NetworkShared;
using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            //TODO 승현님 Equipments.Add(equipmentType, null)에서 null로 인해 오류가 납니다.
            foreach (var equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                Equipments.Add(equipmentType, null);
        }

        public Equipment Equip(Equipment equipment)
        {
            var before = Equipments[equipment.EquipmentOption.Type];
            Equipments[equipment.EquipmentOption.Type] = equipment;

            Inventory.Remove(equipment);

            if (before != null)
                Inventory.Add(before);

            return before;
        }

        public Equipment Unequip(Equipment equipment)
        {
            var found = Equipments.Values.FirstOrDefault(x => x == equipment);
            if (found == null)
                return null;

            Equipments[found.EquipmentOption.Type] = null;
            Inventory.Add(found);
            return found;
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

        public Item Active(ulong id)
        {
            var found = Inventory.SelectMany(x => x.Value).FirstOrDefault(x => x.Id == id);
            if (found == null)
                return null;

            if (found.Master.Type == ItemType.Equipment)
            {
                var equipment = found as Equipment;
                Equip(equipment);
            }

            if (File.Exists(found.Master.ActiveScript))
            {
                Owner.LuaThread = Static.Main.NewThread();
                Owner.LuaThread.Encoding = Encoding.UTF8;
                Owner.LuaThread.DoFile(found.Master.ActiveScript);
                Owner.LuaThread.GetGlobal("func");

                Owner.LuaThread.PushLuable(Owner);
                Owner.LuaThread.PushLuable(found);
                Owner.LuaThread.Resume(2);
            }
            return found;
        }

        public Item Inactive(ulong id)
        {
            var found = Equipments.Values.FirstOrDefault(x => x?.Id == id);
            if (found == null)
                return null;

            Unequip(found);
            if (File.Exists(found.Master.InactiveScript))
            {
                Owner.LuaThread = Static.Main.NewThread();
                Owner.LuaThread.Encoding = Encoding.UTF8;
                Owner.LuaThread.DoFile(found.Master.InactiveScript);
                Owner.LuaThread.GetGlobal("func");

                Owner.LuaThread.PushLuable(Owner);
                Owner.LuaThread.PushLuable(found);
                Owner.LuaThread.Resume(2);
            }
            return found;
        }
    }

    public class Character : Life
    {
        public new interface IListener : Life.IListener
        { }

        public new IListener Listener { get; private set; }

        public IChannelHandlerContext Context { get; set; }

        public ItemCollection Items { get; private set; }

        public Lua LuaThread { get; set; }

        public int Damage { get; set; } = 30;

        public override ObjectType Type => ObjectType.Character;

        public FlatBuffers.Protocol.ShowCharacter.Model ShowCharacterFlatBuffer => new ShowCharacter.Model(Sequence.Value, Name, Position.FlatBuffer, Items.Equipments.Values.Where(x => x != null).Select(x => x.EquipmentFlatBuffer).ToList());

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

        public static int BuiltinItemRemove(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var item = lua.ToLuable<Item>(2);

            character.Items.Inventory.Remove(item);
            lua.PushLuable(item);
            return 1;
        }

        public static int BuiltinFindItem(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var name = lua.ToString(2);

            var found = character.Items.Inventory.SelectMany(x => x.Value).FirstOrDefault(x => x.Name == name) ??
                character.Items.Equipments.Values.FirstOrDefault(x => x?.Name == name);
            if (found != null)
                lua.PushLuable(found);
            else
                lua.PushNil();

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
