using System;

namespace Azrng.Core.Exceptions
{
    /// <summary>
    /// 未认证（未登录或凭证无效），对应 HTTP 401
    /// </summary>
    /// <remarks>
    /// 与 <see cref="ForbiddenException"/> 区分：
    /// 未认证（不知道调用者是谁）抛 <see cref="UnauthorizedException"/> → 401；
    /// 已认证但无权限抛 <see cref="ForbiddenException"/> → 403。
    /// </remarks>
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException() : base("401", "未认证") { }

        public UnauthorizedException(string message) : base("401", message) { }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
