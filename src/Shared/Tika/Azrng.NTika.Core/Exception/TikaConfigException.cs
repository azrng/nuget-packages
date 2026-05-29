namespace Azrng.NTika.Core.Exception
{
    public class TikaConfigException : TikaException
    {
        public TikaConfigException(string message)
            : base(message)
        {
        }

        public TikaConfigException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
