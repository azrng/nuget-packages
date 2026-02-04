using System;
using System.Threading.Tasks;

namespace Azrng.Core.RetryTask
{
    /// <summary>提供异常处理的请求任务</summary>
    /// <typeparam name="TResult"></typeparam>
    internal sealed class ActionHandleTask<TResult> : TaskBase<TResult>, IHandleTask<TResult>
    {
        /// <summary>请求任务创建的委托</summary>
        private readonly Func<Task<TResult>> _invoker;

        /// <summary>异常处理的请求任务</summary>
        /// <param name="invoker">请求任务创建的委托</param>
        public ActionHandleTask(Func<Task<TResult>> invoker) => _invoker = invoker;

        /// <summary>创建请求任务</summary>
        /// <returns></returns>
        protected override Task<TResult> InvokeAsync() => _invoker();

        /// <summary>当捕获到异常时返回指定结果</summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func">获取结果</param>
        /// <returns></returns>
        public IHandleTask<TResult> WhenCatch<TException>(Func<TResult> func) where TException : Exception
        {
            return func != null ? WhenCatch((Func<TException, TResult>)(_ => func())) : throw new ArgumentNullException(nameof(func));
        }

        /// <summary>当捕获到异常时返回指定结果</summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func">获取结果</param>
        /// <returns></returns>
        public IHandleTask<TResult> WhenCatch<TException>(Func<TException, TResult> func) where TException : Exception
        {
            return func != null
                ? WhenCatchAsync((Func<TException, Task<TResult>>)(ex => Task.FromResult(func(ex))))
                : throw new ArgumentNullException(nameof(func));
        }

        /// <summary>当捕获到异常时返回指定结果</summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func">获取结果</param>
        /// <returns></returns>
        public IHandleTask<TResult> WhenCatchAsync<TException>(Func<TException, Task<TResult>> func) where TException : Exception
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            return new ActionHandleTask<TResult>(NewInvoker);

            async Task<TResult> NewInvoker()
            {
                try
                {
                    return await _invoker().ConfigureAwait(false);
                }
                catch (TException ex)
                {
                    return await func(ex).ConfigureAwait(false);
                }
            }
        }
    }
}