using Newtonsoft.Json;
using System;

namespace Common.YuQueSdk.Dto.Doc
{
    /// <summary>
    /// 获取简化的文档详情返回类
    /// </summary>
    public class GetDocsMdBodyResponse
    {
        /// <summary>
        /// 文档编号
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文 Markdown 源代码
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// 文档内容更新时间
        /// </summary>
        [JsonProperty("content_updated_at")]
        public DateTime ContentUpdatedAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}