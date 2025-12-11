using Newtonsoft.Json;

namespace Common.YuQueSdk.Dto.Doc
{
    /// <summary>
    /// 获取保存文档信息的返回类
    /// </summary>
    public class SaveDocResult
    {
        /// <summary>
        /// 文档编号
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 文档标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文 Markdown 源代码
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}