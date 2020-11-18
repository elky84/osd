using Protocols.Types;

namespace Protocols.Response
{
    public class Move : Header
    {
        public override Id.Response Id => Protocols.Id.Response.Move;

        public int PlayerIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public DirectionType Direction { get; set; }
    }
}
