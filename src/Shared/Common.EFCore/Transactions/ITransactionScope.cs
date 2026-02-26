using System;
using System.Threading.Tasks;

namespace Azrng.EFCore.Transactions
{
    /// <summary>
    /// 事务作用域接口
    /// </summary>
    public interface ITransactionScope : IAsyncDisposable
    {
        /// <summary>
        /// 提交事务
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// 回滚事务
        /// </summary>
        Task RollbackAsync();
    }
}
