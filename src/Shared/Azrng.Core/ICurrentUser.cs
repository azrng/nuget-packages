using Azrng.Core.DependencyInjection;
using System.Collections.Generic;

namespace Azrng.Core
{
    /// <summary>
    /// 当前用户
    /// </summary>
    public interface ICurrentUser<out T> : IScopedDependency
    {
        T Sub { get; }

        /// <summary>
        /// 用户标识
        /// </summary>
        T UserId { get; }

        /// <summary>
        /// 用户名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 昵称
        /// </summary>
        string NickName { get; }

        /// <summary>
        /// 授权信息
        /// </summary>
        List<string> Auds { get; }

        /// <summary>
        /// 令牌
        /// </summary>
        string Token { get; }
    }

    /// <summary>
    /// 当前用户
    /// </summary>
    public interface ICurrentUser : ICurrentUser<string> { }

    // /// <summary>
    // /// 默认当前用户实现
    // /// </summary>
    // public class DefaultCurrentUser : ICurrentUser
    // {
    //     private readonly IHttpContextAccessor _httpContextAccessor;
    //
    //     public DefaultICurrentUser(IHttpContextAccessor httpContextAccessor)
    //     {
    //         _httpContextAccessor = httpContextAccessor;
    //     }
    //
    //     public string Sub => _httpContextAccessor.HttpContext.User?.FindFirst("sub")?.Value;
    //
    //     public string UserId => Sub;
    //
    //     public string UserName => _httpContextAccessor.HttpContext.User.FindFirst("name")?.Value;
    //
    //     public string NickName => _httpContextAccessor.HttpContext.User.FindFirst("nickname")?.Value;
    //
    //     public List<string> Auds => _httpContextAccessor.HttpContext.User.FindAll("aud").Select(x => x.Value).ToList();
    //
    //     public string Token => _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
    // }
}