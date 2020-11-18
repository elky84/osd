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

        public Session(IChannelHandlerContext context)
        {
            this.ChannelContext = context;
        }

        public string GetSessionId()
        {
            return ChannelContext.GetHashCode().ToString();
        }

        private async Task Send(IByteBuffer byteBuffer)
        {
            await ChannelContext.WriteAndFlushAsync(byteBuffer);
        }

        public async Task Send(Protocols.Response.Header packet)
        {
            await Send(packet.ToByteBuffer());
        }
    }
}
