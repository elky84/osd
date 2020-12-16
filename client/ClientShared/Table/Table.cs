namespace MasterData.Table
{
    [Table("json/Sheet1.json")]
    public partial class TableSheet1 : BaseDict<string, Sheet1>
    { }
    [Table("json/Sheet23.json")]
    public partial class TableSheet23 : BaseDict<string, Sheet23>
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
}