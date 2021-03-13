using DotNetty.Transport.Channels;
using KeraLua;
using MasterData;
using MasterData.Server;
using NetworkShared;
using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestServer.Container;

namespace TestServer.Model
{
    public class Character : Life
    {
        public new interface IListener : Life.IListener
        {
            public void OnEquipmentChanged(Character character, EquipmentType equipmentType);
            public void OnItemAdded(Character character, Item item);
            public void OnItemRemoved(Character character, Item item);
            public void OnLevelChanged(Character character, int before, int after);
            public void OnExpChanged(Character character, long before, long after);
        }

        public new IListener Listener { get; private set; }
        public IChannelHandlerContext Context { get; set; }

        public ItemContainer Items { get; private set; }
        public SkillCollection Skills { get; private set; }

        private DateTime _lastDamagedTime = DateTime.MinValue;
        public override int Hp
        {
            get => base.Hp;
            protected set
            {
                var isDamaged = (value < 0);
                if (isDamaged)
                {
                    var now = DateTime.Now;
                    var elapsed = now - this._lastDamagedTime;
                    // TODO : 테이블에서 관리
                    if (elapsed.TotalMilliseconds < 1000)
                        return;

                    this._lastDamagedTime = now;
                }

                base.Hp = value;
            }
        }

        private int _mp;
        public int Mp
        {
            get => _mp;
            set
            {
                _mp = Math.Clamp(value, 0, BaseMp);
            }
        }

        public int BaseMp => 100;

        public MasterData.Common.Stat BaseStat => MasterTable.From<TableStat>()[Level];

        public Lua LuaThread { get; set; }

        public new int Damage { get; set; } = 30;

        public override ObjectType Type => ObjectType.Character;

        private int _level = 1;
        public int Level
        {
            get => _level;
            set
            {
                var before = _level;
                _level = value;

                Listener?.OnLevelChanged(this, before, _level);
            }
        }

        private long _exp;
        public long Exp
        {
            get => _exp;
            set
            {
                var incLevel = 0;
                var beforeExp = _exp;

                _exp = value;
                var experienceTable = MasterData.MasterTable.From<TableExperience>();

                while (true)
                {
                    var experienceCase = experienceTable[Level];
                    if (experienceCase == null)
                        break;

                    if (_exp < experienceCase.Value)
                        break;

                    _exp -= experienceCase.Value;
                    incLevel++;
                }

                Level += incLevel;

                if (beforeExp != _exp)
                    Listener?.OnExpChanged(this, beforeExp, _exp);
            }
        }

        public static implicit operator FlatBuffers.Protocol.Response.Character.Model(Character obj) =>
            obj.ToProtocol();

        public new FlatBuffers.Protocol.Response.Character.Model ToProtocol() =>
            new FlatBuffers.Protocol.Response.Character.Model(this.Sequence.Value,
                this.Name,
                this.Position,
                this.Moving,
                (int)this.Direction,
                this.Items.Equipments.Values.Where(x => x != null).Select(x => (FlatBuffers.Protocol.Response.Equipment.Model)x).ToList());

        public Character()
        {
            Items = new ItemContainer(this);
            Skills = new SkillCollection(this);
        }

        public override async Task Send(byte[] bytes)
        {
            await Context.Send(bytes);
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

            _ = character.Context.Send(FlatBuffers.Protocol.Response.ShowDialog.Bytes(message, next, quit));
            return lua.Yield(1);
        }

        public static int BuiltinDialogList(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var obj = lua.ToLuable<Object>(2);
            var message = lua.ToString(3);
            var list = lua.ToStringList(4);

            _ = character.Context.Send(FlatBuffers.Protocol.Response.ShowListDialog.Bytes(message, list));
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

        public static int BuiltinExp(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var argc = lua.GetTop();
            var character = lua.ToLuable<Character>(1);

            if (argc == 1)
            {
                lua.PushInteger(character.Exp);
                return 1;
            }
            else
            {
                var exp = lua.ToInteger(2);
                character.Exp = exp;
                return 0;
            }
        }

        public static int BuiltinLevel(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var argc = lua.GetTop();
            var character = lua.ToLuable<Character>(1);

            if (argc == 1)
            {
                lua.PushInteger(character.Exp);
                return 1;
            }
            else
            {
                var level = lua.ToInteger(2);
                character.Level = (int)level;
                return 0;
            }
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
            if (item.Master.Stack.HasValue)
            {
                var exists = inventory[item.Master.Type].FirstOrDefault(x => x.Master.Id == item.Master.Id);
                if (exists == null)
                {
                    inventory[item.Master.Type].Add(item);
                }
                else if (exists.Count + item.Count < item.Master.Stack.Value)
                {
                    exists.Count += item.Count;
                }
                else
                {
                    var free = item.Master.Stack.Value - exists.Count;
                    exists.Count += free;
                    item.Count -= free;

                    inventory.Add(item);
                }
            }
            else
            {
                inventory[item.Master.Type].Add(item);
            }

            return item;
        }

        public static Item Remove(this Dictionary<ItemType, List<Item>> inventory, Item item)
        {
            inventory[item.Master.Type].Remove(item);
            return item;
        }
    }
}
