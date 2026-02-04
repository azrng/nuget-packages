using Azrng.Core.RetryTask;
using System;
using System.Threading.Tasks;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// ITask扩展
    /// </summary>
    /// <remarks>拷贝自：WebApiClientCore</remarks>
    public static class RetryTaskExtensions
    {
        /// <summary>
        /// 返回提供请求重试的请求任务对象(成功不重试)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="maxCount">最大重试次数</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <remarks>只有当遇到抛出RetryMarkException异常使用才会重试</remarks>
        /// <returns></returns>
        public static IRetryTask<TResult> Retry<TResult>(this ITask<TResult> task, int maxCount)
        {
            return task.Retry(maxCount, null);
        }

        /// <summary>
        /// 返回提供请求重试的请求任务对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="maxCount">最大重试次数</param>
        /// <param name="delay">各次重试的延时时间</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <remarks>只有当遇到抛出RetryMarkException异常使用才会重试</remarks>
        /// <returns></returns>
        public static IRetryTask<TResult> Retry<TResult>(this ITask<TResult> task, int maxCount, TimeSpan delay)
        {
            return task.Retry(maxCount, i => delay);
        }

        /// <summary>
        /// 返回提供请求重试的请求任务对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="maxCount">最大重试次数</param>
        /// <param name="delay">各次重试的延时时间</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <remarks>只有当遇到抛出RetryMarkException异常使用才会重试</remarks>
        /// <returns></returns>
        public static IRetryTask<TResult> Retry<TResult>(this ITask<TResult> task, int maxCount,
                                                         Func<int, TimeSpan>? delay)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (maxCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            return new ActionRetryTask<TResult>(async () => await task, maxCount, delay);
        }

        /// <summary>当遇到异常时返回默认值</summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <returns></returns>
        public static ITask<TResult> HandleAsDefaultWhenException<TResult>(this Task<TResult> task)
        {
            return task.Handle().WhenCatch((Func<Exception, TResult>)(ex => default));
        }

        /// <summary>返回提供异常处理请求任务对象</summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <returns></returns>
        public static IHandleTask<TResult> Handle<TResult>(this Task<TResult> task)
        {
            return task != null
                ? (IHandleTask<TResult>)new ActionHandleTask<TResult>((Func<Task<TResult>>)(async () => await task))
                : throw new ArgumentNullException(nameof(task));
        }
    }
}