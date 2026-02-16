namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 默认脚本服务实现
/// </summary>
/// <remarks>
/// 此实现针对 PostgreSQL 数据库
/// 如需支持其他数据库，请实现 <see cref="IScriptService"/> 接口
/// </remarks>
public class DefaultScriptService : IScriptService
{
    /// <summary>
    /// 获取 PostgreSQL 初始化表的 SQL 脚本
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="field">配置键字段名</param>
    /// <param name="value">配置值字段名</param>
    /// <param name="schema">模式名（Schema），可选</param>
    /// <returns>创建表的 SQL 脚本</returns>
    public virtual string GetInitTableScript(string tableName, string field, string value, string? schema = null)
    {
        var createSchema = schema is not null ? $"CREATE SCHEMA IF NOT EXISTS {schema};" : string.Empty;
        var fullTableName = schema is not null ? $"{schema}.{tableName}" : tableName;
        return $@"{createSchema}
CREATE TABLE IF NOT EXISTS {fullTableName} (
    id SERIAL CONSTRAINT {tableName}_pk PRIMARY KEY,
    {field} VARCHAR(50) NOT NULL,
    {value} VARCHAR(2000) NOT NULL,
    is_delete BOOLEAN NOT NULL DEFAULT FALSE
);";
    }

    /// <summary>
    /// 获取初始化表数据的 SQL 脚本
    /// </summary>
    /// <returns>返回空字符串，表示不初始化数据</returns>
    public virtual string GetInitTableDataScript()
    {
        return string.Empty;
    }
}