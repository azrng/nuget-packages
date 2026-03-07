namespace Azrng.AspNetCore.Core
{
    /// <summary>
    /// CORS 策略配置
    /// </summary>
    public class CorsPolicyConfig
    {
        /// <summary>
        /// 是否允许任何来源
        /// </summary>
        /// <remarks>
        /// ⚠️ 注意：仅适用于开发环境。生产环境应该使用 <see cref="AllowedOrigins"/> 明确指定允许的来源。
        /// </remarks>
        public bool AllowAnyOrigin { get; set; }

        /// <summary>
        /// 允许的来源列表
        /// </summary>
        /// <remarks>
        /// 当 <see cref="AllowCredentials"/> 为 true 时，不能使用通配符，必须明确指定来源。
        /// </remarks>
        public string[]? AllowedOrigins { get; set; }

        /// <summary>
        /// 允许的 HTTP 方法列表
        /// </summary>
        /// <remarks>为 null 时表示允许所有方法</remarks>
        public string[]? AllowedMethods { get; set; }

        /// <summary>
        /// 允许的请求头列表
        /// </summary>
        /// <remarks>为 null 时表示允许所有头部</remarks>
        public string[]? AllowedHeaders { get; set; }

        /// <summary>
        /// 是否允许携带凭证（Cookies、认证头等）
        /// </summary>
        /// <remarks>
        /// 当设置为 true 时，<see cref="AllowedOrigins"/> 不能为空或包含通配符。
        /// SignalR 需要启用此选项以支持认证。
        /// </remarks>
        public bool AllowCredentials { get; set; }

        /// <summary>
        /// 允许客户端读取的响应头列表
        /// </summary>
        public string[]? ExposedHeaders { get; set; }

        /// <summary>
        /// 预检请求的缓存时间
        /// </summary>
        /// <remarks>为 null 时表示不缓存预检请求</remarks>
        public TimeSpan? PreflightMaxAge { get; set; }

        /// <summary>
        /// 允许任何来源、方法和头部（开发环境）
        /// </summary>
        /// <returns>当前配置对象，支持链式调用</returns>
        public CorsPolicyConfig AllowAny()
        {
            AllowAnyOrigin = true;
            AllowCredentials = false;
            return this;
        }

        /// <summary>
        /// 设置允许的来源
        /// </summary>
        /// <param name="origins">允许的来源列表</param>
        /// <returns>当前配置对象，支持链式调用</returns>
        public CorsPolicyConfig WithOrigins(params string[] origins)
        {
            AllowedOrigins = origins;
            AllowAnyOrigin = false;
            return this;
        }

        /// <summary>
        /// 设置允许的 HTTP 方法
        /// </summary>
        /// <param name="methods">允许的方法列表</param>
        /// <returns>当前配置对象，支持链式调用</returns>
        public CorsPolicyConfig WithMethods(params string[] methods)
        {
            AllowedMethods = methods;
            return this;
        }

        /// <summary>
        /// 设置允许的请求头
        /// </summary>
        /// <param name="headers">允许的头部列表</param>
        /// <returns>当前配置对象，支持链式调用</returns>
        public CorsPolicyConfig WithHeaders(params string[] headers)
        {
            AllowedHeaders = headers;
            return this;
        }

        /// <summary>
        /// 设置允许客户端读取的响应头
        /// </summary>
        /// <param name="headers">允许暴露的头部列表</param>
        /// <returns>当前配置对象，支持链式调用</returns>
        public CorsPolicyConfig WithExposedHeaders(params string[] headers)
        {
            ExposedHeaders = headers;
            return this;
        }

        /// <summary>
        /// 允许携带凭证
        /// </summary>
        /// <returns>当前配置对象，支持链式调用</returns>
        /// <remarks>
        /// SignalR 需要启用此选项以支持认证。
        /// 启用后必须明确指定 <see cref="AllowedOrigins"/>，不能使用 <see cref="AllowAnyOrigin()"/>
        /// </remarks>
        public CorsPolicyConfig WithCredentials()
        {
            AllowCredentials = true;
            return this;
        }

        /// <summary>
        /// 设置预检请求缓存时间
        /// </summary>
        /// <param name="maxAge">缓存时长</param>
        /// <returns>当前配置对象，支持链式调用</returns>
        /// <remarks>
        /// 设置此选项可以提升性能，减少预检请求的频率。
        /// 建议在生产环境中设置合理的缓存时间（如 1 小时）。
        /// </remarks>
        public CorsPolicyConfig SetPreflightMaxAge(TimeSpan maxAge)
        {
            PreflightMaxAge = maxAge;
            return this;
        }

        /// <summary>
        /// 配置为适用于 SignalR 的 CORS 策略
        /// </summary>
        /// <returns>当前配置对象，支持链式调用</returns>
        /// <remarks>
        /// SignalR 需要：
        /// 1. 明确指定来源（不能使用 AllowAnyOrigin）
        /// 2. 允许携带凭证（支持认证）
        /// 3. 允许所有方法和头部（WebSocket 传输需要）
        /// 4. 设置预检请求缓存时间（提升性能）
        /// </remarks>
        public CorsPolicyConfig ForSignalR(TimeSpan? preflightMaxAge = null)
        {
            AllowCredentials = true;
            PreflightMaxAge = preflightMaxAge ?? TimeSpan.FromHours(1);
            return this;
        }
    }
}