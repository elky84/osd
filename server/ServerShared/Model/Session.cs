using DotNetty.Buffers;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ServerShared.Model
{
    public class Session<DataType> : IChannelHandlerContext where DataType : class, new()
    {
        private IChannelHandlerContext _context;
        public DataType Data { get; private set; } = new DataType();

        public Session(IChannelHandlerContext context)
        {
            _context = context;
        }

        public IChannel Channel => _context.Channel;

        public IByteBufferAllocator Allocator => _context.Allocator;

        public IEventExecutor Executor => _context.Executor;

        public string Name => _context.Name;

        public IChannelHandler Handler => _context.Handler;

        public bool Removed => _context.Removed;

        public Task BindAsync(EndPoint localAddress)
        {
            return _context.BindAsync(localAddress);
        }

        public Task CloseAsync()
        {
            return _context.CloseAsync();
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return _context.ConnectAsync(remoteAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            return _context.ConnectAsync(remoteAddress, localAddress);
        }

        public Task DeregisterAsync()
        {
            return _context.DeregisterAsync();
        }

        public Task DisconnectAsync()
        {
            return _context.DisconnectAsync();
        }

        public IChannelHandlerContext FireChannelActive()
        {
            return _context.FireChannelActive();
        }

        public IChannelHandlerContext FireChannelInactive()
        {
            return _context.FireChannelInactive();
        }

        public IChannelHandlerContext FireChannelRead(object message)
        {
            return _context.FireChannelRead(message);
        }

        public IChannelHandlerContext FireChannelReadComplete()
        {
            return _context.FireChannelReadComplete();
        }

        public IChannelHandlerContext FireChannelRegistered()
        {
            return _context.FireChannelRegistered();
        }

        public IChannelHandlerContext FireChannelUnregistered()
        {
            return _context.FireChannelUnregistered();
        }

        public IChannelHandlerContext FireChannelWritabilityChanged()
        {
            return _context.FireChannelWritabilityChanged();
        }

        public IChannelHandlerContext FireExceptionCaught(Exception ex)
        {
            return _context.FireExceptionCaught(ex);
        }

        public IChannelHandlerContext FireUserEventTriggered(object evt)
        {
            return _context.FireUserEventTriggered(evt);
        }

        public IChannelHandlerContext Flush()
        {
            return _context.Flush();
        }

        public IChannelHandlerContext Read()
        {
            return _context.Read();
        }

        public Task WriteAndFlushAsync(object message)
        {
            return _context.WriteAndFlushAsync(message);
        }

        public Task WriteAsync(object message)
        {
            return _context.WriteAsync(message);
        }

        public IAttribute<T> GetAttribute<T>(AttributeKey<T> key) where T : class
        {
            return _context.GetAttribute<T>(key);
        }

        public bool HasAttribute<T>(AttributeKey<T> key) where T : class
        {
            return _context.HasAttribute<T>(key);
        }
    }
}
