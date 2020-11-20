using NetworkShared.Protocols.Types;
using System;
using Newtonsoft.Json.Linq;
using Serilog;
using ServerShared.Util;

namespace ServerShared.Model
{
    public partial class Channel
    {

        public bool EnterNewUser(Session session, NetworkShared.Protocols.Request.Enter enter)
        {
            session.Index = AcquirePlayerIndex();

            session.X = 10;
            session.Y = 10;

            session.UserName = enter.UserName;

            Add(session);

            BroadCast(new NetworkShared.Protocols.Response.Enter { Index = session.Index, X = session.X, Y = session.Y, Name = enter.UserName });
            return true;
        }


        public bool LeaveUser(Session session)
        {
            _ = session.Send(new NetworkShared.Protocols.Response.Leave { UserIndex = session.Index });

            Remove(session);
            return true;
        }

        public bool Disconnect(Session session)
        {
            Remove(session);
            return true;
        }


        public bool Move(Session session, DirectionType direction)
        {
            switch (direction)
            {
                case DirectionType.Down:
                    session.Y -= 5;
                    break;
                case DirectionType.Up:
                    session.Y += 5;
                    break;
                case DirectionType.Left:
                    session.X -= 5;
                    break;
                case DirectionType.Right:
                    session.X += 5;
                    break;
            }

            BroadCast(new NetworkShared.Protocols.Response.Move { PlayerIndex = session.Index, X = session.X, Y = session.Y, Direction = direction });
            return true;
        }
    }
}