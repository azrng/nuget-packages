using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 模板卡片类型消息
    /// </summary>
    public class TemplateCardMessageDto : IBaseSendMessageDto
    {
        /// <summary>
        /// 模板卡片消息
        /// </summary>
        public string MsgType => "template_card";

        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty("template_card")]
        public BaseTemplateCardDto TemplateCard { get; set; }
    }

    /// <summary>
    /// 文本通知模板卡片
    /// </summary>
    public class TextNoticeTemplateCard : BaseTemplateCardDto
    {
        /// <summary>
        /// 文本通知卡片
        /// </summary>
        public override string CardType => "text_notice";

        /// <summary>
        /// 关键数据样式
        /// </summary>
        /// <remarks>
        /// title:关键数据样式的数据内容，建议不超过10个字;desc:关键数据样式的数据描述内容，建议不超过15个字
        /// </remarks>
        [JsonProperty("emphasis_content")]
        public TitleContentDto Emphasiscontent { get; set; }

        /// <summary>
        /// 二级普通文本，建议不超过112个字。
        /// </summary>
        [JsonProperty("sub_title_text")]
        public string SubTitleText { get; set; }
    }

    /// <summary>
    /// 图文展示模板卡片
    /// </summary>
    public class NewsNoticeTemplateCard : BaseTemplateCardDto
    {
        /// <summary>
        /// 文本通知卡片
        /// </summary>
        public override string CardType => "news_notice";

        /// <summary>
        /// 图片样式
        /// </summary>
        [JsonProperty("card_image")]
        public CardImageStyleDto CardImage { get; set; }

        /// <summary>
        /// 左图右文样式
        /// </summary>
        [JsonProperty("image_text_area")]
        public ImageTextAreaStyleDto ImageTextArea { get; set; }

        /// <summary>
        /// 卡片二级垂直内容，该字段可为空数组，但有数据的话需确认对应字段是否必填，列表长度不超过4
        /// </summary>
        [JsonProperty("vertical_content_list")]
        public List<TitleContentDto> VerticalContentList { get; set; }
    }
}