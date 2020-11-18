using Protocols.Types;

namespace Protocols.Request
{
    public class Move : Header
    {
        public override Id.Request Id => Protocols.Id.Request.Move;

        public DirectionType Direction { get; set; }
    }
}
