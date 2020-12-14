using DotNetty.Transport.Channels;
using NetworkShared;
using System.Drawing;

namespace TestClient.Model
{
    public class Character
    {
        public IChannelHandlerContext Context { get; set; }

        public Direction Direction { get; set; }
        public Point Position { get; set; }
    }
}
