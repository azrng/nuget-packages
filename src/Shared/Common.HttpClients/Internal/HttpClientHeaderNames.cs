namespace Common.HttpClients
{
    /// <summary>
    /// 跨类共享的 HTTP 头名称常量
    /// </summary>
    internal static class HttpClientHeaderNames
    {
        /// <summary>
        /// Fallback 响应标记头，由 Polly 降级响应写入
        /// </summary>
        public const string FallbackResponse = "X-Fallback-Response";

        /// <summary>
        /// 跳过审计日志标记头
        /// </summary>
        public const string SkipLogger = "X-Skip-Logger";

        /// <summary>
        /// 日志级别控制头
        /// </summary>
        public const string Logger = "X-Logger";

        /// <summary>
        /// 分布式追踪 ID 头
        /// </summary>
        public const string TraceId = "X-Trace-Id";
    }
}
