namespace NetworkShared.Table
{
    [Table("json/Sheet1.json")]
    public partial class TableSheet1 : BaseDict<string, Sheet1>
    { }
    [Table("json/Sheet23.json")]
    public partial class TableSheet23 : BaseDict<string, Sheet23>
    { }
    [Table("json/Portal.json")]
    public partial class TablePortal : BaseDict<string, Portal>
    { }
    [Table("json/Npc.json")]
    public partial class TableNpc : BaseDict<string, Npc>
    { }
}