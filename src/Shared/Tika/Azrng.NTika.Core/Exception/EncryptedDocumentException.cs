namespace Azrng.NTika.Core.Exception
{
    public class EncryptedDocumentException : TikaException
    {
        public EncryptedDocumentException(string message)
            : base(message)
        {
        }

        public EncryptedDocumentException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
