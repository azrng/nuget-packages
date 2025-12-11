using Newtonsoft.Json;

namespace Common.YuQueSdk.Dto
{
    /// <summary>
    /// 语雀返回类
    /// </summary>
    public abstract class YuQueResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => Status == null;

        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty("status")]
        public int? Status { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// 拥有的权限
        /// </summary>
        [JsonProperty("abilities")]
        public AbilitiesDto Abilities { get; set; }
    }

    /// <summary>
    /// 语雀返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class YuQueResult<T> : YuQueResult
    {
        /// <summary>
        /// 结果
        /// </summary>
        public T Data { get; set; }
    }

    /// <summary>
    /// 拥有权限
    /// </summary>
    public class AbilitiesDto
    {
        /// <summary>
        /// 是否有权限修改
        /// </summary>
        [JsonProperty("update")]
        public bool Update { get; set; }

        /// <summary>
        /// 是否有权限销毁
        /// </summary>
        [JsonProperty("destroy")]
        public bool Destroy { get; set; }
    }
}