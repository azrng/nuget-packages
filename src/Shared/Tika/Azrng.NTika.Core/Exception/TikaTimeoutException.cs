namespace Azrng.NTika.Core.Exception
{
    public class TikaTimeoutException : TikaException
    {
        public TikaTimeoutException(string message)
            : base(message)
        {
        }

        public TikaTimeoutException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
