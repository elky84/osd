using LightInject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ServerShared.Service;
using ServerShared.Worker;

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
            //MessageWorker.MessageCallback = Call;
            MessageWorker.Start();
        }

        public void Push(Message message)
        {
            MessageWorker.Push(message);
        }

        //public bool Call(Message message)
        //{
        //    try
        //    {
        //        //switch (message.Header.Id)
        //        //{
        //        //    case Protocols.Id.Request.Enter:
        //        //        return ChannelService.Enter(message.Session, PopulateFromExtensionData<Protocols.Request.Enter>(message.Header.ExtensionData));
        //        //    case Protocols.Id.Request.Leave:
        //        //        return ChannelService.Leave(message.Session);
        //        //    case Protocols.Id.Request.Move:
        //        //        return ChannelService.Move(message.Session, PopulateFromExtensionData<Protocols.Request.Move>(message.Header.ExtensionData));
        //        //    default:
        //        //        throw new System.Exception($"Not Implemented Yet. Not Defined PacketHandler. {message.Header.Id}");
        //        //}
        //    }
        //    catch (System.Exception e)
        //    {
        //        //TODO Send Error to Client
        //        Log.Logger.Error($"Dispatching Exception. {message.Header.Id} {e.Message} {e.StackTrace}");
        //        return false;
        //    }
        //}

        //static private T PopulateFromExtensionData<T>(JObject extensionData) where T : new()
        //{
        //    var value = new T();
        //    if (extensionData != null)
        //    {
        //        JsonSerializer.Populate(extensionData.CreateReader(), value);
        //    }
        //    return value;
        //}
    }
}
