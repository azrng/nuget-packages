using Azrng.AspNetCore.Job.Quartz.Model;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Azrng.AspNetCore.Job.Quartz
{
    public interface ITriggerService
    {
        ITrigger CreateStarNowTrigger(ScheduleViewModel entity);

        ITrigger CreateCronTrigger(ScheduleViewModel entity);

        ITrigger CreateSimpleTrigger(ScheduleViewModel entity);
    }

    public class TriggerService : ITriggerService
    {
        private readonly ILogger<TriggerService> _logger;

        public TriggerService(ILogger<TriggerService> logger)
        {
            _logger = logger;
        }

        public ITrigger CreateCronTrigger(ScheduleViewModel entity)
        {

            return BeginBuilder(entity)
                   .StartAt(entity.BeginTime.AddHours(-8))
                   .EndAt(entity.EndTime?.AddHours(-8))
                   .WithCronSchedule(entity.Cron, builder =>
                   {
                       // 这次使用donothin策略是因为有的环境会立即触发一次
                       builder.WithMisfireHandlingInstructionDoNothing();

                       //.InTimeZone(TimeZoneInfo.ConvertTime(timeZoneId));
                   })
                   .ForJob(entity.JobName, entity.JobGroup)
                   .Build();
        }

        public ITrigger CreateSimpleTrigger(ScheduleViewModel entity)
        {
            if (!entity.IntervalSecond.HasValue)
            {
                throw new Exception("请设置间隔时间");
            }

            var tiggerBuilder = BeginBuilder(entity);
            Action<SimpleScheduleBuilder> action;
            if (entity.RunTimes.HasValue && entity.RunTimes > 0)
            {
                tiggerBuilder = tiggerBuilder.StartNow();
                action = builder => builder.WithIntervalInSeconds(entity.IntervalSecond.Value)
                                           .WithRepeatCount(entity.RunTimes.Value)
                                           .WithMisfireHandlingInstructionFireNow();
            }
            else
            {
                action = builder => builder.WithIntervalInSeconds(entity.IntervalSecond.Value)
                                           .RepeatForever()
                                           .WithMisfireHandlingInstructionFireNow();
            }

            return tiggerBuilder.StartAt(entity.BeginTime) // 开始时间
                                .EndAt(entity.EndTime) // 结束数据
                                .WithSimpleSchedule(action)
                                .ForJob(entity.JobName, entity.JobGroup)
                                .Build();
        }

        public ITrigger CreateStarNowTrigger(ScheduleViewModel entity)
        {
            return BeginBuilder(entity)
                   .StartNow()
                   .WithSimpleSchedule(x =>
                   {
                       x.WithMisfireHandlingInstructionFireNow();
                   })
                   .ForJob(entity.JobName, entity.JobGroup)
                   .Build();
        }

        private TriggerBuilder BeginBuilder(ScheduleViewModel entity)
        {
            return TriggerBuilder.Create().WithIdentity(entity.JobName, entity.JobGroup);
        }
    }
}