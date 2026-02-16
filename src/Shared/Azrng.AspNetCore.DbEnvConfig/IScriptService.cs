namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库脚本服务接口
/// </summary>
/// <remarks>
/// 用于自定义数据库初始化脚本，支持不同的数据库类型（PostgreSQL、SQL Server、MySQL 等）
/// 实现此接口可以自定义表结构和初始数据
/// </remarks>
public interface IScriptService
{
    /// <summary>
    /// 获取初始化表的 SQL 脚本
    /// </summary>
    /// <param name="tableName">表名（不包含 schema）</param>
    /// <param name="field">配置键字段名</param>
    /// <param name="value">配置值字段名</param>
    /// <param name="schema">模式名（Schema），可选</param>
    /// <returns>创建表的 SQL 脚本</returns>
    /// <remarks>
    /// 此方法应返回创建表的 SQL 语句
    /// 如果返回空字符串或 null，则不执行初始化
    /// </remarks>
    string GetInitTableScript(string tableName, string field, string value, string? schema = null);

    /// <summary>
    /// 获取初始化表数据的 SQL 脚本
    /// </summary>
    /// <returns>初始化数据的 SQL 脚本</returns>
    /// <remarks>
    /// 此方法仅在表没有数据时才会执行
    /// 如果返回空字符串或 null，则不执行初始化
    /// </remarks>
    string GetInitTableDataScript();
}