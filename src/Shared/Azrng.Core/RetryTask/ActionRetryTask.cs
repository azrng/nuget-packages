using Azrng.Core.Exceptions;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Azrng.Core.RetryTask
{
    internal sealed class ActionRetryTask<TResult> : TaskBase<TResult>,
        IRetryTask<TResult>
    {
        /// <summary>请求任务创建的委托</summary>
        private readonly Func<Task<TResult>> _invoker;

        /// <summary>获取最大重试次数</summary>
        private readonly int _maxRetryCount;

        /// <summary>获取各次重试的延时时间</summary>
        private readonly Func<int, TimeSpan> _retryDelay;

        /// <summary>支持重试的Api请求任务</summary>
        /// <param name="invoker">请求任务创建的委托</param>
        /// <param name="maxRetryCount">最大尝试次数</param>
        /// <param name="retryDelay">各次重试的延时时间</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        public ActionRetryTask(Func<Task<TResult>> invoker,
                               int maxRetryCount,
                               Func<int, TimeSpan> retryDelay)
        {
            if (maxRetryCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount));
            _invoker = invoker;
            _maxRetryCount = maxRetryCount;
            _retryDelay = retryDelay;
        }

        /// <summary>创建新的请求任务</summary>
        /// <returns></returns>
        protected override async Task<TResult> InvokeAsync()
        {
            Exception inner = null;
            for (var i = 0; i <= _maxRetryCount; ++i)
            {
                TResult result;
                try
                {
                    await DelayBeforeRetry(i).ConfigureAwait(false);
                    result = await _invoker().ConfigureAwait(false);
                }
                catch (RetryMarkException ex)
                {
                    inner = ex.InnerException;
                    continue;
                }

                return result;
            }

            throw new InternalServerException($"超出最大重试{_maxRetryCount} 错误信息：{inner?.InnerException?.Message}");
        }

        /// <summary>
        /// 执行前延时
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task DelayBeforeRetry(int index)
        {
            if (index == 0 || _retryDelay == null)
                return;
            var delay = _retryDelay(index - 1);
            if (!(delay > TimeSpan.Zero))
                return;
            await Task.Delay(delay).ConfigureAwait(false);
        }

        /// <summary>
        /// 当捕获到异常时进行Retry
        /// </summary>
        /// <typeparam name="TException">异常类型</typeparam>
        /// <returns></returns>
        public IRetryTask<TResult> WhenCatch<TException>() where TException : Exception
        {
            return WhenCatch((Func<TException, bool>)(ex => true));
        }

        /// <summary>
        /// 当捕获到异常时进行Retry
        /// </summary>
        /// <typeparam name="TException">异常类型</typeparam>
        /// <param name="handler">捕获到指定异常时</param>
        /// <returns></returns>
        public IRetryTask<TResult> WhenCatch<TException>(Action<TException> handler) where TException : Exception
        {
            return WhenCatch((Func<TException, bool>)(ex =>
            {
                var action = handler;
                action?.Invoke(ex);
                return true;
            }));
        }

        /// <summary>
        /// 当捕获到异常时进行Retry
        /// </summary>
        /// <typeparam name="TException">异常类型</typeparam>
        /// <param name="predicate">返回true才Retry</param>
        /// <returns></returns>
        public IRetryTask<TResult> WhenCatch<TException>(Func<TException, bool> predicate) where TException : Exception
        {
            return WhenCatchAsync((Func<TException, Task<bool>>)(ex =>
                Task.FromResult(predicate == null || predicate(ex))));
        }

        /// <summary>
        /// 当捕获到异常时进行Retry
        /// </summary>
        /// <typeparam name="TException">异常类型</typeparam>
        /// <param name="handler">捕获到指定异常时</param>
        /// <returns></returns>
        public IRetryTask<TResult> WhenCatchAsync<TException>(Func<TException, Task> handler) where TException : Exception
        {
            return WhenCatchAsync((Func<TException, Task<bool>>)(async ex =>
            {
                if (handler != null)
                    await handler(ex).ConfigureAwait(false);
                return true;
            }));
        }

        /// <summary>
        /// 当捕获到异常时进行Retry
        /// </summary>
        /// <typeparam name="TException">异常类型</typeparam>
        /// <param name="predicate">返回true才Retry</param>
        /// <returns></returns>
        public IRetryTask<TResult> WhenCatchAsync<TException>(Func<TException, Task<bool>> predicate) where TException : Exception
        {
            return new ActionRetryTask<TResult>(NewInvoker, _maxRetryCount,
                _retryDelay);

            async Task<TResult> NewInvoker()
            {
                try
                {
                    return await _invoker().ConfigureAwait(false);
                }
                catch (TException ex)
                {
                    var flag = predicate == null;
                    if (!flag)
                        flag = await predicate(ex).ConfigureAwait(false);
                    if (flag)
                        throw new RetryMarkException(ex);

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return default;
                }
            }
        }

        /// <summary>当结果符合条件时进行Retry</summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        public IRetryTask<TResult> WhenResult(Func<TResult, bool> predicate)
        {
            return predicate != null
                ? WhenResultAsync(r => Task.FromResult(predicate(r)))
                : throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>当结果符合条件时进行Retry</summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        public IRetryTask<TResult> WhenResultAsync(Func<TResult, Task<bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            return new ActionRetryTask<TResult>(NewInvoker, _maxRetryCount,
                _retryDelay);

            async Task<TResult> NewInvoker()
            {
                var result = await _invoker().ConfigureAwait(false);
                return !await predicate(result).ConfigureAwait(false)
                    ? result
                    : throw new ArgumentException("待定");
            }
        }

        /// <summary>表示重试标记的异常</summary>
        /// <summary>重试标记的异常</summary>
        /// <param name="inner">内部异常</param>
        private class RetryMarkException(Exception inner) : Exception(null, inner) { }
    }
}