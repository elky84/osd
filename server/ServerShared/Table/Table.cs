using NetworkShared;
using System.Collections.Generic;

namespace MasterData.Table
{
    [Table("json/Experience.json")]
    public partial class TableExperience : BaseDict<int, Experience>
    { }

    [Table("json/Stat.json")]
    public partial class TableStat : BaseDict<int, Stat>
    { }

    [Table("json/Collision.json")]
    public partial class TableCollision : BaseDict<string, Collision>
    { }

    [Table("json/Consume.json")]
    public partial class TableConsume : BaseDict<string, List<Consume>>
    { }

    [Table("json/Item.json")]
    public partial class TableItem : BaseDict<string, Item>
    { }

    [Table("json/EquipmentOption.json")]
    public partial class TableEquipmentOption : BaseDict<string, EquipmentOption>
    { }

    [Table("json/WeaponOption.json")]
    public partial class TableWeaponOption : BaseDict<string, WeaponOption>
    { }

    [Table("json/ConsumeOption.json")]
    public partial class TableConsumeOption : BaseDict<string, ConsumeOption>
    { }

    [Table("json/WeaponRange.json")]
    public partial class TableWeaponRange : BaseDict<WeaponType, WeaponRange>
    { }

    [Table("json/Portal.json")]
    public partial class TablePortal : BaseDict<string, Portal>
    { }

    [Table("json/Map.json")]
    public partial class TableMap : BaseDict<string, Map>
    { }

    [Table("json/Mob.json")]
    public partial class TableMob : BaseDict<string, Mob>
    { }

    [Table("json/MobSpawn.json")]
    public partial class TableMobSpawn : BaseList<MobSpawn>
    { }

    [Table("json/Npc.json")]
    public partial class TableNpc : BaseDict<string, Npc>
    { }

    [Table("json/Reward.json")]
    public partial class TableReward : BaseDict<string, List<Reward>>
    { }

    [Table("json/Skill.json")]
    public partial class TableSkill : BaseDict<string, Skill>
    { }

    [Table("json/Buff.json")]
    public partial class TableBuff : BaseDict<string, List<Buff>>
    { }

    [Table("json/Passive.json")]
    public partial class TablePassive : BaseDict<string, List<Passive>>
    { }

    [Table("json/SkillProperty.json")]
    public partial class TableSkillProperty : BaseDict<string, List<SkillProperty>>
    { }

}