using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Results.Lay
{
    /// <summary>
    /// 菜单视图模型。
    /// </summary>
    public class LayNavbar
    {
        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
        /// <summary>
        ///  图标
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }
        /// <summary>
        ///  是否展开
        /// </summary>
        [JsonProperty("spread")]
        public bool Spread { get; set; }
        /// <summary>
        /// 子级菜单集合
        /// </summary>
        [JsonProperty("children")]
        public List<LayChildNavbar> Children { get; set; }
    }

    /// <summary>
    /// 子级菜单模型
    /// </summary>
    public class LayChildNavbar
    {
        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        [JsonProperty("href")]
        public string Href { get; set; }
    }
}
