using System.Collections.Generic;

namespace ServerShared.Model
{
    public class BaseSession
    {
        public List<byte> Buffer { get; private set; } = new List<byte>();

        protected BaseSession()
        {

        }
    }
}
