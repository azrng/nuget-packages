using Common.Security;
using Common.Security.Enums;
using System;
using System.Security.Cryptography;

namespace StudyUse
{
    /// <summary>
    /// 订单硬件授权
    /// </summary>
    public static class OrderHardwareLicense
    {
        /// <summary>
        /// 验证许可证是否有效
        /// </summary>
        /// <param name="deviceId">设备id</param>
        /// <param name="licenseKey">Base64编码的许可证签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="strict">是否使用严格模式</param>
        /// <returns>验证结果</returns>
        public static bool Validate(string deviceId, string licenseKey, string publicKey, bool strict = true)
        {
            if (strict)
                CommonHelper.AntiTamperCheck();

            if (string.IsNullOrWhiteSpace(licenseKey))
                throw new ArgumentException("许可证不能为空");

            try
            {
                return RsaHelper.VerifyData(deviceId, licenseKey, publicKey, HashAlgorithmName.SHA256, signType: OutType.Hex);
            }
            catch (FormatException)
            {
                throw new ArgumentException("许可证格式无效");
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException("许可证验证失败", ex);
            }
        }
    }
}