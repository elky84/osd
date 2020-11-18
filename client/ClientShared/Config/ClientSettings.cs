using System.Net;

namespace ClientShared.Config
{

    public class ClientSettings
    {
        public static bool IsSsl
        {
            get
            {
                string ssl = Helper.Configuration["ssl"];
                return !string.IsNullOrEmpty(ssl) && bool.Parse(ssl);
            }
        }

        public static IPAddress Host => IPAddress.Parse(Helper.Configuration["host"]);

        public static int Port => int.Parse(Helper.Configuration["port"]);

        public static int Size => int.Parse(Helper.Configuration["size"]);

        public static string UserName => Helper.Configuration["userName"];

        public static bool UseLibuv
        {
            get
            {
                string libuv = Helper.Configuration["libuv"];
                return !string.IsNullOrEmpty(libuv) && bool.Parse(libuv);
            }
        }
    }
}