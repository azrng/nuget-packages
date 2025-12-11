namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 上传资源请求类
    /// </summary>
    public class UploadMediaRequest
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件数组
        /// </summary>
        public byte[] Media { get; set; }
    }
}