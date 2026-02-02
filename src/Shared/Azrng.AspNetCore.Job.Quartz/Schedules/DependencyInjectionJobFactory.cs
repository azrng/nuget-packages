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
        private readonly ConcurrentDictionary<string, IServiceScope> _scopes = new();

        public DependencyInjectionJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _serviceProvider.CreateScope();
            var jobKey = bundle.JobDetail.Key.ToString();

            // 存储scope以便后续清理
            _scopes.TryAdd(jobKey, scope);

            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            if (job == null)
            {
                throw new InvalidOperationException($"无法创建作业实例: {bundle.JobDetail.JobType.Name}。请确保作业类型已注册为服务。");
            }

            return job;
        }

        public void ReturnJob(IJob job)
        {
            // 清理scope
            if (job is IDisposable disposableJob)
            {
                disposableJob.Dispose();
            }

            // 尝试清理对应的scope
            // 注意：由于我们无法从job实例直接获取jobKey，这里采用保守策略
            // 实际的清理将在作业完成时通过其他方式处理
        }

        /// <summary>
        /// 清理指定作业的scope
        /// </summary>
        /// <param name="jobKey">作业键</param>
        public void DisposeScope(string jobKey)
        {
            if (_scopes.TryRemove(jobKey, out var scope))
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// 清理所有scope
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