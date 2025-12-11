using Common.SecurityCrypto.Model;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Text;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM4
    {
        public string secretKey = "";
        public string iv = "";
        public bool hexString = false;

        public OutType _outType;

        public SM4(OutType outType = OutType.Hex)
        {
            _outType = outType;
        }

        public string EncryptECB(string plainText)
        {
            var ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = 1;
            var key = !hexString ? Encoding.UTF8.GetBytes(secretKey) : Hex.Decode(secretKey);
            var cryptoServiceProvider = new SM4CryptoServiceProvider();
            cryptoServiceProvider.sm4_setkey_enc(ctx, key);
            var encrypted = cryptoServiceProvider.sm4_crypt_ecb(ctx, Encoding.UTF8.GetBytes(plainText));
            return _outType == OutType.Hex ? Encoding.UTF8.GetString(Hex.Encode(encrypted)) : Convert.ToBase64String(encrypted);
        }

        public string DecryptECB(string cipherText)
        {
            var ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = 0;
            var key = !hexString ? Encoding.UTF8.GetBytes(secretKey) : Hex.Decode(secretKey);
            var cryptoServiceProvider = new SM4CryptoServiceProvider();
            cryptoServiceProvider.sm4_setkey_dec(ctx, key);
            byte[] decrypted = _outType == OutType.Hex ? cryptoServiceProvider.sm4_crypt_ecb(ctx, Hex.Decode(cipherText))
                : cryptoServiceProvider.sm4_crypt_ecb(ctx, Convert.FromBase64String(cipherText));
            return Encoding.UTF8.GetString(decrypted);
        }

        public string EncryptCBC(string plainText)
        {
            var ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = 1;
            byte[] key;
            byte[] iv;
            if (hexString)
            {
                key = Hex.Decode(secretKey);
                iv = Hex.Decode(this.iv);
            }
            else
            {
                key = Encoding.UTF8.GetBytes(secretKey);
                iv = Encoding.UTF8.GetBytes(this.iv);
            }
            var cryptoServiceProvider = new SM4CryptoServiceProvider();
            cryptoServiceProvider.sm4_setkey_enc(ctx, key);
            var encrypted = cryptoServiceProvider.sm4_crypt_cbc(ctx, iv, Encoding.UTF8.GetBytes(plainText));
            return _outType == OutType.Hex ? Encoding.UTF8.GetString(Hex.Encode(encrypted)) : Convert.ToBase64String(encrypted);
        }

        public string DecryptCBC(string cipherText)
        {
            var ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = 0;
            byte[] key;
            byte[] iv;
            if (hexString)
            {
                key = Hex.Decode(secretKey);
                iv = Hex.Decode(this.iv);
            }
            else
            {
                key = Encoding.UTF8.GetBytes(secretKey);
                iv = Encoding.UTF8.GetBytes(this.iv);
            }
            var cryptoServiceProvider = new SM4CryptoServiceProvider();
            cryptoServiceProvider.sm4_setkey_dec(ctx, key);
            return Encoding.UTF8.GetString(cryptoServiceProvider.sm4_crypt_cbc(ctx, iv, _outType == OutType.Hex ? Hex.Decode(cipherText) : Convert.FromBase64String(cipherText)));
        }

        public string EncryptECB4JS(string plainText)
        {
            var ctx = new SM4ContextJS();
            ctx.isPadding = true;
            ctx.mode = 1;
            var key = !hexString ? Encoding.UTF8.GetBytes(secretKey) : Hex.Decode(secretKey);
            var serviceProviderJs = new SM4CryptoServiceProviderJS();
            serviceProviderJs.sm4_setkey_enc(ctx, key);
            return Encoding.UTF8.GetString(Hex.Encode(serviceProviderJs.sm4_crypt_ecb(ctx, Encoding.UTF8.GetBytes(plainText))));
        }

        public string DecryptECB4JS(string cipherText)
        {
            var ctx = new SM4ContextJS();
            ctx.isPadding = true;
            ctx.mode = 0;
            var key = !hexString ? Encoding.UTF8.GetBytes(secretKey) : Hex.Decode(secretKey);
            var serviceProviderJs = new SM4CryptoServiceProviderJS();
            serviceProviderJs.sm4_setkey_dec(ctx, key);
            return Encoding.UTF8.GetString(serviceProviderJs.sm4_crypt_ecb(ctx, Hex.Decode(cipherText)));
        }

        public string EncryptCBC4JS(string plainText)
        {
            var ctx = new SM4ContextJS();
            ctx.isPadding = true;
            ctx.mode = 1;
            byte[] key;
            byte[] iv;
            if (hexString)
            {
                key = Hex.Decode(secretKey);
                iv = Hex.Decode(this.iv);
            }
            else
            {
                key = Encoding.UTF8.GetBytes(secretKey);
                iv = Encoding.UTF8.GetBytes(this.iv);
            }
            var serviceProviderJs = new SM4CryptoServiceProviderJS();
            serviceProviderJs.sm4_setkey_enc(ctx, key);
            return Encoding.UTF8.GetString(Hex.Encode(serviceProviderJs.sm4_crypt_cbc(ctx, iv, Encoding.UTF8.GetBytes(plainText))));
        }

        public string DecryptCBC4JS(string cipherText)
        {
            var ctx = new SM4ContextJS();
            ctx.isPadding = true;
            ctx.mode = 0;
            byte[] key;
            byte[] iv;
            if (hexString)
            {
                key = Hex.Decode(secretKey);
                iv = Hex.Decode(this.iv);
            }
            else
            {
                key = Encoding.UTF8.GetBytes(secretKey);
                iv = Encoding.UTF8.GetBytes(this.iv);
            }
            var serviceProviderJs = new SM4CryptoServiceProviderJS();
            serviceProviderJs.sm4_setkey_dec(ctx, key);
            return Encoding.UTF8.GetString(serviceProviderJs.sm4_crypt_cbc(ctx, iv, Hex.Decode(cipherText)));
        }
    }
}