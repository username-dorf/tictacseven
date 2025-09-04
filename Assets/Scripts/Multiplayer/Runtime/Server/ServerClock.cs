using System.Diagnostics;

namespace Multiplayer.Server
{
    public static class ServerClock
    {
        public static long NowTicks()
        {
            return Stopwatch.GetTimestamp();
        }
        public static long Frequency
        {
            get { return Stopwatch.Frequency; }
        }

        public static long AddSeconds(long nowTicks, double seconds)
        {
            return nowTicks + (long) (seconds * Frequency);
        }

        public static double TicksToMs(long ticks)
        {
            return (ticks * 1000.0) / Frequency;
        }
    }
}