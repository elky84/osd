namespace NetworkShared.Table
{
    [Table("Json/sheet1.json")]
    public partial class TableSheet1 : BaseList<Sheet1>
    { }
    
    [Table("Json/sheet23.json")]
    public partial class TableSheet23 : BaseDict<string, Sheet23>
    { }
}