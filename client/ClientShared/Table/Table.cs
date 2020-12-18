namespace MasterData.Table
{
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
    [Table("json/Map.json")]
    public partial class TableMap : BaseDict<string, Map>
    { }
    [Table("json/Mob.json")]
    public partial class TableMob : BaseDict<string, Mob>
    { }
    [Table("json/Npc.json")]
    public partial class TableNpc : BaseDict<string, Npc>
    { }
    [Table("json/Reward.json")]
    public partial class TableReward : BaseDict<string, Reward>
    { }
}