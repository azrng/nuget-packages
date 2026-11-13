using Azrng.EFCore.Entities;
using Azrng.EFCore.Extensions;
using Azrng.EFCore.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if NET7_0_OR_GREATER
#endif

namespace Azrng.EFCore
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public class UnitOfWork<TContext> : IUnitOfWork, IUnitOfWork<TContext>, IDisposable where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly ILogger<UnitOfWork<TContext>> _logger;

        /// <summary>
        /// 仓储缓存。仓储持有 scoped 的 DbContext，生命周期必须与本工作单元一致，
        /// 因此只缓存在实例内，不能放静态字典（否则跨 Scope 复用已释放的上下文）
        /// </summary>
        private readonly ConcurrentDictionary<string, object> _repositories = new ConcurrentDictionary<string, object>();

        public UnitOfWork(TContext context, ILogger<UnitOfWork<TContext>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// 返回上下文
        /// </summary>
        /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
        public TContext DbContext => _context;

        public DatabaseFacade GetDatabase()
        {
            return _context.Database;
        }

        public IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntity
        {
            var key = _context.GetType().FullName + "." + typeof(TEntity).FullName;
            return (IBaseRepository<TEntity>)_repositories.GetOrAdd(key,
                _ => new BaseRepository<TEntity>(DbContext));
        }

        public int SaveChanges() => _context.SaveChanges();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);

        public void CommitTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using var transaction = _context.Database.BeginTransaction(isolationLevel);
            try
            {
                action.Invoke();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CommitTransaction failed, message:{Message}", ex.Message);
                TryRollback(transaction);
                throw;
            }
        }

        public async Task CommitTransactionAsync(Func<Task> func,
                                                 IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var transaction = await _context.Database.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
            try
            {
                await func().ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CommitTransaction failed, message:{Message}", ex.Message);
                await TryRollbackAsync(transaction).ConfigureAwait(false);
                throw;
            }
            finally
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }
        }

        public ITransactionScope BeginTransactionScope(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var transaction = _context.Database.BeginTransaction(isolationLevel);
            return new TransactionScope(transaction, _logger);
        }

        public async Task<ITransactionScope> BeginTransactionScopeAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var transaction = await _context.Database.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
            return new TransactionScope(transaction, _logger);
        }

#if NET7_0_OR_GREATER
        public IQueryable<T> SqlQuery<T>(FormattableString sql)
        {
            return _context.Database.SqlQuery<T>(sql);
        }
#endif

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlCommand(sql, parameters);
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        public object ExecuteScalar(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteScalar(sql, parameters);
        }

        public async Task<object> ExecuteScalarAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteScalarAsync(sql, parameters);
        }

        public DataTable SqlQueryDataTable(string sql, params object[] parameters)
        {
            return _context.Database.SqlQueryDataTable(sql, parameters);
        }

        public Task<DataTable> SqlQueryDataTableAsync(string sql, params object[] parameters)
        {
            return _context.Database.SqlQueryDataTableAsync(sql, parameters);
        }

        public List<T> SqlQueryList<T>(string sql, params object[] parameters) where T : class, new()
        {
            return _context.Database.SqlQueryList<T>(sql, parameters);
        }

        public Task<List<T>> SqlQueryListAsync<T>(string sql, params object[] parameters) where T : class, new()
        {
            return _context.Database.SqlQueryListAsync<T>(sql, parameters);
        }

        /// <summary>
        /// 回滚失败（如连接已中断）时只记录日志，不掩盖触发回滚的原始异常
        /// </summary>
        private void TryRollback(IDbContextTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackEx)
            {
                _logger?.LogWarning(rollbackEx, "Rollback failed, message:{Message}", rollbackEx.Message);
            }
        }

        /// <summary>
        /// 回滚失败（如连接已中断）时只记录日志，不掩盖触发回滚的原始异常
        /// </summary>
        private async Task TryRollbackAsync(IDbContextTransaction transaction)
        {
            try
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
            }
            catch (Exception rollbackEx)
            {
                _logger?.LogWarning(rollbackEx, "RollbackAsync failed, message:{Message}", rollbackEx.Message);
            }
        }

        public void Dispose()
        {
            // DbContext 与仓储的生命周期均由 DI Scope 管理，这里无需清理全局状态
        }
    }
}
