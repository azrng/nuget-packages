using Azrng.AspNetCore.Core.Extension;
using Azrng.Core.Exceptions;
using Azrng.Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.AspNetCore.Core.Middleware
{
    /// <summary>
    /// 全局异常中间件
    /// </summary>
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;
        private readonly CommonMvcConfig _config;

        /// <summary>
        /// 异常响应序列化选项，复用同一个实例避免每次请求重新构建缓存
        /// </summary>
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 关闭默认转义
            ReferenceHandler = ReferenceHandler.IgnoreCycles, // 忽略循环引用
            ReadCommentHandling = JsonCommentHandling.Skip, //跳过注释
            AllowTrailingCommas = true, // 允许尾随逗号
        };

        public CustomExceptionMiddleware(RequestDelegate next,
                                         ILogger<CustomExceptionMiddleware> logger,
                                         IOptions<CommonMvcConfig>? config = null)
        {
            _next = next;
            _logger = logger;
            // 通过 IOptions 注入，使 Configure<CommonMvcConfig> 设置的选项真正生效
            _config = config?.Value ?? new CommonMvcConfig();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                _logger.LogInformation("{UrlAddress} request", context.Request.GetUrl());

                await _next.Invoke(context);

                _logger.LogInformation("{UrlAddress} response with status code {Code}",
                    context.Request.GetUrl(), context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                // 异常处理本身可能再次抛出（如序列化失败、响应已开始写入），二次异常单独记录避免污染原始错误
                try
                {
                    await HandleExceptionAsync(context, ex);
                }
                catch (Exception secondary)
                {
                    var traceId = Activity.Current != null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier;
                    _logger.LogError(secondary,
                        "异常处理过程中发生二次异常-{Url} xRequestId:{TraceId} time:{Time}",
                        context.Request.GetUrl(), traceId, DateTime.Now);
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception? ex)
        {
            if (ex == null) return;
            await WriteExceptionAsync(httpContext, ex);
        }

        private async Task WriteExceptionAsync(HttpContext context, Exception exception)
        {
            var xRequested = Activity.Current != null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier;

            _logger.LogError(exception,
                "统一日志记录异常-{Url} request had an exception, xRequestId:{TraceId}, time:{Time}",
                context.Request.GetUrl(), xRequested, DateTime.Now);

            var result = new ResultModel { Message = "系统异常,请联系管理员", IsSuccess = false };

            //状态码
            switch (exception)
            {
                case ForbiddenException ua:
                    // ForbiddenException 语义为禁止访问（已认证但无权限），对应 HTTP 403
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    result.Code = ((int)HttpStatusCode.Forbidden).ToString();
                    result.Message = ua.Message;
                    break;

                case NotFoundException enf:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    result.Code = enf.ErrorCode;
                    result.Message = enf.Message;
                    break;

                case ParameterException inp:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    result.Code = inp.ErrorCode;
                    result.Message = inp.Message;
                    break;

                case LogicBusinessException inp:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    result.Code = inp.ErrorCode;
                    result.Message = inp.Message;
                    break;

                case InternalServerException ser:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result.Code = ser.ErrorCode;
                    result.Message = ser.Message;
                    break;

                case BaseException bc:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result.Code = bc.ErrorCode;
                    result.Message = bc.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result.Code = ((int)HttpStatusCode.InternalServerError).ToString();
                    break;
            }

            if (!_config.UseHttpStateCode)
                context.Response.StatusCode = (int)HttpStatusCode.OK;

            context.Response.ContentType = "application/json; charset=utf-8";

            await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonSerializerOptions));
        }
    }
}
