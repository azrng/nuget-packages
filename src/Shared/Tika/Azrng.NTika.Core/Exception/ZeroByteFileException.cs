namespace Azrng.NTika.Core.Exception
{
    public class ZeroByteFileException : TikaException
    {
        public ZeroByteFileException()
            : base("document is empty")
        {
        }
    }
}
