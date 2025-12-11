using Newtonsoft.Json;

namespace Common.Results.Lay
{
    /// <summary>
    /// zTree单层节点数据模型。
    /// </summary>
    public class LayZTreeNode
    {
        /// <summary>
        /// 节点ID。
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// 父节点ID。
        /// </summary>
        [JsonProperty("pId")]
        public string PId { get; set; }

        /// <summary>
        /// 节点名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 是否展开。
        /// </summary>
        [JsonProperty("open")]
        public bool Open { get; set; }

        /// <summary>
        /// 是否选中。
        /// </summary>
        [JsonProperty("@checked")]
        public bool Checked { get; set; }
    }
}