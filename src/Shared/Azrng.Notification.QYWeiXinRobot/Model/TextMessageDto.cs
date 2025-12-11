using Newtonsoft.Json;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 文本消息
    /// </summary>
    public class TextMessageDto : IBaseSendMessageDto
    {
        /// <summary>
        /// 文本内容，最长不超过2048个字节，必须是utf8编码
        /// </summary>
        [JsonProperty("text")]
        public SendMentionedUser Text { get; set; }

        /// <summary>
        /// 文本类型
        /// </summary>
        public string MsgType => "text";
    }
}