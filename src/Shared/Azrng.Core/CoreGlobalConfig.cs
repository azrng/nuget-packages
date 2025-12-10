using Azrng.Core.Enums;

namespace Azrng.Core
{
    public static class CoreGlobalConfig
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// 是否清理本地日志
        /// </summary>
        public static bool IsClearLocalLog { get; set; } = true;

        /// <summary>
        /// 日志清理间隔
        /// </summary>
        public static int CleanupInterval { get; set; } = 7;
    }
}