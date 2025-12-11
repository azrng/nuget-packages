namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 文件上传响应
    /// </summary>
    public class FileUploadResponse
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 媒体文件上传后获取的唯一标识，3天内有效
        /// </summary>
        public string media_id { get; set; }

        /// <summary>
        /// 媒体文件上传时间戳
        /// </summary>
        public string created_at { get; set; }
    }

    /// <summary>
    /// 文件上传返回类
    /// </summary>
    public class FileUploadResult
    {
        public FileUploadResult(string type, string mediaId)
        {
            Type = type;
            MediaId = mediaId;
        }

        /// <summary>
        ///  媒体文件类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 媒体文件上传后获取的唯一标识，3天内有效
        /// </summary>
        public string MediaId { get; set; }
    }
}