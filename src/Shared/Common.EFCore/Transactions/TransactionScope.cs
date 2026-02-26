using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Azrng.EFCore.Transactions
{
    /// <summary>
    /// 事务作用域实现
    /// </summary>
    internal sealed class TransactionScope : ITransactionScope
    {
        private readonly IDbContextTransaction _transaction;
        private readonly ILogger _logger;
        private bool _disposed;
        private bool _committed;
        private bool _rolledBack;

        public TransactionScope(IDbContextTransaction transaction, ILogger logger)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _logger = logger;
        }

        public async Task CommitAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TransactionScope));

            if (_rolledBack)
                throw new InvalidOperationException("Transaction has already been rolled back");

            if (_committed)
            {
                _logger.LogWarning("Transaction already committed");
                return;
            }

            await _transaction.CommitAsync();
            _committed = true;
        }

        public async Task RollbackAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TransactionScope));

            // 如果事务已提交，静默返回，不抛出异常
            // 这样用户可以在 catch 块中安全地调用 RollbackAsync 而无需检查状态
            if (_committed)
            {
                _logger.LogDebug("Transaction already committed, rollback ignored");
                return;
            }

            if (_rolledBack)
            {
                _logger.LogWarning("Transaction already rolled back");
                return;
            }

            await _transaction.RollbackAsync();
            _rolledBack = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            // 未提交且未回滚的事务自动回滚
            if (!_committed && !_rolledBack)
            {
                await _transaction.RollbackAsync();
            }

            await _transaction.DisposeAsync();
            _disposed = true;
        }
    }
}
