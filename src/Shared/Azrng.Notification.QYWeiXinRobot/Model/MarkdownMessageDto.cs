using Newtonsoft.Json;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// markdown消息
    /// </summary>
    public class MarkdownMessageDto : IBaseSendMessageDto
    {
        /// <summary>
        /// markdown类型 
        /// </summary>
        public string MsgType => "markdown";

        /// <summary>
        /// markdown内容，最长不超过4096个字节，必须是utf8编码
        /// </summary>
        [JsonProperty("markdown")]
        public SendMentionedUser Text { get; set; }
    }
}