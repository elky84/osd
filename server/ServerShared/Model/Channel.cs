using System.Collections.Generic;
using ServerShared.Worker;
using ServerShared.Service;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerShared.Model
{
    public partial class Channel
    {
        public int ChannelId { get; }

        private int PlayerIndexCounter;

        private readonly Dictionary<string, Session> Sessions = new Dictionary<string, Session>();

        private MessageWorker MessageWorker { get; set; }

        private ChannelService ChannelService { get; set; }

        private static JsonSerializer JsonSerializer = new JsonSerializer();

        public Channel(int channelId, ChannelService channelService, MessageWorker messageWorker)
        {
            this.ChannelId = channelId;
            this.MessageWorker = messageWorker;
            this.ChannelService = channelService;
        }

        private int AcquirePlayerIndex()
        {
            return ++PlayerIndexCounter;
        }

        static private T PopulateFromExtensionData<T>(JObject extensionData) where T : new()
        {
            var value = new T();
            if (extensionData != null)
            {
                JsonSerializer.Populate(extensionData.CreateReader(), value);
            }
            return value;
        }

        private Session GetSessionByIndex(int index)
        {
            return Sessions.Values.FirstOrDefault(x => x.Index == index);
        }

        private void Add(Session session)
        {
            Sessions.Add(session.GetSessionId(), session);
            session.ChannelId = ChannelId;
        }

        private void Remove(Session session)
        {
            Sessions.Remove(session.GetSessionId());
            BroadCast(new NetworkShared.Protocols.Response.Leave { UserIndex = session.Index });
        }

        public void BroadCast(NetworkShared.Protocols.Response.Header header)
        {
            foreach (var pair in Sessions)
            {
                _ = pair.Value.Send(header);
            }
        }
    }
}