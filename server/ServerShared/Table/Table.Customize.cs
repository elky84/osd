using System.Collections.Generic;
using System.Linq;

namespace MasterData.Table
{
    public partial class Portal
    {
        public FlatBuffers.Protocol.Portal.Model FlatBuffer => new FlatBuffers.Protocol.Portal.Model(BeforePosition.FlatBuffer, AfterMap);
    }



    public partial class TablePortal : BaseDict<string, Portal>
    {
        public List<Portal> Nears(string mapName)
        {
            return this.Values.Where(x => x.BeforeMap == mapName).ToList();
        }
    }
}