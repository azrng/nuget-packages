using Azrng.NTika.Core.Exception;

namespace Azrng.NTika.Core.Sax
{
    public class WriteLimiter
    {
        private readonly int _writeLimit;
        private int _writeCount;

        public WriteLimiter(int writeLimit)
        {
            _writeLimit = writeLimit;
        }

        public void ThrowIfWriteLimitReached()
        {
            if (_writeLimit >= 0 && _writeCount > _writeLimit)
            {
                throw new WriteLimitReachedException(_writeLimit);
            }
        }

        public void AddWritten(int count)
        {
            _writeCount += count;
            ThrowIfWriteLimitReached();
        }

        public int WriteCount => _writeCount;
        public int WriteLimit => _writeLimit;
    }
}
