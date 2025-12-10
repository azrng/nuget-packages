using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 异步方法同步执行
    /// </summary>
    public class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// 运行有参异步方法
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 运行无参异步方法
        /// </summary>
        /// <param name="func"></param>
        public static void RunSync(Func<Task> func)
        {
            _taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}