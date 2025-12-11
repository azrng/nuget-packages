using Microsoft.AspNetCore.Mvc;

namespace APIStudy.Controllers
{
    /// <summary>
    /// base控制器
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        //protected virtual OkObjectResult Success()
        //{
        //    return Ok(new ApiResult
        //    {
        //        IsSuccess = true
        //    });
        //}

        //protected virtual OkObjectResult Success<T>(T data, string message = "")
        //{
        //    if (data == null)
        //    {
        //        return Ok(new ApiResult<T>
        //        {
        //            IsSuccess = true,
        //            Message = "没有查询到数据"
        //        });
        //    }

        //    return Ok(new ApiResult<T>
        //    {
        //        IsSuccess = true,
        //        Message = message,
        //        Data = data
        //    });
        //}

        //protected virtual OkObjectResult Error(string message)
        //{
        //    return Ok(new ApiResult
        //    {
        //        IsSuccess = false,
        //        Message = message
        //    });
        //}

        //protected virtual OkObjectResult Error(string message, string errorCode)
        //{
        //    return Ok(new ApiResult
        //    {
        //        IsSuccess = false,
        //        Message = message,
        //        ErrorCode = errorCode
        //    });
        //}
    }
}