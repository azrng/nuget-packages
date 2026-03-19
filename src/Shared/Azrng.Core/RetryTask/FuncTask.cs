using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Azrng.Core.RetryTask
{
    /// <summary>
    /// 将 Func&lt;Task&lt;TResult&gt;&gt; 包装为 ITask&lt;TResult&gt;
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    internal class FuncTask<TResult> : ITask<TResult>
    {
        private readonly Func<Task<TResult>> _func;

        public FuncTask(Func<Task<TResult>> func)
        {
            _func = func;
        }

        public TaskAwaiter<TResult> GetAwaiter()
        {
            return _func().GetAwaiter();
        }

        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            return _func().ConfigureAwait(continueOnCapturedContext);
        }
    }
}
