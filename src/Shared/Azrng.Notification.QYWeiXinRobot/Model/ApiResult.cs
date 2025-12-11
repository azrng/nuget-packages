using Newtonsoft.Json;

namespace Azrng.Notification.QYWeiXinRobot.Model
{
    /// <summary>
    /// 消息返回类
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty("errcode")]
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [JsonProperty("errmsg")]
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => Code == 0;
    }

    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 内容
        /// </summary>
        public T Data { get; set; }
    }
}