namespace Azrng.DistributeLock.PostgreSql;

/// <summary>
///  数据库分布式锁配置选项
/// </summary>
public class DbLockOptions
{
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 锁定表所在的表空间
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// 锁定的表名
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// 默认的锁超时时间
    /// </summary>
    public TimeSpan DefaultExpireTime { get; set; } = TimeSpan.FromSeconds(5);
}