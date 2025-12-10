using System.Collections.Generic;

namespace Azrng.Core.Results
{
    /// <summary>
    /// 返回模型类接口
    /// </summary>
    public interface IResultModel
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 是否失败
        /// </summary>
        bool IsFailure { get; }

        /// <summary>
        /// 消息
        /// </summary>
        string Message { get; }

        /// <summary>
        /// 状态码
        /// </summary>
        string Code { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        IEnumerable<ErrorInfo> Errors { get; }
    }

    public interface IResultModel<T> : IResultModel
    {
        /// <summary>
        /// 数据
        /// </summary>
        T Data { get; set; }
    }

    /// <summary>
    /// 错误信息返回类
    /// </summary>
    public struct ErrorInfo
    {
        /// <summary>
        /// 参数领域
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }
    }
}