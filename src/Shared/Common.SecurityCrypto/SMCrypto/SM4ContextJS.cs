namespace Common.SecurityCrypto.SMCrypto
{
    public class SM4ContextJS
    {
        public int mode;
        public int[] sk;
        public bool isPadding;

        public SM4ContextJS()
        {
            mode = 1;
            isPadding = true;
            sk = new int[32];
        }
    }
}