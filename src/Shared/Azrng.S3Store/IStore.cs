namespace Azrng.S3Store
{
    /// <summary>
    /// 存储接口
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// 检查桶是否存在
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <returns></returns>
        Task<bool> CheckBucketExistAsync(string bucketName);

        /// <summary>
        /// 检查桶内是否存在某一个文件目录
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="folderPath">文件目录</param>
        /// <returns></returns>
        Task<bool> CheckBucketFolderExistAsync(string bucketName, string folderPath);

        /// <summary>
        /// 创建桶如果不存在
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <returns></returns>
        Task<bool> CreateBucketIfNotExistsAsync(string bucketName);

        /// <summary>
        /// 获取文件url并设置过期时间
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="expiresInt">过期时间(秒)</param>
        /// <returns></returns>
        Task<string> GetFileUrlAsync(string bucketName, string fileName, int expiresInt = 7 * 24 * 3600);

        /// <summary>
        /// 上传文件。传输或权限失败时抛出 <c>Amazon.Runtime.AmazonServiceException</c> 等异常；HTTP 响应非 200 时返回 false。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件地址</param>
        /// <param name="fileContentType">文件内容类型</param>
        /// <returns>上传是否成功</returns>
        Task<bool> UploadFileAsync(string bucketName, string fileName, string filePath, string? fileContentType = null);

        /// <summary>
        /// 上传文件流。传输或权限失败时抛出 <c>Amazon.Runtime.AmazonServiceException</c> 等异常；HTTP 响应非 200 时返回 false。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="steamData">文件流</param>
        /// <param name="fileContentType">文件内容类型</param>
        /// <returns>上传是否成功</returns>
        Task<bool> UploadFileAsync(string bucketName, string fileName, Stream steamData, string fileContentType);

        /// <summary>
        /// 下载文件。
        /// 桶或对象不存在时返回 null；网络故障、权限错误等真实故障抛出 <c>Amazon.Runtime.AmazonServiceException</c> 或
        /// <c>Amazon.Runtime.AmazonClientException</c> 原样传播，调用方可用异常类型与 HttpStatusCode 区分「未命中」与「故障」。
        /// 返回的流需要调用方负责释放。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <returns>对象内容流；对象不存在时为 null</returns>
        Task<Stream?> DownLoadFileAsync(string bucketName, string fileName);

        /// <summary>
        /// 下载文件到本地路径。对象不存在时返回 false，不创建本地文件；真实故障抛出异常。
        /// 采用先写临时文件再原子替换的方式落盘，避免中断留下半成品文件。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="localPath">本地文件完整路径</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否下载成功；对象不存在时为 false</returns>
        Task<bool> DownloadToFileAsync(string bucketName, string fileName, string localPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查对象是否存在（单对象 HEAD 查询，比目录级前缀列举更精确）。
        /// 桶不存在或对象不存在均返回 false；网络/权限故障抛出异常。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <returns>对象是否存在</returns>
        Task<bool> DoesObjectExistAsync(string bucketName, string fileName);

        /// <summary>
        /// 获取对象元数据（Size/ETag/ContentType/LastModified）。对象不存在时返回 null；网络/权限故障抛出异常。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <returns>对象元数据；对象不存在时为 null</returns>
        Task<S3ObjectMetadata?> GetObjectMetadataAsync(string bucketName, string fileName);

        /// <summary>
        /// 删除桶
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<bool> DeleteBucketAsync(string bucketName);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(string bucketName, string fileName);

        /// <summary>
        /// 配置桶的匿名访问策略。
        /// <see cref="BucketReadAccess.ReadOnly"/>：仅 s3:GetObject 公开，适用于公开下载类桶；
        /// <see cref="BucketReadAccess.ReadWrite"/>：匿名可读写删，任何人可篡改对象，仅限确实需要匿名上传的场景。
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="access">匿名读访问级别</param>
        /// <returns>是否配置成功</returns>
        Task<bool> ConfigBucketPolicyAsync(string bucketName, BucketReadAccess access);

        /// <summary>
        /// 设置桶支持读写的策略(可以实现永久链接)。
        /// 注意：等价于 ConfigBucketPolicyAsync(bucketName, BucketReadAccess.ReadWrite)，
        /// 任何人都可读取、覆盖、删除桶内对象，公开下载场景请改用 BucketReadAccess.ReadOnly。
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<bool> ConfigBucketReadAndWritePolicyAsync(string bucketName);

        /// <summary>
        /// 获取桶对应的策略内容
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<string> GetBucketPolicyAsync(string bucketName);
    }
}