namespace Common.SecurityCrypto.Model
{
    public class RSAKey
    {
        public string PublickKey { get; set; }

        public string PrivateKey { get; set; }

        public string Exponent { get; set; }

        public string Modulus { get; set; }
    }
}