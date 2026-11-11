using Azrng.Core.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 任务帮助类
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// 运行时间限制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">操作</param>
        /// <param name="timeout">限制时间</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<T> RunTimeLimitAsync<T>(Func<Task<T>> func, TimeSpan timeout)
        {
            // WhenAny 等待替代 task.Wait 同步阻塞：避免调用线程死锁、保留原始异常（而非 AggregateException）
            using var tokenSource = new CancellationTokenSource();

            var task = Task.Run(func, tokenSource.Token);

            var completed = await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);
            if (completed != task)
            {
                // 超时后放弃等待；func 未支持协作取消时任务仍会在后台跑完，其结果与异常被丢弃
                throw new TimeoutException("操作超时");
            }

            return await task.ConfigureAwait(false);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// 尝试等待指定时间 直到成功或者超时
        /// </summary>
        /// <param name="func"></param>
        /// <param name="timeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>成功/超时</returns>
        public static async Task<IResultModel<T>> TryWaitAsync<T>(Func<Task<IResultModel<T>>> func, TimeSpan timeout)
        {
            using var time = new PeriodicTimer(TimeSpan.FromSeconds(1));

            var endTimeUtc = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < endTimeUtc && await time.WaitForNextTickAsync())
            {
                var result = await func();
                if (result.IsSuccess)
                    return result;
            }

            return ResultModel<T>.Error("超时", "408");
        }
#endif

        /// <summary>
        /// 执行Func&lt;T&gt;委托，直到成功或达到最大尝试次数，并返回结果
        /// </summary>
        /// <param name="func"></param>
        /// <param name="maxAttempts"></param>
        /// <param name="delayInMilliseconds">等待的时间</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T?> ExecuteFuncWithRetryAsync<T>(Func<Task<T>> func, int maxAttempts, int delayInMilliseconds = 1000)
        {
            if (maxAttempts < 1)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            if (delayInMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(delayInMilliseconds));

            for (var attempts = 1; attempts <= maxAttempts; attempts++)
            {
                try
                {
                    var result = await func.Invoke();
                    Console.WriteLine("方法执行成功。");
                    return result;
                }
                catch (Exception ex) when (attempts < maxAttempts)
                {
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    await Task.Delay(delayInMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    Console.WriteLine("达到最大尝试次数，执行失败。");
                    throw;
                }
            }

            throw new InvalidOperationException("重试执行流程异常结束。");
        }

        /// <summary>
        /// 执行Func&lt;T&gt;委托，直到成功或达到最大尝试次数，并返回结果
        /// </summary>
        /// <param name="func"></param>
        /// <param name="maxAttempts"></param>
        /// <param name="delayInMilliseconds">等待的时间</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? ExecuteFuncWithRetry<T>(Func<T> func, int maxAttempts, int delayInMilliseconds = 1000)
        {
            if (maxAttempts < 1)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            if (delayInMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(delayInMilliseconds));

            for (var attempts = 1; attempts <= maxAttempts; attempts++)
            {
                try
                {
                    var result = func.Invoke();
                    Console.WriteLine("方法执行成功。");
                    return result;
                }
                catch (Exception ex) when (attempts < maxAttempts)
                {
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    Thread.Sleep(delayInMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    Console.WriteLine("达到最大尝试次数，执行失败。");
                    throw;
                }
            }

            throw new InvalidOperationException("重试执行流程异常结束。");
        }

        /// <summary>
        /// 执行Action委托，直到成功或达到最大尝试次数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxAttempts"></param>
        /// <param name="delayInMilliseconds">等待的时间</param>
        public static void ExecuteActionWithRetry(Action action, int maxAttempts, int delayInMilliseconds = 1000)
        {
            if (maxAttempts < 1)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            if (delayInMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(delayInMilliseconds));

            for (var attempts = 1; attempts <= maxAttempts; attempts++)
            {
                try
                {
                    action.Invoke();
                    Console.WriteLine("方法执行成功。");
                    return;
                }
                catch (Exception ex) when (attempts < maxAttempts)
                {
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    Thread.Sleep(delayInMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    Console.WriteLine("达到最大尝试次数，执行失败。");
                    throw;
                }
            }
        }
    }
}