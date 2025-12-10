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
        public static Task<T> RunTimeLimitAsync<T>(Func<Task<T>> func, TimeSpan timeout)
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var task = Task.Run(func, cancellationToken);

            if (!task.Wait(timeout))
            {
                tokenSource.Cancel();
                throw new TimeoutException("操作超时");
            }

            return task;
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
        /// 执行Func<T>委托，直到成功或达到最大尝试次数，并返回结果
        /// </summary>
        /// <param name="func"></param>
        /// <param name="maxAttempts"></param>
        /// <param name="delayInMilliseconds">等待的时间</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ExecuteFuncWithRetryAsync<T>(Func<Task<T>> func, int maxAttempts, int delayInMilliseconds = 1000)
        {
            var attempts = 0;
            var result = default(T);
            while (attempts < maxAttempts)
            {
                try
                {
                    result = await func.Invoke();
                    Console.WriteLine("方法执行成功。");
                    return result; // 成功则返回结果并退出循环
                }
                catch (Exception ex)
                {
                    attempts++;
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    if (attempts == maxAttempts)
                    {
                        Console.WriteLine("达到最大尝试次数，执行失败。");
                        return result; // 达到最大尝试次数，返回结果（可能是默认值）
                    }

                    await Task.Delay(delayInMilliseconds);
                }
            }

            return result;
        }

        /// <summary>
        /// 执行Func<T>委托，直到成功或达到最大尝试次数，并返回结果
        /// </summary>
        /// <param name="func"></param>
        /// <param name="maxAttempts"></param>
        /// <param name="delayInMilliseconds">等待的时间</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ExecuteFuncWithRetry<T>(Func<T> func, int maxAttempts, int delayInMilliseconds = 1000)
        {
            var attempts = 0;
            var result = default(T);
            while (attempts < maxAttempts)
            {
                try
                {
                    result = func.Invoke();
                    Console.WriteLine("方法执行成功。");
                    return result; // 成功则返回结果并退出循环
                }
                catch (Exception ex)
                {
                    attempts++;
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    if (attempts == maxAttempts)
                    {
                        Console.WriteLine("达到最大尝试次数，执行失败。");
                        return result; // 达到最大尝试次数，返回结果（可能是默认值）
                    }

                    Thread.Sleep(delayInMilliseconds);
                }
            }

            return result;
        }

        /// <summary>
        /// 执行Action委托，直到成功或达到最大尝试次数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxAttempts"></param>
        /// <param name="delayInMilliseconds">等待的时间</param>
        public static void ExecuteActionWithRetry(Action action, int maxAttempts, int delayInMilliseconds = 1000)
        {
            var attempts = 0;
            while (attempts < maxAttempts)
            {
                try
                {
                    action.Invoke();
                    Console.WriteLine("方法执行成功。");
                    break; // 成功则退出循环
                }
                catch (Exception ex)
                {
                    attempts++;
                    Console.WriteLine($"尝试第{attempts}/{maxAttempts}次执行失败: {ex.Message}");
                    if (attempts == maxAttempts)
                    {
                        Console.WriteLine("达到最大尝试次数，执行失败。");
                        break; // 达到最大尝试次数
                    }

                    Thread.Sleep(delayInMilliseconds);
                }
            }
        }
    }
}