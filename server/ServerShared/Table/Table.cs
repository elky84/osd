using NetworkShared;

namespace Table
{
    [Table("Json/sheet1.json")]
    public class TableSheet1 : BaseList<Sheet1>
    {
        public TableSheet1()
        { }
    }
    
    [Table("Json/sheet23.json")]
    public class TableSheet23 : BaseDict<string, Sheet23>
    {
        public TableSheet23()
        { }
    }
}