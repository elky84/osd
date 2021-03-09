using DotNetty.Transport.Channels;
using NetworkShared;
using System;
using System.Drawing;
using System.Numerics;

namespace TestClient.Model
{
    public class Character
    {
        public IChannelHandlerContext Context { get; set; }

        public int Sequence { get; set; }

        public Direction Direction { get; set; }
        public Vector2 Position { get; set; }
        public DateTime? BeginMoveTime { get; set; }
        public Vector2 Velocity { get; set; }
    }
}
