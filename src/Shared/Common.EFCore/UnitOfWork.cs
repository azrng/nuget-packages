using Azrng.EFCore.Entities;
using Azrng.EFCore.Extensions;
using Azrng.EFCore.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        private readonly object _repositoryObj = new object();

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
            var key = _context.GetType().FullName + typeof(TEntity).FullName;
            if (EfCoreGlobalConfig.Repositories.TryGetValue(key, out var repository))
            {
                return (IBaseRepository<TEntity>)repository;
            }

            lock (_repositoryObj)
            {
                if (EfCoreGlobalConfig.Repositories.TryGetValue(key, out var repository1))
                {
                    return (IBaseRepository<TEntity>)repository1;
                }

                // 获取指定上下文对应的BaseRepository
                var curdRepository = new BaseRepository<TEntity>(DbContext);
                EfCoreGlobalConfig.Repositories.Add(key, curdRepository);
                return curdRepository;
            }
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
                _logger.LogError(ex, "CommitTransaction failed, message:{Message}", ex.Message);
                transaction.Rollback();
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
                _logger.LogError(ex, "CommitTransaction failed, message:{Message}", ex.Message);
                await transaction.RollbackAsync().ConfigureAwait(false);
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

        #region 私有方法

        /// <summary>
        /// DataTable类型转为List类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static List<T> ToList<T>(DataTable dt) where T : class, new()
        {
            var propertyInfos = typeof(T).GetProperties();
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var t = new T();
                foreach (var p in propertyInfos)
                {
                    if (dt.Columns.IndexOf(p.Name) != -1 && row[p.Name] != DBNull.Value)
                        p.SetValue(t, row[p.Name], null);
                }

                list.Add(t);
            }

            return list;
        }

        /// <summary>
        /// 查询并返回DataTable
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        private static async Task<DataTable> SqlQueryDataTable(DatabaseFacade facade, string sql,
                                                               params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var reader = await command.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            await reader.CloseAsync();
            await conn.CloseAsync();
            return dt;
        }

        /// <summary>
        /// 创建命令
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="sql"></param>
        /// <param name="connection"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection connection,
                                               params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            connection = conn;
            conn.Open();
            var cmd = conn.CreateCommand();

            //提供了根据不同类型数据库的判断
            //facade.IsNpgsql()

            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        #endregion 私有方法

        public void Dispose()
        {
            if (EfCoreGlobalConfig.Repositories != null && EfCoreGlobalConfig.Repositories.Count > 0)
            {
                EfCoreGlobalConfig.Repositories.Clear();
            }
        }
    }
}