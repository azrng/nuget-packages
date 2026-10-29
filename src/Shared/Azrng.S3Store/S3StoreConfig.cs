using System.ComponentModel.DataAnnotations;

namespace Azrng.S3Store
{
    /// <summary>
    /// s3 存储 配置
    /// </summary>
    public class S3StoreConfig
    {
        /// <summary>
        /// 服务url
        /// </summary>
        [Required]
        public string? Url { get; set; }

        /// <summary>
        /// 授权key
        /// </summary>
        [Required]
        public string? AccessKey { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        [Required]
        public string? SecretKey { get; set; }

        /// <summary>
        /// 使用 https 时是否接受不受信任的 TLS 证书（自签名内网 MinIO 场景）。
        /// 默认 true 保持历史行为；生产环境使用受信证书时建议显式设为 false，启用标准证书校验。
        /// </summary>
        public bool AllowInsecureTls { get; set; } = true;

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ParamVerify()
        {
            if (string.IsNullOrWhiteSpace(AccessKey))
                throw new ArgumentNullException(nameof(AccessKey));
            if (string.IsNullOrWhiteSpace(SecretKey))
                throw new ArgumentNullException(nameof(SecretKey));
            if (string.IsNullOrWhiteSpace(Url))
                throw new ArgumentNullException(nameof(Url));
        }
    }
}