using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerShared.Model;

namespace ServerShared.Worker
{
    public class Message
    {
        public Session Session { get; set; }

        public Protocols.Request.Header Header { get; set; }
    }

    public class MessageWorker
    {
        private Thread GlobalThread { get; set; }

        private Thread ChannelThread { get; set; }

        public delegate bool Callback(Message message);

        public Callback MessageCallback { get; set; }

        private ConcurrentQueue<Message> GlobalQueue { get; set; } = new ConcurrentQueue<Message>();

        private ConcurrentDictionary<int, ConcurrentQueue<Message>> ChannelQueue { get; set; } = new ConcurrentDictionary<int, ConcurrentQueue<Message>>();

        public MessageWorker()
        {
            GlobalThread = new Thread(new ThreadStart(GlobalRun));
            ChannelThread = new Thread(new ThreadStart(ChannelRun));
        }

        public void Push(Message message)
        {
            if (message.Session.ChannelId != 0)
            {
                Push(message.Session.ChannelId, message);
            }
            else
            {
                GlobalQueue.Enqueue(message);
            }
        }

        public void Push(int channelId, Message message)
        {
            var channelQueue = ChannelQueue.GetOrAdd(channelId, new ConcurrentQueue<Message>());
            channelQueue.Enqueue(message);
        }

        public void DoAsync<T>(int channelId, T t) where T : Protocols.Request.Header
        {
            var obj = JsonConvert.SerializeObject(t);
            Push(channelId, new Message { Header = JsonConvert.DeserializeObject<Protocols.Request.Header>(obj) });
        }

        public void DoTimer<T>(int millisedons, int channelId, T t) where T : Protocols.Request.Header
        {
            Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromMilliseconds(millisedons));
                var obj = JsonConvert.SerializeObject(t);
                Push(channelId, new Message { Header = JsonConvert.DeserializeObject<Protocols.Request.Header>(obj) });
            });
        }

        public void Push(int channelId, Protocols.Request.Header header)
        {
            var obj = JsonConvert.SerializeObject(header);
            Push(channelId, new Message { Header = JsonConvert.DeserializeObject<Protocols.Request.Header>(obj) });
        }

        public void Start()
        {
            GlobalThread.Start();
            ChannelThread.Start();
        }

        public void Join()
        {
            GlobalThread.Join();
            ChannelThread.Join();
        }

        public void Abort()
        {
            GlobalThread.Abort();
            ChannelThread.Abort();
        }

        public void GlobalRun()
        {
            while (true)
            {
                if (GlobalQueue.TryDequeue(out var message))
                {
                    MessageCallback?.Invoke(message);
                }
            }
        }

        public void ChannelRun()
        {
            while (true)
            {
                Parallel.ForEach(ChannelQueue, queue =>
                {
                    if (queue.Value.TryDequeue(out var message))
                    {
                        MessageCallback?.Invoke(message);
                    }
                });
            }
        }
    }
}
