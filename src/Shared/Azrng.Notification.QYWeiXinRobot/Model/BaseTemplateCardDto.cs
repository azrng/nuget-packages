using Newtonsoft.Json;
using System.Collections.Generic;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 模板卡片基类
    /// </summary>
    public class BaseTemplateCardDto
    {
        /// <summary>
        /// 模版卡片的模版类型，文本通知模版卡片的类型为text_notice
        /// </summary>
        [JsonProperty("card_type")]
        public virtual string CardType { get; }

        /// <summary>
        /// 卡片来源样式信息，不需要来源样式可不填写
        /// </summary>
        [JsonProperty("source")]
        public CardSourceStyleDto Source { get; set; }

        /// <summary>
        /// 模版卡片的主要内容，包括一级标题和标题辅助信息（必填）
        /// </summary>
        /// <remarks>
        /// title：一级标题，建议不超过26个字；desc:标题辅助信息，建议不超过30个字
        /// </remarks>
        [JsonProperty("main_title")]
        public TitleContentDto MainTitle { get; set; }

        /// <summary>
        /// 引用文献样式，建议不与关键数据共用
        /// </summary>
        [JsonProperty("quote_area")]
        public QuoteAreaStyleDto QuoteArea { get; set; }

        /// <summary>
        /// 二级标题+文本列表，该字段可为空数组，但有数据的话需确认对应字段是否必填，列表长度不超过6
        /// </summary>
        [JsonProperty("horizontal_content_list")]
        public List<HorizontalContentDto> HorizontalContentList { get; set; }

        /// <summary>
        /// 跳转指引样式的列表，该字段可为空数组，但有数据的话需确认对应字段是否必填，列表长度不超过3
        /// </summary>
        [JsonProperty("jump_list")]
        public List<JumpStyleDto> JumpList { get; set; }

        /// <summary>
        /// 整体卡片的点击跳转事件，text_notice模版卡片中该字段为必填项
        /// </summary>
        [JsonProperty("card_action")]
        public CardActionDto CardAction { get; set; }
    }

    /// <summary>
    /// 卡片来源样式
    /// </summary>
    public class CardSourceStyleDto
    {
        /// <summary>
        /// 来源图片的url
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        /// <summary>
        /// 来源图片的描述，建议不超过13个字
        /// </summary>
        [JsonProperty("desc")]
        public string Desc { get; set; }

        /// <summary>
        /// 来源文字的颜色，目前支持：0(默认) 灰色，1 黑色，2 红色，3 绿色
        /// </summary>
        [JsonProperty("desc_color")]
        public int DescColor { get; set; }
    }

    /// <summary>
    /// 标题内容
    /// </summary>
    public class TitleContentDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("desc_color")]
        public string Desc { get; set; }
    }

    /// <summary>
    /// 引用文献样式
    /// </summary>
    public class QuoteAreaStyleDto
    {
        /// <summary>
        /// 引用文献样式区域点击事件，0或不填代表没有点击事件，1 代表跳转url，2 代表跳转小程序
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// 点击跳转的url，quote_area.type是1时必填
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 点击跳转的小程序的appid，quote_area.type是2时必填
        /// </summary>
        [JsonProperty("appid")]
        public string Appid { get; set; }

        /// <summary>
        /// 点击跳转的小程序的pagepath，quote_area.type是2时选填
        /// </summary>
        [JsonProperty("pagepath")]
        public string PagePath { get; set; }

        /// <summary>
        /// 引用文献样式的标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 引用文献样式的引用文案
        /// </summary>
        [JsonProperty("quote_text")]
        public string QuoteText { get; set; }
    }

    /// <summary>
    /// 整体卡片的点击跳转事件
    /// </summary>
    public class CardActionDto
    {
        /// <summary>
        /// 卡片跳转类型，1 代表跳转url，2 代表打开小程序。text_notice模版卡片中该字段取值范围为[1,2]
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// 跳转事件的url，card_action.type是1时必填
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 跳转事件的小程序的appid，card_action.type是2时必填
        /// </summary>
        [JsonProperty("appid")]
        public string Appid { get; set; }

        /// <summary>
        /// 跳转事件的小程序的pagepath，card_action.type是2时选填
        /// </summary>
        [JsonProperty("pagepath")]
        public string PagePath { get; set; }
    }

    /// <summary>
    ///  二级标题内容
    /// </summary>
    public class HorizontalContentDto
    {
        /// <summary>
        /// 二级标题，建议不超过5个字
        /// </summary>
        [JsonProperty("keyname")]
        public string KeyName { get; set; }

        /// <summary>
        /// 二级文本，如果horizontal_content_list.type是2，该字段代表文件名称（要包含文件类型），建议不超过26个字
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// 链接类型，0或不填代表是普通文本，1 代表跳转url，2 代表下载附件，3 代表@员工
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// 链接跳转的url，horizontal_content_list.type是1时必填
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 附件的media_id，horizontal_content_list.type是2时必填
        /// </summary>
        [JsonProperty("media_id")]
        public string MediaId { get; set; }

        /// <summary>
        /// 被@的成员的userid，horizontal_content_list.type是3时必填
        /// </summary>
        [JsonProperty("userid")]
        public string UserId { get; set; }
    }

    /// <summary>
    /// 跳转指引样式
    /// </summary>
    public class JumpStyleDto
    {
        /// <summary>
        /// 跳转链接类型，0或不填代表不是链接，1 代表跳转url，2 代表跳转小程序
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// 跳转链接样式的文案内容，建议不超过13个字
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 跳转链接的url，jump_list.type是1时必填
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 跳转链接的小程序的appid，jump_list.type是2时必填
        /// </summary>
        [JsonProperty("appid")]
        public string Appid { get; set; }

        /// <summary>
        /// 跳转链接的小程序的pagepath，jump_list.type是2时选填
        /// </summary>
        [JsonProperty("pagepath")]
        public string PagePath { get; set; }
    }

    /// <summary>
    /// 卡片图片样式
    /// </summary>
    public class CardImageStyleDto
    {
        /// <summary>
        /// 	图片的url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 图片的宽高比，宽高比要小于2.25，大于1.3，不填该参数默认1.3
        /// </summary>
        [JsonProperty("aspect_ratio")]
        public float AspectRatio { get; set; }
    }

    /// <summary>
    /// 左图右文样式
    /// </summary>
    public class ImageTextAreaStyleDto
    {
        /// <summary>
        /// 左图右文样式区域点击事件，0或不填代表没有点击事件，1 代表跳转url，2 代表跳转小程序
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// 点击跳转的url，image_text_area.type是1时必填
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 左图右文样式的标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 左图右文样式的描述
        /// </summary>
        [JsonProperty("desc")]
        public string Desc { get; set; }

        /// <summary>
        /// 左图右文样式的图片url
        /// </summary>
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// 点击跳转的小程序的appid，必须是与当前应用关联的小程序，image_text_area.type是2时必填
        /// </summary>
        [JsonProperty("appid")]
        public string Appid { get; set; }

        /// <summary>
        /// 点击跳转的小程序的pagepath，image_text_area.type是2时选填
        /// </summary>
        [JsonProperty("pagepath")]
        public string PagePath { get; set; }
    }
}