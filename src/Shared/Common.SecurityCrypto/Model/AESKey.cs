namespace Common.SecurityCrypto.Model
{
    public class AESKey
    {
        public string Key { get; set; }

        public string IV { get; set; }

        public AESKeySizeType Size { get; set; }
    }
}