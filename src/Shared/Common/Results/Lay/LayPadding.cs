using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Results.Lay
{
    /// <summary>
    /// Laytpl + Laypage 分页模型。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class LayPadding<TEntity> where TEntity : class
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 获取结果。
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; } = true;

        /// <summary>
        /// 备注信息。
        /// </summary>
        [JsonProperty("msg")]
        public string Msg { get; set; } = "success";

        /// <summary>
        /// 数据列表。
        /// </summary>
        [JsonProperty("list")]
        public List<TEntity> List { get; set; } = new List<TEntity>();

        [JsonProperty("backgroundImage")]
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 记录条数。
        /// </summary>
        [JsonProperty("count")]
        public long Count { get; set; }
    }

    /// <summary>
    /// layui的table数据表格
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class LayTablePageResult<TEntity> where TEntity : class
    {
        [JsonProperty("code")]
        public int Code { get; set; } = 0;

        /// <summary>
        /// 备注信息。
        /// </summary>
        [JsonProperty("msg")]
        public string Msg { get; set; } = "success";

        /// <summary>
        /// 数据列表。
        /// </summary>
        [JsonProperty("data")]
        public List<TEntity> Data { get; set; } = new List<TEntity>();

        /// <summary>
        /// 记录条数。
        /// </summary>
        [JsonProperty("count")]
        public long Count { get; set; }
    }
}