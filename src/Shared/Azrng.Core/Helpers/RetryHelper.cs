using Azrng.Core.Extension;
using Azrng.Core.RetryTask;
using System;
using System.Threading.Tasks;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 重试辅助类，提供便捷的重试功能
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// 执行带重试的异步操作
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="operation">要执行的操作（lambda表达式）</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        /// <returns></returns>
        /// <remarks>默认配置只有当遇到抛出RetryMarkException异常使用才会重试</remarks>
        /// <example>
        /// <code>
        /// // 使用示例
        /// var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3);
        ///
        /// // 带参数的方法
        /// var result = await RetryHelper.ExecuteAsync(() => ProcessAsync(id, name), 3);
        /// </code>
        /// </example>
        public static IRetryTask<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> operation,
            int maxRetryCount)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (maxRetryCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

            return new FuncTask<TResult>(operation)
                .Retry(maxRetryCount);
        }

        /// <summary>
        /// 执行带重试的异步操作（带固定延时）
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="operation">要执行的操作</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        /// <param name="delay">重试延时</param>
        /// <remarks>默认配置只有当遇到抛出RetryMarkException异常使用才会重试</remarks>
        /// <returns></returns>
        public static IRetryTask<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> operation,
            int maxRetryCount,
            TimeSpan delay)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (maxRetryCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

            return new FuncTask<TResult>(operation)
                .Retry(maxRetryCount, delay);
        }

        /// <summary>
        /// 执行带重试的异步操作（带动态延时策略）
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="operation">要执行的操作</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        /// <param name="delayStrategy">延时策略（参数为重试索引）</param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// // 指数退避重试
        /// var result = await RetryHelper.ExecuteAsync(
        ///     () => GetDataAsync(),
        ///     maxRetryCount: 3,
        ///     delayStrategy: i => TimeSpan.FromSeconds(Math.Pow(2, i))
        /// );
        /// </code>
        /// </example>
        public static IRetryTask<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> operation,
            int maxRetryCount,
            Func<int, TimeSpan> delayStrategy)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (maxRetryCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

            return new FuncTask<TResult>(operation)
                .Retry(maxRetryCount, delayStrategy);
        }
    }
}