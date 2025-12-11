using Newtonsoft.Json;

namespace Common.Requests.Lay
{
    /// <summary>
    /// 基础分页请求类
    /// </summary>
    public class LayPageRequest
    {
        /// <summary>
        /// 页码
        /// </summary>
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页数
        /// </summary>
        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 关键词
        /// </summary>
        [JsonProperty("keyWord")]
        public string KeyWord { get; set; } = string.Empty;
    }
}
