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