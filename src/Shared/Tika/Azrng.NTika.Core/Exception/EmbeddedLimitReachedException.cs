namespace Azrng.NTika.Core.Exception
{
    public class EmbeddedLimitReachedException : TikaException
    {
        public EmbeddedLimitReachedException(string message)
            : base(message)
        {
        }

        public EmbeddedLimitReachedException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
