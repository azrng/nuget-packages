using System.Reflection;
using Azrng.AspNetCore.Job.Quartz;
using FluentAssertions;
using Quartz;
using Xunit;

namespace Azrng.AspNetCore.Job.Quartz.Test
{
    public class CronPresetsTests
    {
        /// <summary>
        /// 所有公开常量必须是合法的 Quartz Cron 表达式
        /// </summary>
        [Fact]
        public void AllPresets_ShouldBeValidQuartzCronExpressions()
        {
            var presets = typeof(CronPresets)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (Name: f.Name, Value: (string)f.GetRawConstantValue()!))
                .ToList();

            presets.Should().NotBeEmpty("应至少内置一组常用 Cron 预设");

            var invalid = presets
                .Where(p => !CronExpression.IsValidExpression(p.Value))
                .ToList();

            invalid.Should().BeEmpty("以下预设不是合法的 Quartz Cron 表达式: {0}",
                string.Join(", ", invalid.Select(p => $"{p.Name}={p.Value}")));
        }

        [Theory]
        [InlineData(CronPresets.Every5Seconds)]
        [InlineData(CronPresets.EveryMinute)]
        [InlineData(CronPresets.EveryHour)]
        [InlineData(CronPresets.EveryDayMidnight)]
        [InlineData(CronPresets.EveryMondayMidnight)]
        [InlineData(CronPresets.MonthlyFirstDayMidnight)]
        [InlineData(CronPresets.MonthlyLastDayAt11Pm)]
        public void KeyPresets_ShouldBeValid(string cron)
        {
            CronExpression.IsValidExpression(cron).Should().BeTrue();
        }

        /// <summary>
        /// 验证关键预设的语义正确性：给定固定基准时间，下次触发时间应落在预期时刻。
        /// 使用固定 UTC 时区与固定基准时间，确保测试不依赖运行机器时区与当前时间。
        /// </summary>
        [Theory]
        // 基准：2026-07-10 00:00:00 UTC（周五）
        [InlineData(CronPresets.Every5Seconds,           "2026-07-10T00:00:00+00:00", "2026-07-10T00:00:05+00:00")]
        [InlineData(CronPresets.EveryMinute,             "2026-07-10T00:00:00+00:00", "2026-07-10T00:01:00+00:00")]
        [InlineData(CronPresets.EveryHour,               "2026-07-10T00:00:00+00:00", "2026-07-10T01:00:00+00:00")]
        [InlineData(CronPresets.EveryDayMidnight,        "2026-07-10T00:00:00+00:00", "2026-07-11T00:00:00+00:00")]
        [InlineData(CronPresets.EveryDayAt6Am,           "2026-07-10T00:00:00+00:00", "2026-07-10T06:00:00+00:00")]
        [InlineData(CronPresets.EveryDayNoon,            "2026-07-10T00:00:00+00:00", "2026-07-10T12:00:00+00:00")]
        [InlineData(CronPresets.EveryMondayMidnight,     "2026-07-10T00:00:00+00:00", "2026-07-13T00:00:00+00:00")]
        [InlineData(CronPresets.EverySundayMidnight,     "2026-07-10T00:00:00+00:00", "2026-07-12T00:00:00+00:00")]
        [InlineData(CronPresets.MonthlyFirstDayMidnight, "2026-07-10T00:00:00+00:00", "2026-08-01T00:00:00+00:00")]
        [InlineData(CronPresets.MonthlyLastDayAt11Pm,    "2026-07-10T00:00:00+00:00", "2026-07-31T23:00:00+00:00")]
        public void KeyPresets_ShouldFireAtExpectedTime(string cron, string afterIso, string expectedIso)
        {
            var after = DateTimeOffset.Parse(afterIso);
            var expected = DateTimeOffset.Parse(expectedIso);

            // 固定 UTC 时区，使测试不依赖运行机器本地时区
            var expression = new CronExpression(cron)
            {
                TimeZone = TimeZoneInfo.Utc
            };

            var next = expression.GetNextValidTimeAfter(after);

            next.Should().NotBeNull();
            var actual = next ?? throw new InvalidOperationException("未计算出下次触发时间");
            actual.UtcDateTime.Should().Be(expected.UtcDateTime,
                "预设 {0} 的下次触发时间应符合其语义", cron);
        }
    }
}
