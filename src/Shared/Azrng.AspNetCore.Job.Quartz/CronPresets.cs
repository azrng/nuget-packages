namespace Azrng.AspNetCore.Job.Quartz
{
    /// <summary>
    /// 常用 Cron 表达式预设（Quartz 格式：秒 分 时 日 月 周 [年]）
    /// </summary>
    /// <remarks>
    /// 使用方式：<code>[JobConfig(nameof(HelloJob), "default", CronPresets.Every5Seconds)]</code>
    /// </remarks>
    public static class CronPresets
    {
        // ── 秒 / 分钟级 ──────────────────────────────────────────

        /// <summary>每 5 秒</summary>
        public const string Every5Seconds = "0/5 * * * * ?";

        /// <summary>每 10 秒</summary>
        public const string Every10Seconds = "0/10 * * * * ?";

        /// <summary>每 30 秒</summary>
        public const string Every30Seconds = "0/30 * * * * ?";

        /// <summary>每分钟</summary>
        public const string EveryMinute = "0 * * * * ?";

        // ── 分钟 / 小时级 ────────────────────────────────────────

        /// <summary>每 5 分钟</summary>
        public const string Every5Minutes = "0 0/5 * * * ?";

        /// <summary>每 10 分钟</summary>
        public const string Every10Minutes = "0 0/10 * * * ?";

        /// <summary>每 15 分钟</summary>
        public const string Every15Minutes = "0 0/15 * * * ?";

        /// <summary>每 30 分钟</summary>
        public const string Every30Minutes = "0 0/30 * * * ?";

        /// <summary>每小时整点</summary>
        public const string EveryHour = "0 0 * * * ?";

        // ── 每日固定时刻 ────────────────────────────────────────

        /// <summary>每天 00:00</summary>
        public const string EveryDayMidnight = "0 0 0 * * ?";

        /// <summary>每天 06:00</summary>
        public const string EveryDayAt6Am = "0 0 6 * * ?";

        /// <summary>每天 08:00</summary>
        public const string EveryDayAt8Am = "0 0 8 * * ?";

        /// <summary>每天 12:00</summary>
        public const string EveryDayNoon = "0 0 12 * * ?";

        /// <summary>每天 18:00</summary>
        public const string EveryDayAt6Pm = "0 0 18 * * ?";

        /// <summary>每天 23:00</summary>
        public const string EveryDayAt11Pm = "0 0 23 * * ?";

        // ── 周 / 月级 ───────────────────────────────────────────

        /// <summary>每周一 00:00</summary>
        public const string EveryMondayMidnight = "0 0 0 ? * MON";

        /// <summary>每周日 00:00</summary>
        public const string EverySundayMidnight = "0 0 0 ? * SUN";

        /// <summary>每月 1 号 00:00</summary>
        public const string MonthlyFirstDayMidnight = "0 0 0 1 * ?";

        /// <summary>每月最后一天 23:00（由 Quartz 的 LastDayOfMonth 功能支持）</summary>
        public const string MonthlyLastDayAt11Pm = "0 0 23 L * ?";
    }
}
