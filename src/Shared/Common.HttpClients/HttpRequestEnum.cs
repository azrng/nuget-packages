using System.ComponentModel;

namespace Common.HttpClients
{
    /// <summary>
    /// http请求枚举
    /// </summary>
    public enum HttpRequestEnum
    {
        /// <summary>
        /// HttpGet
        /// </summary>
        [Description("HttpGet")]
        Get = 0,

        /// <summary>
        /// HttpPut
        /// </summary>
        [Description("HttpPut")]
        Put = 1,

        /// <summary>
        /// HttpPost
        /// </summary>
        [Description("HttpPost")]
        Post = 2,

        /// <summary>
        /// HttpDelete
        /// </summary>
        [Description("HttpDelete")]
        Delete = 3,
    }
}