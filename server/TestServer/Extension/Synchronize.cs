using System;

namespace TestServer.Extension
{
    public static class Synchronize
    {
        public static bool Validate(this DateTime now, uint latency = 10)
        {
            if (now > DateTime.Now)
                return false;

            if ((DateTime.Now - now).TotalSeconds > latency)
                return false;

            return true;
        }

        public static bool Validate(this long now) => Validate(new DateTime(now));
    }
}
