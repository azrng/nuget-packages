namespace Azrng.S3Store
{
    /// <summary>
    /// 对象元数据（不泄漏 AWS SDK 类型）
    /// </summary>
    public class S3ObjectMetadata
    {
        /// <summary>
        /// 对象大小(字节)
        /// </summary>
        public long Size { get; init; }

        /// <summary>
        /// 对象 ETag(分块上传对象为多部件摘要，不能当作文件内容哈希使用)
        /// </summary>
        public string? ETag { get; init; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string? ContentType { get; init; }

        /// <summary>
        /// 最后修改时间(UTC)
        /// </summary>
        public DateTime LastModifiedUtc { get; init; }
    }
}
