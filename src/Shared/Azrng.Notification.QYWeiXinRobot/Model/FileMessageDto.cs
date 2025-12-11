using Newtonsoft.Json;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 文件类型消息
    /// </summary>
    public class FileMessageDto : IBaseSendMessageDto
    {
        /// <summary>
        /// 文本类型
        /// </summary>
        public string MsgType => "file";

        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty("file")]
        public FileContent File { get; set; }

        public class FileContent
        {
            /// <summary>
            /// 文件id，通过下文的文件上传接口获取
            /// </summary>
            [JsonProperty("media_id")]
            public string MediaId { get; set; }
        }
    }
}