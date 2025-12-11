using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 基础发送消息类
    /// </summary>
    public interface IBaseSendMessageDto
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        [JsonProperty("msgtype")]
        public string MsgType { get; }
    }

    /// <summary>
    /// 内容信息
    /// </summary>
    public class SendMentionedUser
    {
        /// <summary>
        /// 文本内容，最长不超过2048个字节，必须是utf8编码
        /// markdown内容，最长不超过4096个字节，必须是utf8编码
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// userid的列表，提醒群中的指定成员(@某个成员)，@all表示提醒所有人，如果开发者获取不到userid，可以使用mentioned_mobile_list
        /// </summary>
        [JsonProperty("mentioned_list")]
        public List<string> MentionedList { get; set; }

        /// <summary>
        /// 手机号列表，提醒手机号对应的群成员(@某个成员)，@all表示提醒所有人
        /// </summary>
        [JsonProperty("mentioned_mobile_list")]
        public List<string> MentionedMobileList { get; set; }
    }

    /// <summary>
    /// 图片内容
    /// </summary>
    public class ImageContentDto
    {
        /// <summary>
        /// 图片内容的base64编码
        /// </summary>
        /// <remarks>图片（base64编码前）最大不能超过2M，支持JPG,PNG格式</remarks>
        [JsonProperty("base64")]
        public string Base64 { get; set; }

        /// <summary>
        /// 图片内容（base64编码前）的md5值
        /// </summary>
        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }

    /// <summary>
    /// 文章内容
    /// </summary>
    public class ArticlesContentDto
    {
        /// <summary>
        /// 标题，不超过128个字节，超过会自动截断
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 描述，不超过512个字节，超过会自动截断
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 点击后跳转的链接。
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 图文消息的图片链接，支持JPG、PNG格式，较好的效果为大图 1068*455，小图150*150。
        /// </summary>
        [JsonProperty("picurl")]
        public string PicUrl { get; set; }
    }
}