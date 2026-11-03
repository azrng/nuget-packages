namespace Azrng.S3Store
{
    public class GlobalConfig
    {
        /// <summary>
        /// 是否使用https
        /// </summary>
        public bool UseHttps { get; set; }

        /// <summary>
        /// 上传时是否自动创建不存在的桶（由 S3StoreConfig.AutoCreateBucket 传入）
        /// </summary>
        public bool AutoCreateBucket { get; set; } = true;
    }
}