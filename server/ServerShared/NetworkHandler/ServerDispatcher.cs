using LightInject;
using ServerShared.Service;
using ServerShared.Model;
using ServerShared.Worker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerShared.Util;
using Serilog;

namespace ServerShared.NetworkHandler
{
    public class ServerDispatcher
    {
        [Inject]
        public ChannelService ChannelService { get; set; }

        public MessageWorker MessageWorker { get; set; } = new MessageWorker();

        private static JsonSerializer JsonSerializer = new JsonSerializer();

        public ServerDispatcher()
        {
            MessageWorker.MessageCallback = Call;
            MessageWorker.Start();
        }

        public void Push(Message message)
        {
            MessageWorker.Push(message);
        }

        public bool Call(Message message)
        {
            try
            {
                var ret = Dispatching(message);
                return ret.HasValue && ret.Value;
            }
            catch (System.Exception e)
            {
                //TODO Send Error to Client
                Log.Logger.Error($"Dispatching Exception. {message.Header.Id} {e.Message} {e.StackTrace}");
                return false;
            }
        }

        private bool? Dispatching(Message message)
        {
            switch (message.Header.Id)
            {
                case NetworkShared.Protocols.Id.Request.Enter:
                    return ChannelService.Enter(message.Session, PopulateFromExtensionData<NetworkShared.Protocols.Request.Enter>(message.Header.ExtensionData));
                case NetworkShared.Protocols.Id.Request.Leave:
                    return ChannelService.Leave(message.Session);
                case NetworkShared.Protocols.Id.Request.Move:
                    return ChannelService.Move(message.Session, PopulateFromExtensionData<NetworkShared.Protocols.Request.Move>(message.Header.ExtensionData));
                default:
                    Log.Logger.Error($"Not Implemented Yet. Not Defined PacketHandler. {message.Header.Id}");
                    return false;
            }
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
    }
}
