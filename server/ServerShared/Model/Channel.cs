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
    }
}
