using System;
using System.IO;
using System.Security.Cryptography;

namespace Common.Security.Model
{
    /// <summary>
    /// 对称加密基类
    /// </summary>
    public class SymmetricEncryptionBase
    {
        protected static byte[] EncryptCore<TCryptoServiceProvider>(byte[] sourceBytes, byte[] keyBytes,
            CipherMode cipherMode = CipherMode.ECB, byte[] ivBytes = null)
            where TCryptoServiceProvider : SymmetricAlgorithm, new()
        {
            var cryptoServiceProvider = new TCryptoServiceProvider();
            try
            {
                cryptoServiceProvider.Key = keyBytes;
                cryptoServiceProvider.Mode = cipherMode;
                if (cipherMode != CipherMode.ECB)
                {
                    cryptoServiceProvider.IV = ivBytes;
                }

                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateEncryptor(),
                    CryptoStreamMode.Write);
                cryptoStream.Write(sourceBytes, 0, sourceBytes.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
            finally
            {
                ((IDisposable)cryptoServiceProvider).Dispose();
            }
        }

        public static byte[] DecryptCore<TCryptoServiceProvider>(byte[] encryptBytes, byte[] keyBytes,
            CipherMode cipherMode = CipherMode.ECB, byte[] ivBytes = null)
            where TCryptoServiceProvider : SymmetricAlgorithm, new()
        {
            var cryptoServiceProvider = new TCryptoServiceProvider();
            try
            {
                cryptoServiceProvider.Key = keyBytes;
                cryptoServiceProvider.Mode = cipherMode;
                if (cipherMode != CipherMode.ECB)
                {
                    cryptoServiceProvider.IV = ivBytes;
                }

                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateDecryptor(),
                    CryptoStreamMode.Write);
                cryptoStream.Write(encryptBytes, 0, encryptBytes.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
            finally
            {
                ((IDisposable)cryptoServiceProvider).Dispose();
            }
        }
    }
}