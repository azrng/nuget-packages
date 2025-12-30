using System.Runtime.CompilerServices;

namespace CommonCollect.RetryTask
{
    /// <summary>表示Task抽象类</summary>
    /// <typeparam name="TResult"></typeparam>
    internal abstract class TaskBase<TResult> : ITask<TResult>
    {
        /// <summary>返回新创建的请求任务的等待器</summary>
        /// <returns></returns>
        public TaskAwaiter<TResult> GetAwaiter() => this.InvokeAsync().GetAwaiter();

        /// <summary>返回新创建的请求任务的等待器</summary>
        /// <param name="continueOnCapturedContext">试图继续回夺取的原始上下文，则为 true；否则为 false</param>
        /// <returns></returns>
        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            return this.InvokeAsync().ConfigureAwait(continueOnCapturedContext);
        }

        /// <summary>创建新的请求任务</summary>
        /// <returns></returns>
        protected abstract Task<TResult> InvokeAsync();
    }
}