	﻿using Azrng.AspNetCore.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace Azrng.AspNetCore.Core.Middleware
{
    /// <summary>
    /// 显示所有服务的中间件
    /// </summary>
    public class ShowAllServicesMiddleware
    {
        private readonly ShowServiceConfig _config;
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;

        public ShowAllServicesMiddleware(RequestDelegate next, IOptions<ShowServiceConfig> config,
            IHostEnvironment environment)
        {
            _config = config.Value;
            _next = next;
            _environment = environment;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path == _config.Path)
            {
                // 服务清单泄露内部架构信息，非开发环境默认拒绝；确有需要时可在配置中显式放开
                if (!_environment.IsDevelopment() && !_config.AllowNonDevelopment)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                var stringBuilder = new StringBuilder();
                stringBuilder.Append("<h1>All Services</h1>");
                stringBuilder.Append("<table><thead>");
                stringBuilder.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
                stringBuilder.Append("</thead><tbody>");
                foreach (var service in _config.Services)
                {
                    stringBuilder.Append("<tr>");
                    stringBuilder.Append("<td>" + WebUtility.HtmlEncode(service.ServiceType.FullName) + "</td>");
                    stringBuilder.Append($"<td>{service.Lifetime}</td>");
                    stringBuilder.Append("<td>" + WebUtility.HtmlEncode(service.ImplementationType?.FullName) + "</td>");
                    stringBuilder.Append("</tr>");
                }

                stringBuilder.Append("</tbody></table>");
                await httpContext.Response.WriteAsync(stringBuilder.ToString());
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
