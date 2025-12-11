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
        connection.Open();

        var flag = false;
        using var tokenSource = new CancellationTokenSource(getLockTimeOut);
        var cancellationToken = tokenSource.Token;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var num = await connection.ExecuteAsync(
                $"insert into {_schema}.{_table}(key,value,expire_time) values ('{lockKey}','{lockValue}','{SystemDateTime.Now().Add(expireTime):yyyy-MM-dd HH:mm:ss}') ON CONFLICT (key) DO NOTHING;");
            if (num == 0)
            {
                await connection.ExecuteAsync(
                    @$"delete from {_schema}.{_table} where expire_time<'{SystemDateTime.Now():yyyy-MM-dd HH:mm:ss}';");
                await Task.Delay(10, cancellationToken);
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
        connection.Open();
        await connection.ExecuteAsync($"delete from {_schema}.{_table} where key='{lockKey}' and value='{lockValue}';");
    }

    public Task ExtendLockAsync(string lockKey, string lockValue, TimeSpan extendTime)
    {
        // todo 还没实现
        throw new NotImplementedException();
    }

    /// <summary>
    ///检测锁定的表是否存在，不存在则初始化表
    /// </summary>
    public void Init()
    {
        var sql = $@"CREATE SCHEMA IF NOT EXISTS {_schema};
                        create table if not exists {_schema}.{_table}
                        (
	                        key text not null constraint {_table}_pk primary key,
	                        value text not null,
	                        expire_time timestamp without time zone not null
                        );";
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        connection.Execute(sql);
    }
}