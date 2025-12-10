using Azrng.Core.Results;

namespace Azrng.Core.Service
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public abstract class BaseService
    {
        protected virtual IResultModel Success()
        {
            return new ResultModel(true);
        }

        protected virtual IResultModel<T> Success<T>(T data)
        {
            return new ResultModel<T>(data: data, true, "success", "200");
        }

        protected virtual IResultModel Error(string message = "错误")
        {
            return new ResultModel(false, message, "400");
        }

        protected virtual IResultModel<T> Error<T>(string message = "错误")
        {
            return new ResultModel<T>(default, false, message, "400");
        }

        protected virtual IResultModel Error(string message, string errorCode)
        {
            return new ResultModel(false, message, errorCode);
        }

        protected virtual IResultModel<T> Error<T>(string message, string errorCode)
        {
            return new ResultModel<T>(default, false, message, errorCode);
        }
    }
}