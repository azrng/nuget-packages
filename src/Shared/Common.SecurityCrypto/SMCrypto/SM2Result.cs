using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM2Result
    {
        public BigInteger r;
        public BigInteger s;
        public BigInteger R;
        public byte[] sa;
        public byte[] sb;
        public byte[] s1;
        public byte[] s2;
        public ECPoint keyra;
        public ECPoint keyrb;
    }
}