using Newtonsoft.Json;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 推送图片消息
    /// </summary>
    public class ImageMessageDto : IBaseSendMessageDto
    {
        [JsonProperty("image")]
        public ImageContentDto Text { get; set; }

        /// <summary>
        /// 图片类型
        /// </summary>
        public string MsgType => "image";
    }
}