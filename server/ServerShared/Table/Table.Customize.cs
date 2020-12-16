using System.Collections.Generic;
using System.Linq;

namespace MasterData.Table
{
    public partial class TableSheet1 : BaseDict<string, Sheet1>
    {
        public Dictionary<string, bool?> Cached { get; private set; } = new Dictionary<string, bool?>();

        public TableSheet1()
        {
            Cached = this.ToDictionary(x => x.Value.name, x => x.Value.value2);
        }
    }

    public partial class TablePortal : BaseDict<string, Portal>
    {
        public List<Portal> Nears(string mapName)
        {
            return this.Values.Where(x => x.BeforeMap == mapName).ToList();
        }
    }
}