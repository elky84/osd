using NetworkShared;

namespace MasterData.Table
{
    [Table("json/Collision.json")]
    public partial class TableCollision : BaseDict<string, Collision>
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
    public partial class TableReward : BaseDict<string, Reward>
    { }

    [Table("json/Skill.json")]
    public partial class TableSkill : BaseDict<string, Skill>
    { }

}