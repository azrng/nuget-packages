namespace Azrng.NTika.Core.Exception
{
    public class UnsupportedFormatException : TikaException
    {
        public UnsupportedFormatException(string message)
            : base(message)
        {
        }

        public UnsupportedFormatException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
