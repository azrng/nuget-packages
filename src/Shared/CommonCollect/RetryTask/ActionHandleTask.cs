namespace CommonCollect.RetryTask
{
    /// <summary>提供异常处理的请求任务</summary>
    /// <typeparam name="TResult"></typeparam>
    internal sealed class ActionHandleTask<TResult> :
        TaskBase<TResult>,
        IHandleTask<TResult>,
        ITask<TResult>
    {
        /// <summary>请求任务创建的委托</summary>
        private readonly Func<Task<TResult>> invoker;

        /// <summary>异常处理的请求任务</summary>
        /// <param name="invoker">请求任务创建的委托</param>
        public ActionHandleTask(Func<Task<TResult>> invoker) => this.invoker = invoker;

        /// <summary>创建请求任务</summary>
        /// <returns></returns>
        protected override Task<TResult> InvokeAsync() => invoker();

        /// <summary>当捕获到异常时返回指定结果</summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func">获取结果</param>
        /// <returns></returns>
        public IHandleTask<TResult> WhenCatch<TException>(Func<TResult> func) where TException : Exception
        {
            return func != null ? WhenCatch((Func<TException, TResult>)(ex => func())) : throw new ArgumentNullException(nameof(func));
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
            return new ActionHandleTask<TResult>(newInvoker);

            async Task<TResult> newInvoker()
            {
                int num;
                TException exception;
                try
                {
                    return await invoker().ConfigureAwait(false);
                }
                catch (TException ex)
                {
                    num = 1;
                    exception = ex;
                }

                if (num != 1)
                {
                    return default;
                }

              return await func(exception).ConfigureAwait(false);
            }
        }
    }
}