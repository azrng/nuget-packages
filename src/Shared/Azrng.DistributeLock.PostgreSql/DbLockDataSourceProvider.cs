using Azrng.DistributeLock.Core;
using Dapper;
using Npgsql;

namespace Azrng.DistributeLock.PostgreSql;

/// <summary>
///数据库锁数据源提供程序
/// </summary>
internal class DbLockDataSourceProvider : ILockDataSourceProvider
{
    private readonly string _connectionString;
    private readonly string _schema;
    private readonly string _table;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="schema">表分区</param>
    /// <param name="table">表</param>
    public DbLockDataSourceProvider(string connectionString, string schema, string table)
    {
        _connectionString = connectionString;
        _schema = schema;
        _table = table;
    }

    /// <summary>
    ///获取锁
    ///1.删除过期锁
    ///2.尝试插入新锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="lockValue">值</param>
    /// <param name="expireTime">锁过期时间</param>
    /// <param name="getLockTimeOut">获取锁超时时间</param>
    /// <returns></returns>
    public async Task<bool> TakeLockAsync(string lockKey, string lockValue, TimeSpan expireTime,
        TimeSpan getLockTimeOut)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var flag = false;
        using var tokenSource = new CancellationTokenSource(getLockTimeOut);
        var cancellationToken = tokenSource.Token;

        // 使用参数化查询防止SQL注入
        var insertSql = $"INSERT INTO {_schema}.{_table}(key, value, expire_time) VALUES (@lockKey, @lockValue, @expireTime) ON CONFLICT (key) DO NOTHING;";
        var deleteExpiredSql = $"DELETE FROM {_schema}.{_table} WHERE expire_time < @currentTime;";

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var num = await connection.ExecuteAsync(
                insertSql,
                new
                {
                    lockKey,
                    lockValue,
                    expireTime = SystemDateTime.Now().Add(expireTime)
                }).ConfigureAwait(false);

            if (num == 0)
            {
                await connection.ExecuteAsync(
                    deleteExpiredSql,
                    new { currentTime = SystemDateTime.Now() }).ConfigureAwait(false);
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                continue;
            }

            flag = true;
            break;
        }

        return flag;
    }


    /// <summary>
    ///释放锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="lockValue">值</param>
    /// <returns></returns>
    public async Task ReleaseLockAsync(string lockKey, string lockValue)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        // 使用参数化查询防止SQL注入
        var deleteSql = $"DELETE FROM {_schema}.{_table} WHERE key = @lockKey AND value = @lockValue;";
        await connection.ExecuteAsync(deleteSql, new { lockKey, lockValue }).ConfigureAwait(false);
    }

    /// <summary>
    /// 延长锁的过期时间
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="lockValue">值</param>
    /// <param name="extendTime">延长时间</param>
    /// <returns>延长成功返回true，否则返回false</returns>
    public async Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        // 使用参数化查询防止SQL注入
        var updateSql = $"UPDATE {_schema}.{_table} SET expire_time = @newExpireTime WHERE key = @lockKey AND value = @lockValue;";
        var affectedRows = await connection.ExecuteAsync(
            updateSql,
            new
            {
                lockKey,
                lockValue,
                newExpireTime = SystemDateTime.Now().Add(extendTime)
            }).ConfigureAwait(false);

        return affectedRows > 0;
    }

    /// <summary>
    /// 检测锁定的表是否存在，不存在则初始化表
    /// </summary>
    public void Init()
    {
        // 验证 schema 和 table 名称，防止 SQL 注入
        if (!IsValidIdentifier(_schema) || !IsValidIdentifier(_table))
        {
            throw new ArgumentException("Schema 或 table 名称包含非法字符");
        }

        var sql = $@"CREATE SCHEMA IF NOT EXISTS {_schema};
                        CREATE TABLE IF NOT EXISTS {_schema}.{_table}
                        (
	                        key TEXT NOT NULL CONSTRAINT {_table}_pk PRIMARY KEY,
	                        value TEXT NOT NULL,
	                        expire_time TIMESTAMP WITHOUT TIME ZONE NOT NULL
                        );";
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        connection.Execute(sql);
    }

    /// <summary>
    /// 验证标识符是否合法，防止 SQL 注入
    /// </summary>
    /// <param name="identifier">要验证的标识符</param>
    /// <returns>合法返回 true，否则返回 false</returns>
    private static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        // PostgreSQL 标识符规则：只能包含字母、数字、下划线，且不能以数字开头
        // 同时限制长度和防止关键字
        if (identifier.Length > 63)
            return false;

        return identifier.All(c => char.IsLetterOrDigit(c) || c == '_') && !char.IsDigit(identifier[0]);
    }
}