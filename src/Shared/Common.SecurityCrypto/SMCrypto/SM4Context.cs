namespace Common.SecurityCrypto.SMCrypto
{
    public class SM4Context
    {
        public int mode;
        public long[] sk;
        public bool isPadding;

        public SM4Context()
        {
            mode = 1;
            isPadding = true;
            sk = new long[32];
        }
    }
}