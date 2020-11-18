using System;

namespace ServerShared.Util
{
    public static class TickUtil
    {

        public static bool IsReadyTick(this int latestTick, int delay)
        {
            return Math.Abs(Environment.TickCount - latestTick) > delay;
        }
    }
}
