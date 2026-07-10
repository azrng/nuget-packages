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
    }
}
