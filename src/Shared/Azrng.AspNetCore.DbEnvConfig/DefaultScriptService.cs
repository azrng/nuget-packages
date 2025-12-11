namespace Azrng.AspNetCore.DbEnvConfig
{
    /// <summary>
    /// 默认脚本服务(该内容脚本针对pgsql开发)
    /// </summary>
    public class DefaultScriptService : IScriptService
    {
        public virtual string GetInitTableScript(string tableName, string field, string value, string? schema = null)
        {
            var createSchema = schema is not null ? $"create schema if not exists {schema};" : string.Empty;
            var fullTableName = schema is not null ? $"{schema}.{tableName}" : tableName;
            return $@"{createSchema};create table if not exists {fullTableName}
            (
                id        serial
                    constraint {tableName}_pk
                        primary key,
                {field}      varchar(50)   not null,
                {value}     varchar(2000) not null,
                is_delete bool      not null
            );";
        }

        public virtual string GetInitTableDataScript()
        {
            return string.Empty;
        }
    }
}