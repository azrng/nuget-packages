using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public abstract class AbsAsymmetricProvider : IAsymmetricProvider
    {
        public OutType OutType { get; set; }

        public Encoding Encoding { get; set; }

        public RSAKeyType KeyType { get; set; }

        public AbsAsymmetricProvider(OutType outType = OutType.Hex, RSAKeyType keyType = RSAKeyType.Xml, Encoding encoding = null)
        {
            OutType = outType;
            if (encoding == null)
                encoding = Encoding.UTF8;
            Encoding = encoding;
            KeyType = keyType;
        }

        /// <summary>
        /// 创建key
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public abstract AsymmetricKey CreateKey(RSAKeySizeType size = RSAKeySizeType.L2048);

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public abstract string Encrypt(string value, string publicKey);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public abstract string Decrypt(string value, string privateKey);

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="source"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public abstract string SignData(string source, string privateKey);

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="source"></param>
        /// <param name="signData"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public abstract bool VerifyData(string source, string signData, string publicKey);
    }
}