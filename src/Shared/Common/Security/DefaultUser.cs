// using Common.Core;
// using Common.Core.DependencyInjection;
// using Common.Extension;
// using Microsoft.AspNetCore.Http;
// using System;
// using System.Security.Claims;
//
// namespace Common.Security
// {
//     /// <summary>
//     /// 默认从上下文取值的方案实现
//     /// </summary>
//     public class DefaultUser : IScopedDependency, ICurrentUser<long>
//     {
//         private readonly IHttpContextAccessor _httpContextAccessor;
//
//         public DefaultUser(IHttpContextAccessor httpContextAccessor)
//         {
//             if (httpContextAccessor is null)
//                 throw new ArgumentNullException("IHttpContextAccessor未注册，请注册");
//
//             _httpContextAccessor = httpContextAccessor;
//         }
//
//         public long UserId =>
//             _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToInt64() ?? 0;
//
//         public string UserName =>
//             _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
//
//         public string NickName =>
//             _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
//     }
// }