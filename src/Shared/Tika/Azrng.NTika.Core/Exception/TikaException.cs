namespace Azrng.NTika.Core.Exception
{
    public class TikaException : System.Exception
    {
        public TikaException(string message)
            : base(message)
        {
        }

        public TikaException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
