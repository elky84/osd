using NetworkShared;

namespace TestServer.Model
{
    public class NPC : Object
    {
        public override ObjectType Type => ObjectType.NPC;

        public string Script { get; set; }
    }
}
