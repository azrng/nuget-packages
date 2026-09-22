
namespace Common.HttpClients
{
    /// <summary>
    /// 文件下载结果
    /// </summary>
    public class DownloadResult
    {
        /// <summary>
        /// 保存的本地文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 下载的文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }
    }
}
