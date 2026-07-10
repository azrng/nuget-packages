using Azrng.AspNetCore.Job.Quartz.Model;
using Azrng.AspNetCore.Job.Quartz.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;
using Xunit;

namespace Azrng.AspNetCore.Job.Quartz.Test
{
    /// <summary>
    /// TriggerService 单元测试，重点验证 Cron 触发器不再硬编码时区偏移
    /// </summary>
    public class TriggerServiceTests
    {
        private readonly TriggerService _service = new(NullLogger<TriggerService>.Instance);

        [Fact]
        public void CreateCronTrigger_ShouldNotApplyTimezoneOffset()
        {
            // 东八区 10:00（对应 UTC 02:00）
            var beginTime = new DateTimeOffset(2026, 7, 7, 10, 0, 0, TimeSpan.FromHours(8));
            var entity = new ScheduleViewModel
            {
                JobName = "j",
                JobGroup = "g",
                BeginTime = beginTime,
                Cron = "0/5 * * * * ?"
            };

            var trigger = _service.CreateCronTrigger(entity);

            // 不应再因旧的 AddHours(-8) 偏移：UTC 应等于 BeginTime 自身的 UTC 值（02:00）
            trigger.StartTimeUtc.UtcDateTime.Should().Be(beginTime.UtcDateTime);
        }

        [Fact]
        public void CreateSimpleTrigger_ShouldRequireInterval()
        {
            var entity = new ScheduleViewModel { JobName = "j", JobGroup = "g" };

            var act = () => _service.CreateSimpleTrigger(entity);

            act.Should().Throw<Exception>().WithMessage("*间隔时间*");
        }
    }
}
