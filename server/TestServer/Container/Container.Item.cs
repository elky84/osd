using KeraLua;
using NetworkShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TestServer.Model;

namespace TestServer.Container
{
    public class ItemContainer
    {
        public Character Owner { get; private set; }

        public Dictionary<ItemType, List<Model.Item>> Inventory { get; private set; } = new Dictionary<ItemType, List<Model.Item>>();
        public Dictionary<EquipmentType, Equipment> Equipments { get; private set; } = new Dictionary<EquipmentType, Equipment>();

        public ItemContainer(Character owner)
        {
            Owner = owner;

            foreach (var itemType in Enum.GetValues(typeof(ItemType)).Cast<ItemType>())
                Inventory.Add(itemType, new List<Model.Item>());

            foreach (var equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                Equipments.Add(equipmentType, null);
        }

        private void SetAdditionalStats(Equipment equipment)
        {
            Owner.Stats.Additional[StatType.HP] += equipment.EquipmentOption.HP;
            Owner.Stats.Additional[StatType.MP] += equipment.EquipmentOption.MP;
            Owner.Stats.Additional[StatType.Defence] += equipment.EquipmentOption.Defence;
            if (equipment.EquipmentOption.Type == EquipmentType.Weapon)
            {
                var weapon = equipment as Weapon;
                Owner.Stats.Additional[StatType.AttackSpeed] += weapon.WeaponOption.AttackSpeed;
                Owner.Stats.Additional[StatType.PhysicalDamage] += weapon.WeaponOption.PhysicalDamage;
                Owner.Stats.Additional[StatType.MagicalDamage] += weapon.WeaponOption.MagicalDamage;
                Owner.Stats.Additional[StatType.Critical] += weapon.WeaponOption.Critical;
                Owner.Stats.Additional[StatType.CriticalDamage] += weapon.WeaponOption.CriticalDamage;
            }
        }

        private void UnsetAdditionalStats(Equipment equipment)
        {
            Owner.Stats.Additional[StatType.HP] -= equipment.EquipmentOption.HP;
            Owner.Stats.Additional[StatType.MP] -= equipment.EquipmentOption.MP;
            Owner.Stats.Additional[StatType.Defence] -= equipment.EquipmentOption.Defence;

            if (equipment.EquipmentOption.Type == EquipmentType.Weapon)
            {
                var weapon = equipment as Weapon;
                Owner.Stats.Additional[StatType.AttackSpeed] -= weapon.WeaponOption.AttackSpeed;
                Owner.Stats.Additional[StatType.PhysicalDamage] -= weapon.WeaponOption.PhysicalDamage;
                Owner.Stats.Additional[StatType.MagicalDamage] -= weapon.WeaponOption.MagicalDamage;
                Owner.Stats.Additional[StatType.Critical] -= weapon.WeaponOption.Critical;
                Owner.Stats.Additional[StatType.CriticalDamage] -= weapon.WeaponOption.CriticalDamage;
            }
        }

        public Equipment Equip(Equipment equipment)
        {
            var before = Equipments[equipment.EquipmentOption.Type];
            Equipments[equipment.EquipmentOption.Type] = equipment;
            Inventory.Remove(equipment);
            SetAdditionalStats(equipment);

            if (before != null)
            {
                UnsetAdditionalStats(before);
                Inventory.Add(before);
                Owner.Listener?.OnItemAdded(this.Owner, before);
            }

            Owner.Listener?.OnEquipmentChanged(this.Owner, equipment.EquipmentOption.Type);
            Owner.Listener?.OnItemRemoved(this.Owner, equipment);

            return before;
        }

        public Equipment Unequip(Equipment equipment)
        {
            var found = Equipments.Values.FirstOrDefault(x => x == equipment);
            if (found == null)
                return null;

            Equipments[found.EquipmentOption.Type] = null;
            UnsetAdditionalStats(found);
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

        public ulong Gold { get; set; }

        public Model.Item Active(ulong id)
        {
            var found = Inventory.SelectMany(x => x.Value).FirstOrDefault(x => x.Id == id);
            if (found == null)
                return null;

            found.Active(this.Owner);

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

        public Model.Item Inactive(ulong id)
        {
            var found = Equipments.Values.FirstOrDefault(x => x?.Id == id);
            if (found == null)
                return null;

            found.Inactive(this.Owner);
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
}
