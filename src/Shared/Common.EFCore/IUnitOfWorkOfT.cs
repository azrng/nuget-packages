using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore
{
    /// <summary>
    /// 工作单元
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        /// <summary>
        /// 获取上下文
        /// </summary>
        TContext DbContext { get; }
    }
}