using System.Threading;

namespace Serilog.Support
{
    internal static class Some
    {
        private static int _counter;

        public static int Int()
        {
            return Interlocked.Increment(ref _counter);
        }

        public static string String(string tag = null)
        {
            return (tag ?? "") + "__" + Int();
        }
    }
}
