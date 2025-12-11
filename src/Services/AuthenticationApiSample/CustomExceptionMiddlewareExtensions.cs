// using Azrng.Core.Exceptions;
// using Azrng.Core.Results;
// using Newtonsoft.Json;
// using System.Net;
//
// namespace AuthenticationApiSample
// {
//     /// <summary>
//     /// 全局异常中间件
//     /// </summary>
//     public class CustomExceptionMiddleware
//     {
//         private readonly RequestDelegate _next;
//         private readonly ILogger<CustomExceptionMiddleware> _logger;
//
//         public CustomExceptionMiddleware(RequestDelegate next,
//             ILogger<CustomExceptionMiddleware> logger)
//         {
//             _next = next;
//             _logger = logger;
//         }
//
//         public async Task Invoke(HttpContext context)
//         {
//             try
//             {
//                 _logger.LogInformation($"{context.Request.Path} request");
//
//                 if (!context.Response.HasStarted) //先判断context.Response.HasStarted
//                 {
//                     await _next.Invoke(context);
//                 }
//
//                 _logger.LogInformation(
//                     $"{context.Request.Path} response with status code {context.Response.StatusCode}");
//             }
//             catch (Exception ex)
//             {
//                 await HandleExceptionAsync(context, ex);
//             }
//         }
//
//         private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
//         {
//             if (ex == null) return;
//             await WriteExceptionAsync(httpContext, ex);
//         }
//
//         private async Task WriteExceptionAsync(HttpContext context, Exception exception)
//         {
//             var xRequested = context.Response.Headers.TryGetValue("X-Requestid", out var header)
//                 ? header.ToString()
//                 : context.Response?.GetHashCode() + string.Empty;
//
//             _logger.LogError(
//                 $@"统一日志记录异常-{context.Request.Path} request had an exception, xRequestId:{xRequested},
//                 message:{exception.Message}{exception.InnerException?.Message},stackTrace:{exception.StackTrace},time:{DateTime.Now}");
//
//             var result = new ResultModel { Message = "系统异常,请联系管理员", IsSuccess = false };
//
//             //状态码
//             switch (exception)
//             {
//                 case ForbiddenException ua:
//                     context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
//                     result.Code = ua.ErrorCode;
//                     result.Message = ua.Message;
//                     break;
//
//                 case NotFoundException enf:
//                     context.Response.StatusCode = (int)HttpStatusCode.NotFound;
//                     result.Code = enf.ErrorCode;
//                     result.Message = enf.Message;
//                     break;
//
//                 case ParameterException inp:
//                     context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
//                     result.Code = inp.ErrorCode;
//                     result.Message = inp.Message;
//                     break;
//
//                 case InternalServerException ser:
//                     context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//                     result.Code = ser.ErrorCode;
//                     result.Message = ser.Message;
//                     break;
//
//                 case BaseException bc:
//                     context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//                     result.Code = bc.ErrorCode;
//                     result.Message = bc.Message;
//                     break;
//
//                 default:
//                     context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//                     result.Code = ((int)HttpStatusCode.InternalServerError).ToString();
//                     break;
//             }
//
//             context.Response.StatusCode = (int)HttpStatusCode.OK;
//
//             context.Response.ContentType = "application/json; charset=utf-8";
//
//             var setting = new JsonSerializerSettings
//             {
//                 ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
//             };
//             await context.Response.WriteAsync(JsonConvert.SerializeObject(result, setting));
//         }
//     }
// }