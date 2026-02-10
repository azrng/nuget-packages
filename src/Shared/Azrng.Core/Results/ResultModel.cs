using System.Collections.Generic;

namespace Azrng.Core.Results
{
    /// <summary>
    /// 返回类模型
    /// </summary>
    public class ResultModel : IResultModel
    {
        public ResultModel()
        {
            Code = "200";
            Message = "success";
            Errors = new List<ErrorInfo>();
        }

        public ResultModel(bool isSuccess, string message = "success", string code = "200", IEnumerable<ErrorInfo>? errorInfos = null)
        {
            IsSuccess = isSuccess;
            Code = code;
            Message = message;
            Errors = errorInfos ?? new List<ErrorInfo>();
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 是否失败
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// 状态码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误信息  model验证错误时候输入
        /// </summary>
        public IEnumerable<ErrorInfo> Errors { get; }

        #region 扩展方法

        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        public static IResultModel Success() => new ResultModel(true);

        /// <summary>
        /// 返回错误
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorInfos">model验证错误</param>
        /// <returns></returns>
        public static IResultModel Error(string message, string errorCode = "400",
                                         IEnumerable<ErrorInfo>? errorInfos = null)
        {
            return new ResultModel(false, message, errorCode, errorInfos);
        }

        /// <summary>
        /// 返回错误
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorInfos">model验证错误</param>
        /// <returns></returns>
        public static IResultModel Failure(string message, string errorCode = "400",
                                           IEnumerable<ErrorInfo>? errorInfos = null)
        {
            return new ResultModel(false, message, errorCode, errorInfos);
        }

        #endregion
    }

    /// <summary>
    /// 返回类模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultModel<T> : ResultModel, IResultModel<T>
    {
        public ResultModel() { }

        public ResultModel(T? data, bool isSuccess, string message, string code, IEnumerable<ErrorInfo>? errorInfos = null)
            : base(isSuccess, message, code, errorInfos)
        {
            Data = data;
        }

        /// <summary>
        ///返回的数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IResultModel<T> Success(T data)
        {
            return new ResultModel<T>(data, true, "success", "200");
        }

        /// <summary>
        /// 返回错误
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorInfos">model验证错误信息</param>
        /// <returns></returns>
        public static new IResultModel<T> Error(string message, string errorCode = "400",
                                                IEnumerable<ErrorInfo>? errorInfos = null)
        {
            return new ResultModel<T>(default, false, message, errorCode, errorInfos);
        }

        /// <summary>
        /// 返回错误
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorInfos">model验证错误信息</param>
        /// <returns></returns>
        public static new IResultModel<T> Failure(string message, string errorCode = "400",
                                                  IEnumerable<ErrorInfo>? errorInfos = null)
        {
            return new ResultModel<T>(default, false, message, errorCode, errorInfos);
        }
    }
}