using Common.SecurityCrypto.Model;
using System.Text;

namespace Common.SecurityCrypto
{
    public interface IAsymmetricProvider
    {
        /// <summary>
        /// 返回格式
        /// </summary>
        OutType OutType { get; set; }

        /// <summary>
        /// 编码格式
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// key 类型
        /// </summary>
        RSAKeyType KeyType { get; set; }

        /// <summary>
        /// 创建key
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        AsymmetricKey CreateKey(RSAKeySizeType size = RSAKeySizeType.L2048);

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        string Encrypt(string value, string publicKey);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        string Decrypt(string value, string privateKey);

        string SignData(string source, string privateKey);

        /// <summary>
        /// 验证值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="signData"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        bool VerifyData(string source, string signData, string publicKey);
    }
}