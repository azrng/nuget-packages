namespace Azrng.NTika.Core.Exception
{
    public class CorruptedFileException : TikaException
    {
        public CorruptedFileException(string message)
            : base(message)
        {
        }

        public CorruptedFileException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
