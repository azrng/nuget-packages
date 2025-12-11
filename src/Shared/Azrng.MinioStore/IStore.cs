namespace Azrng.MinioStore
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
        /// 上传文件
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件地址</param>
        /// <param name="fileContentType">文件内容类型</param>
        /// <returns></returns>
        Task<bool> UploadFileAsync(string bucketName, string fileName, string filePath, string fileContentType = null);

        /// <summary>
        /// 上传文件流
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="steamData"></param>
        /// <param name="fileContentType">文件内容类型</param>
        /// <returns></returns>
        Task<bool> UploadFileAsync(string bucketName, string fileName, Stream steamData, string fileContentType);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="bucketName">桶名称</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        Task<Stream?> DownLoadFileAsync(string bucketName, string fileName);

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
        /// 设置桶支持读写的策略(可以实现永久链接)
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