using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NetworkShared.Util;
using System.Threading.Tasks;

namespace ServerShared.Model
{
    public class Session
    {
        public int Index { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public string UserName { get; set; }

        private IChannelHandlerContext ChannelContext { get; }

        public int ChannelId { get; set; }

        public string Id => ChannelContext.GetHashCode().ToString();

        public Session(IChannelHandlerContext context)
        {
            this.ChannelContext = context;
        }

        private async Task Send(IByteBuffer byteBuffer)
        {
            await ChannelContext.WriteAndFlushAsync(byteBuffer);
        }
    }
}
