using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkShared.Protocols.Request
{
    public class Leave : Header
    {
        public override Id.Request Id => Protocols.Id.Request.Leave;

    }
}
