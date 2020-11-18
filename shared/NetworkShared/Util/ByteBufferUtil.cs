using DotNetty.Buffers;
using Newtonsoft.Json;

namespace NetworkShared.Util
{
    public static class ByteBufferUtil
    {
        public static IByteBuffer ToByteBuffer(this Protocols.Request.Header header)
        {
            var msg = Unpooled.Buffer();
            msg.WriteString(JsonConvert.SerializeObject(header), System.Text.Encoding.UTF8);
            return msg;
        }

        public static IByteBuffer ToByteBuffer(this Protocols.Response.Header header)
        {
            var msg = Unpooled.Buffer();
            msg.WriteString(JsonConvert.SerializeObject(header), System.Text.Encoding.UTF8);
            return msg;
        }
    }
}
