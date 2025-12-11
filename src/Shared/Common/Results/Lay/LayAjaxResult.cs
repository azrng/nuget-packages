using Newtonsoft.Json;

namespace Common.Results.Lay
{
    /// <summary>
    /// 通用AJAX请求响应数据格式模型。
    /// </summary>
    public class LayAjaxResult
    {
        public LayAjaxResult(ResultType state, string message, object data = null)
        {
            State = state;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// 结果类型。
        /// </summary>
        [JsonProperty("state")]
        public ResultType State { get; set; }

        /// <summary>
        /// 消息内容。
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// 返回数据。
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// 结果类型枚举。
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 警告。
        /// </summary>
        Warning = 0,

        /// <summary>
        /// 成功。
        /// </summary>
        Success = 1,

        /// <summary>
        /// 异常。
        /// </summary>
        Error = 2,

        /// <summary>
        /// 消息。
        /// </summary>
        Info = 6
    }
}