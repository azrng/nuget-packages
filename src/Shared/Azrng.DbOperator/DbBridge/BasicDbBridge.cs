using Azrng.Core.Model;
using Azrng.DbOperator.Dto;

namespace Azrng.DbOperator.DbBridge
{
    /// <summary>
    /// 基础数据库操作
    /// </summary>
    public abstract class BasicDbBridge : IBasicDbBridge
    {
        protected BasicDbBridge(string connectionString)
        {
            ConnectionString = connectionString;
            DataSourceConfig = new DataSourceConfig();
        }

        protected BasicDbBridge(DataSourceConfig dataSourceConfig)
        {
            DataSourceConfig = dataSourceConfig;
        }

        public abstract Dictionary<string, string> QuerySqlMap { get; }

        public string ConnectionString { get; private set; } = string.Empty;

        public abstract DatabaseType DatabaseType { get; }

        public abstract IDbHelper DbHelper { get; }

        protected DataSourceConfig DataSourceConfig { get; private set; }

        protected bool HasConnectionString => !string.IsNullOrWhiteSpace(ConnectionString);

        public virtual async Task<List<string>> GetSchemaNameListAsync()
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaName);
            return await DbHelper.QueryAsync<string>(sql);
        }

        public async Task<List<GetSchemaListDto>> GetSchemaListAsync()
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaInfo);
            return await DbHelper.QueryAsync<GetSchemaListDto>(sql);
        }

        public async Task<List<SchemaTableDto>> GetTableNameListAsync()
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaTableName);
            var result = await DbHelper.QueryAsync<SchemaTableDto>(sql);
            return result.OrderBy(x => x.TableName).ToList();
        }

        public async Task<List<GetTableInfoBySchemaDto>> GetTableInfoListAsync(string schemaName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaTableInfoList);
            var result = await DbHelper.QueryAsync<GetTableInfoBySchemaDto>(sql, new { schema_name = schemaName });
            return result.OrderBy(x => x.TableName).ToList();
        }

        public async Task<GetTableInfoBySchemaDto?> GetTableInfoAsync(string schemaName, string tableName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaTableInfo);
            return await DbHelper.QueryFirstOrDefaultAsync<GetTableInfoBySchemaDto>(
                sql,
                new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<ColumnInfoDto>> GetColumnListAsync(string schemaName, string tableName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.TableColumn);
            return await DbHelper.QueryAsync<ColumnInfoDto>(sql, new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<ColumnInfoDto>> GetColumnListAsync(string schemaName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaColumn);
            return await DbHelper.QueryAsync<ColumnInfoDto>(sql, new { schema_name = schemaName });
        }

        public async Task<List<PrimaryModel>> GetPrimaryListAsync(string schemaName, string tableName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.TablePrimary);
            return await DbHelper.QueryAsync<PrimaryModel>(sql, new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<PrimaryModel>> GetPrimaryListAsync(string schemaName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaPrimary);
            return await DbHelper.QueryAsync<PrimaryModel>(sql, new { schema_name = schemaName });
        }

        public async Task<List<ForeignModel>> GetForeignListAsync(string schemaName, string tableName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.TableForeign);
            return await DbHelper.QueryAsync<ForeignModel>(sql, new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<ForeignModel>> GetForeignListAsync(string schemaName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaForeign);
            return await DbHelper.QueryAsync<ForeignModel>(sql, new { schema_name = schemaName });
        }

        public async Task<List<IndexModel>> GetIndexListAsync(string schemaName, string tableName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.TableIndex);
            return await DbHelper.QueryAsync<IndexModel>(sql, new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<IndexModel>> GetIndexListAsync(string schemaName)
        {
            var sql = GetRequiredSql(SystemOperatorConst.SchemaIndex);
            return await DbHelper.QueryAsync<IndexModel>(sql, new { schema_name = schemaName });
        }

        public async Task<List<ViewModel>> GetViewListAsync()
        {
            var sql = GetOptionalSql(SystemOperatorConst.DbView);
            if (string.IsNullOrWhiteSpace(sql))
                throw new NotSupportedException("该数据库类型暂不支持该方法");

            return await DbHelper.QueryAsync<ViewModel>(sql);
        }

        public async Task<List<ViewModel>> GetSchemaViewListAsync(string schemaName)
        {
            var sql = GetOptionalSql(SystemOperatorConst.SchemaView);
            if (string.IsNullOrWhiteSpace(sql))
                throw new NotSupportedException("该数据库类型暂不支持该方法");

            return await DbHelper.QueryAsync<ViewModel>(sql, new { schema_name = schemaName });
        }

        public async Task<List<DbProcModel>> GetProcListAsync()
        {
            var sql = GetOptionalSql(SystemOperatorConst.DbProc);
            if (string.IsNullOrWhiteSpace(sql))
                throw new NotSupportedException("该数据库类型暂不支持该方法");

            return await DbHelper.QueryAsync<DbProcModel>(sql);
        }

        public async Task<List<ProcModel>> GetSchemaProcListAsync(string schemaName)
        {
            var sql = GetOptionalSql(SystemOperatorConst.SchemaProc);
            if (string.IsNullOrWhiteSpace(sql))
                throw new NotSupportedException("该数据库类型暂不支持该方法");

            return await DbHelper.QueryAsync<ProcModel>(sql, new { schema_name = schemaName });
        }

        protected string GetRequiredSql(string key)
        {
            if (QuerySqlMap.TryGetValue(key, out var sql) && !string.IsNullOrWhiteSpace(sql))
            {
                return sql;
            }

            throw new NotSupportedException($"当前数据库桥接未实现 SQL Key: {key}");
        }

        protected string? GetOptionalSql(string key)
        {
            return QuerySqlMap.GetValueOrDefault(key);
        }
    }
}
