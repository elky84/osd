namespace ServerShared.Service
{
    public class ChannelService
    {
        //private readonly ConcurrentDictionary<int, Channel> Channels = new ConcurrentDictionary<int, Channel>();

        //public MessageWorker MessageWorker { get; set; }

        //public ChannelService()
        //{

        //}

        //public Channel Get(int channelId)
        //{
        //    if (!Channels.TryGetValue(channelId, out Channel channel))
        //    {
        //        Log.Logger.Information("Failed get Channel. {0}", channelId);
        //        return null;
        //    }

        //    return channel;
        //}

        //public void BroadCast(Session session, NetworkShared.Protocols.Response.Header header)
        //{
        //    Get(session.ChannelId)?.BroadCast(header);
        //}


        //public bool Enter(Session session, NetworkShared.Protocols.Request.Enter enter, int channelId = 1)
        //{
        //    if (!Channels.TryGetValue(channelId, out Channel channel))
        //    {
        //        channel = new Channel(channelId, this, MessageWorker);
        //        Channels.TryAdd(channelId, channel);
        //    }

        //    return channel.EnterNewUser(session, enter);
        //}

        //public bool Leave(Session session)
        //{
        //    return Get(session.ChannelId)?.LeaveUser(session) ?? false;
        //}

        //public bool Disconnect(Session session)
        //{
        //    return Get(session.ChannelId)?.Disconnect(session) ?? false;
        //}

        //public bool Move(Session session, Protocols.Request.Move move)
        //{
        //    return Get(session.ChannelId)?.Move(session, move.Direction) ?? false;
        //}
    }
}
