namespace Azrng.NTika.Core.Exception
{
    public class AccessPermissionException : TikaException
    {
        public AccessPermissionException(string message)
            : base(message)
        {
        }

        public AccessPermissionException(string message, System.Exception? cause)
            : base(message, cause)
        {
        }
    }
}
