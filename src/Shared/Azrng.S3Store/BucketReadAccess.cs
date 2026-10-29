namespace Azrng.S3Store
{
    /// <summary>
    /// 桶匿名读访问级别
    /// </summary>
    public enum BucketReadAccess
    {
        /// <summary>
        /// 仅公开读：匿名可读取对象与列出对象，不可写入/删除。
        /// 适用于源码快照、静态资源等只需要公开下载的场景。
        /// </summary>
        ReadOnly,

        /// <summary>
        /// 公开可读写：匿名可读取、写入、删除对象。
        /// 安全风险高（任何人可篡改/删除对象），仅在确实需要匿名上传时使用。
        /// </summary>
        ReadWrite
    }
}
