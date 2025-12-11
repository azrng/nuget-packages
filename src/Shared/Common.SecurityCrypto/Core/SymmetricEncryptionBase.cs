using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.SecurityCrypto.Core
{
    public abstract class SymmetricEncryptionBase
    {
        protected static Func<string, Func<string, Func<Encoding, Func<int, byte[]>>>> ComputeRealValueFunc() => originString => (Func<string, Func<Encoding, Func<int, byte[]>>>)(salt => (Func<Encoding, Func<int, byte[]>>)(encoding => (Func<int, byte[]>)(size =>
        {
            if (string.IsNullOrWhiteSpace(originString))
                return new byte[1];
            encoding ??= Encoding.UTF8;
            int length = size / 8;
            if (string.IsNullOrWhiteSpace(salt))
            {
                var destinationArray = new byte[length];
                Array.Copy((Array)encoding.GetBytes(originString.PadRight(length)), destinationArray, length);
                return destinationArray;
            }
            byte[] bytes = encoding.GetBytes(salt);
            return new Rfc2898DeriveBytes(encoding.GetBytes(originString), bytes, 1000).GetBytes(length);
        })));

        protected static byte[] EncryptCore<TCryptoServiceProvider>(
          byte[] sourceBytes,
          byte[] keyBytes,
          byte[] ivBytes)
          where TCryptoServiceProvider : SymmetricAlgorithm, new()
        {
            var cryptoServiceProvider = new TCryptoServiceProvider();
            try
            {
                cryptoServiceProvider.Key = keyBytes;
                cryptoServiceProvider.IV = ivBytes;
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream((Stream)memoryStream, cryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(sourceBytes, 0, sourceBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray();
                    }
                }
            }
            finally
            {
                if (cryptoServiceProvider != null)
                    ((IDisposable)cryptoServiceProvider).Dispose();
            }
        }

        protected static byte[] DecryptCore<TCryptoServiceProvider>(
          byte[] encryptBytes,
          byte[] keyBytes,
          byte[] ivBytes)
          where TCryptoServiceProvider : SymmetricAlgorithm, new()
        {
            var cryptoServiceProvider = new TCryptoServiceProvider();
            try
            {
                cryptoServiceProvider.Key = keyBytes;
                cryptoServiceProvider.IV = ivBytes;
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream((Stream)memoryStream, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(encryptBytes, 0, encryptBytes.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
            finally
            {
                if (cryptoServiceProvider != null)
                    ((IDisposable)cryptoServiceProvider).Dispose();
            }
        }
    }
}