using Azrng.AspNetCore.Job.Quartz.Model;
using Azrng.AspNetCore.Job.Quartz.Services;
using FluentAssertions;
using Xunit;

namespace Azrng.AspNetCore.Job.Quartz.Test
{
    public class ModelTests
    {
        [Fact]
        public void ScheduleViewModel_ShouldHaveDefaultValues()
        {
            var model = new ScheduleViewModel();
            model.JobName.Should().NotBeNull();
            model.JobGroup.Should().Be("default");
        }

        [Fact]
        public void JobExecutionStatistics_ShouldCalculateSuccessRateCorrectly()
        {
            var statistics = new JobExecutionStatistics
            {
                TotalExecutions = 100,
                SuccessCount = 85,
                FailedCount = 15
            };
            statistics.SuccessRate.Should().Be(85.0);
        }
    }
}
