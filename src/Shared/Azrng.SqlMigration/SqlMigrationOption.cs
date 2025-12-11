using System.Data;

namespace Azrng.SqlMigration;

/// <summary>
/// 迁移配置
/// </summary>
public class SqlMigrationOption
{
    /// <summary>
    /// dbConnection构建委托
    /// </summary>
    public Func<IServiceProvider, IDbConnection> ConnectionBuilder { get; set; } = null!;

    /// <summary>
    /// 迁移锁
    /// </summary>
    public Func<IServiceProvider, Task<IAsyncDisposable?>>? LockProvider { get; set; }

    /// <summary>
    /// 初始化版本设置器实现类
    /// </summary>
    public Type? InitVersionSetterType { get; private set; }

    /// <summary>
    /// sql所在文件夹路径
    /// </summary>
    public string? SqlRootPath { get; set; }

    /// <summary>
    /// sql文件前缀
    /// </summary>
    public string VersionPrefix { get; set; } = "version";

    /// <summary>
    /// 数据库schema
    /// </summary>
    public string Schema { get; set; } = "public";

    /// <summary>
    /// 针对于不存在迁移表或迁移表数据为空的情况
    /// 来确认当前系统的版本
    /// 默认是0.0.0
    /// </summary>
    /// <typeparam name="T">初始化版本设置器实现类</typeparam>
    public void SetInitVersionSetter<T>() where T : IInitVersionSetter
    {
        InitVersionSetterType = typeof(T);
    }
}