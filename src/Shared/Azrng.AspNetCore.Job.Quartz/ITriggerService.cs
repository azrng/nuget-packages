using Azrng.AspNetCore.Job.Quartz.Model;
using Quartz;

namespace Azrng.AspNetCore.Job.Quartz
{
    public interface ITriggerService
    {
        ITrigger CreateStarNowTrigger(ScheduleViewModel entity);

        ITrigger CreateCronTrigger(ScheduleViewModel entity);

        ITrigger CreateSimpleTrigger(ScheduleViewModel entity);
    }
}
