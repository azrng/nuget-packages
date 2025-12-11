using System;
using System.Text;

namespace Common.SecurityCrypto
{
    public class AES
    {
        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public string AesEncrypt(string key, string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            var toEncryptArray = Encoding.UTF8.GetBytes(str);

            var rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            var cTransform = rm.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="key">密文</param>
        /// <param name="str">明文（待解密）</param>
        /// <returns></returns>
        public string AesDecrypt(string key, string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            byte[] toEncryptArray = Convert.FromBase64String(str);

            var rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            var cTransform = rm.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}