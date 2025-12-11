using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 图文消息
    /// </summary>
    public class NewsMessageDto : IBaseSendMessageDto
    {
        /// <summary>
        /// 图文类型
        /// </summary>
        public string MsgType => "news";

        /// <summary>
        /// 图文内容
        /// </summary>
        [JsonProperty("news")]
        public NewsContent News { get; set; }

        public class NewsContent
        {
            /// <summary>
            /// 图文消息，一个图文消息支持1到8条图文
            /// </summary>
            [JsonProperty("articles")]
            public List<ArticlesContentDto> Articles { get; set; }
        }
    }
}