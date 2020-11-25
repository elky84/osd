using System.Collections.Generic;
using System.Linq;

namespace NetworkShared.Table
{
    public partial class TableSheet1 : BaseList<Sheet1>
    {
        public Dictionary<string, bool?> Cached { get; private set; } = new Dictionary<string, bool?>();

        public TableSheet1()
        {
            Cached = this.ToDictionary(x => x.Name, x => x.Value2);
        }
    }
}