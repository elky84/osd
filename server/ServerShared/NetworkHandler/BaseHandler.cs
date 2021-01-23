using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FlatBuffers;
using Serilog;
using ServerShared.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ServerShared.NetworkHandler
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FlatBufferEventAttribute : Attribute
    { }

    public class Timer
    { 
        public long Interval { get; set; }
        public Action<long> Callback { get; set; }
        public long LastEventTick { get; set; }
    }

    public abstract class BaseHandler<DataType> : ChannelHandlerAdapter, IEnumerable<Session<DataType>>
        where DataType : class, new()
    {
        private Dictionary<Type, Delegate> _allocatorDict = new Dictionary<Type, Delegate>();
        private Dictionary<string, Type> _flatBufferDict = new Dictionary<string, Type>();
        private Dictionary<Type, Func<Session<DataType>, IFlatbufferObject, bool>> _bindedEventDict = new Dictionary<Type, Func<Session<DataType>, IFlatbufferObject, bool>>();
        private Dictionary<IChannelHandlerContext, Session<DataType>> _sessionDict = new Dictionary<IChannelHandlerContext, Session<DataType>>();
        private Thread _schedulerThread;
        private Mutex _mutex = new Mutex();
        private List<Timer> _timers = new List<Timer>();

        public override bool IsSharable => true;

        public bool Running { get; private set; } = true;

        public List<Session<DataType>> Sessions => _sessionDict.Values.ToList();

        protected BaseHandler()
        {
            BindFlatBufferAllocator("NetworkShared");
            BindEventHandler();
        }

        public bool ExecuteScheduler()
        {
            if (_schedulerThread != null)
                return false;

            _schedulerThread = new Thread(new ParameterizedThreadStart(SchedulerHandler));
            _schedulerThread.Start();
            return true;
        }

        private void SchedulerHandler(object param)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (Running)
            {
                try
                {
                    stopWatch.Stop();
                    _mutex.WaitOne();

                    foreach (var timer in _timers)
                    {
                        var now = DateTime.Now.Ticks;
                        var elapsedTime = new TimeSpan(now - timer.LastEventTick);
                        if (elapsedTime.TotalMilliseconds < timer.Interval)
                            continue;

                        timer.LastEventTick = now;
                        timer.Callback((long)elapsedTime.TotalMilliseconds);
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.Message);
                }
                finally
                {
                    _mutex.ReleaseMutex();
                    stopWatch.Reset();
                    stopWatch.Start();
                }
            }
        }

        public void SetTimer(long interval, Action<long> callback)
        {
            _timers.Add(new Timer
            { 
                Interval = interval,
                Callback = callback,
                LastEventTick = DateTime.Now.Ticks
            });
        }

        public void Release()
        {
            Running = false;
            _schedulerThread?.Join();
        }

        private void BindFlatBufferAllocator(string assemblyName)
        {
            var assembly = string.IsNullOrEmpty(assemblyName) ?
                Assembly.GetExecutingAssembly() :
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName);

            var flatBufferTypes = assembly.GetTypes().Where(x => x.GetInterface(nameof(IFlatbufferObject)) != null).ToList();

            foreach (var flatbType in flatBufferTypes)
            {
                var methods = flatbType.GetMethods();
                var allocator = methods.FirstOrDefault(x =>
                {
                    if (x.IsStatic == false)
                        return false;

                    var parameters = x.GetParameters();
                    if (parameters.Length != 1)
                        return false;

                    if (parameters.First().ParameterType != typeof(ByteBuffer))
                        return false;

                    if (x.ReturnType != flatbType)
                        return false;

                    return true;
                });

                var method = allocator.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(ByteBuffer), flatbType));
                _allocatorDict.Add(flatbType, method);
                _flatBufferDict.Add(flatbType.FullName, flatbType);
            }
        }

        private void BindEventHandler()
        {
            var methods = this.GetType().GetMethods().Where(x =>
            {
                if (x.GetCustomAttribute<FlatBufferEventAttribute>() == null)
                    return false;

                if (x.ReturnType != typeof(bool))
                    return false;

                var parameters = x.GetParameters();
                if (parameters.Length != 2)
                    return false;

                if (parameters[0].ParameterType != typeof(Session<DataType>))
                    return false;

                if (parameters[1].ParameterType.GetInterface(nameof(IFlatbufferObject)) == null)
                    return false;

                return true;
            });

            foreach (var method in methods)
            {

                try
                {
                    var parameters = method.GetParameters();
                    var flatBufferType = parameters[1].ParameterType;
                    var delegateType = Expression.GetDelegateType(parameters.Select(x => x.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
                    var createdDelegate = method.CreateDelegate(delegateType, this);
                    _bindedEventDict.Add(flatBufferType, new Func<Session<DataType>, IFlatbufferObject, bool>((session, protocol) =>
                    {
                        return (bool)createdDelegate.DynamicInvoke(session, Convert.ChangeType(protocol, flatBufferType));
                    }));
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.Message);
                }
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            try
            {
                _mutex.WaitOne();
                base.ChannelActive(context);
                _sessionDict.Add(context, new Session<DataType>(context));
                OnConnected(_sessionDict[context]);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            try
            {
                _mutex.WaitOne();
                base.ChannelInactive(context);
                if (_sessionDict.ContainsKey(context))
                {
                    OnDisconnected(_sessionDict[context]);
                    _sessionDict.Remove(context);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object byteBuffer)
        {
            if (_sessionDict.TryGetValue(context, out var session) == false)
            {
                Log.Logger.Error("Session Get Failed() {0}", context);
                return;
            }

            var buffer = byteBuffer as IByteBuffer;
            try
            {
                var size = buffer.ReadInt();
                var strLength = buffer.ReadByte();
                var flatBufferName = buffer.ReadString(strLength, System.Text.Encoding.Default);
                if (_flatBufferDict.TryGetValue(flatBufferName, out var flatBufferType) == false)
                    throw new Exception($"{flatBufferName} is not binded in event handler.");

                var bytes = new byte[size];
                buffer.ReadBytes(bytes);
                var result = Call(session, flatBufferType, bytes);
                if (result == false)
                {
                    _sessionDict.Remove(context);
                    context.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"{e}");
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Log.Logger.Error("Exception: " + exception);
            context.CloseAsync();
        }

        public bool Call<FlatBufferType>(Session<DataType> session, byte[] bytes) where FlatBufferType : struct, IFlatbufferObject
        {
            try
            {
                if (_bindedEventDict.TryGetValue(typeof(FlatBufferType), out var bindedEvent) == false)
                    return false;

                if (_allocatorDict.TryGetValue(typeof(FlatBufferType), out var allocator) == false)
                    return false;

                return bindedEvent.Invoke(session, (FlatBufferType)allocator.DynamicInvoke(new ByteBuffer(bytes)));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Call(Session<DataType> session, Type type, byte[] bytes)
        {
            try
            {
                _mutex.WaitOne();
                if (_bindedEventDict.TryGetValue(type, out var bindedEvent) == false)
                    return false;

                if (_allocatorDict.TryGetValue(type, out var allocator) == false)
                    return false;

                return bindedEvent.Invoke(session, Convert.ChangeType(allocator.DynamicInvoke(new ByteBuffer(bytes)), type) as IFlatbufferObject);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public IEnumerator<Session<DataType>> GetEnumerator() => Sessions.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Sessions.GetEnumerator();

        protected abstract void OnConnected(Session<DataType> session);

        protected abstract void OnDisconnected(Session<DataType> session);
    }

    public static class Extension
    {
        public static async Task Send(this IChannelHandlerContext context, byte[] bytes)
        {
            await context.WriteAndFlushAsync(Unpooled.Buffer().WriteBytes(bytes));
        }
    }
}