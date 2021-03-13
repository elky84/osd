using NetworkShared;

namespace TestServer.Model
{
    public class NPC : Object
    {
        public override ObjectType Type => ObjectType.NPC;

        public MasterData.Server.Npc Master { get; private set; }

        public override string Name => Master.Id;

        public NPC(MasterData.Server.Npc master)
        {
            Master = master;
        }
    }
}
