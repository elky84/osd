using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkShared.Protocols.Response
{
    public class Leave : Header
    {
        public override Id.Response Id => Protocols.Id.Response.Leave;

        public int UserIndex { get; set; }

    }
}
