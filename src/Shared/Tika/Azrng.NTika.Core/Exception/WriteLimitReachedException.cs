namespace Azrng.NTika.Core.Exception
{
    public class WriteLimitReachedException : TikaException
    {
        private const int MaxDepth = 100;

        private readonly int _writeLimit;

        public WriteLimitReachedException(int writeLimit)
            : base($"Your document contained more than {writeLimit} characters, and so your requested limit has been reached. To receive the full text of the document, increase your limit. (Text up to the limit is however available).")
        {
            _writeLimit = writeLimit;
        }

        public int WriteLimit => _writeLimit;

        public static bool IsWriteLimitReached(System.Exception? t)
        {
            return IsWriteLimitReached(t, 0);
        }

        private static bool IsWriteLimitReached(System.Exception? t, int depth)
        {
            if (t == null || depth > MaxDepth)
            {
                return false;
            }

            if (t is WriteLimitReachedException)
            {
                return true;
            }

            return IsWriteLimitReached(t.InnerException, depth + 1);
        }

        public static void ThrowIfWriteLimitReached(System.Exception? ex)
        {
            ThrowIfWriteLimitReached(ex, 0);
        }

        private static void ThrowIfWriteLimitReached(System.Exception? ex, int depth)
        {
            if (ex == null || depth > MaxDepth)
            {
                return;
            }

            if (ex is WriteLimitReachedException writeLimitEx)
            {
                throw writeLimitEx;
            }

            ThrowIfWriteLimitReached(ex.InnerException, depth + 1);
        }
    }
}
