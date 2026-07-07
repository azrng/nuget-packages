using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System.Collections.Concurrent;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    /// <summary>
    /// 支持依赖注入的作业工厂
    /// </summary>
    internal class DependencyInjectionJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new();

        public DependencyInjectionJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _serviceProvider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            if (job == null)
            {
                scope.Dispose();
                throw new InvalidOperationException($"无法创建作业实例: {bundle.JobDetail.JobType.Name}。请确保作业类型已注册为服务。");
            }

            // 以 job 实例为键存储 scope，ReturnJob 时据此清理，避免 scope 永不释放
            _scopes[job] = scope;
            return job;
        }

        public void ReturnJob(IJob job)
        {
            // 作业执行完毕，回收对应的 DI scope（修复原先 scope 永不释放的内存泄漏）
            if (job != null && _scopes.TryRemove(job, out var scope))
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// 清理所有尚未回收的 scope（调度器关闭时兜底）
        /// </summary>
        public void DisposeAllScopes()
        {
            foreach (var scope in _scopes.Values)
            {
                try
                {
                    scope.Dispose();
                }
                catch
                {
                    // 忽略清理异常
                }
            }
            _scopes.Clear();
        }
    }
}
