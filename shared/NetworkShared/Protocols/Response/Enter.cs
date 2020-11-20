using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkShared.Protocols.Response
{
    public class Enter : Header
    {
        public override Id.Response Id => Protocols.Id.Response.Enter;

        public int Index { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public string Name { get; set; }
    }
}
