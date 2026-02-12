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
        }

        protected BasicDbBridge(DataSourceConfig dataSourceConfig)
        {
            DataSourceConfig = dataSourceConfig;
        }

        public abstract Dictionary<string, string> QuerySqlMap { get; }

        public string ConnectionString { get; private set; }

        public abstract DatabaseType DatabaseType { get; }

        public abstract IDbHelper DbHelper { get; }

        protected DataSourceConfig DataSourceConfig { get; private set; }

        public virtual async Task<List<string>> GetSchemaNameListAsync()
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaName];
            return await DbHelper.QueryAsync<string>(sql);
        }

        public async Task<List<GetSchemaListDto>> GetSchemaListAsync()
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaInfo];
            return await DbHelper.QueryAsync<GetSchemaListDto>(sql);
        }

        public async Task<List<SchemaTableDto>> GetTableNameListAsync()
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaTableName];
            var result = await DbHelper.QueryAsync<SchemaTableDto>(sql);
            return result.OrderBy(x => x.TableName).ToList();
        }

        public async Task<List<GetTableInfoBySchemaDto>> GetTableInfoListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaTableInfoList];
            var result = await DbHelper.QueryAsync<GetTableInfoBySchemaDto>(sql, new { schema_name = schemaName });
            return result.OrderBy(x => x.TableName).ToList();
        }

        public async Task<GetTableInfoBySchemaDto?> GetTableInfoAsync(string schemaName, string tableName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaTableInfo];
            return await DbHelper.QueryFirstOrDefaultAsync<GetTableInfoBySchemaDto>(sql,
                new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<ColumnInfoDto>> GetColumnListAsync(string schemaName, string tableName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.TableColumn];
            return await DbHelper.QueryAsync<ColumnInfoDto>(sql,
                new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<ColumnInfoDto>> GetColumnListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaColumn];
            return await DbHelper.QueryAsync<ColumnInfoDto>(sql,
                new { schema_name = schemaName });
        }

        public async Task<List<PrimaryModel>> GetPrimaryListAsync(string schemaName, string tableName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.TablePrimary];
            return await DbHelper.QueryAsync<PrimaryModel>(sql,
                new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<PrimaryModel>> GetPrimaryListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaPrimary];
            return await DbHelper.QueryAsync<PrimaryModel>(sql,
                new { schema_name = schemaName });
        }

        public async Task<List<ForeignModel>> GetForeignListAsync(string schemaName, string tableName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.TableForeign];
            return await DbHelper.QueryAsync<ForeignModel>(sql,
                new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<ForeignModel>> GetForeignListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaForeign];
            return await DbHelper.QueryAsync<ForeignModel>(sql,
                new { schema_name = schemaName });
        }

        public async Task<List<IndexModel>> GetIndexListAsync(string schemaName, string tableName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.TableIndex];
            return await DbHelper.QueryAsync<IndexModel>(sql,
                new { schema_name = schemaName, table_name = tableName });
        }

        public async Task<List<IndexModel>> GetIndexListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaIndex];
            return await DbHelper.QueryAsync<IndexModel>(sql,
                new { schema_name = schemaName });
        }

        public async Task<List<ViewModel>> GetViewListAsync()
        {
            var sql = QuerySqlMap[SystemOperatorConst.DbView];
            if (sql.IsNullOrWhiteSpace())
                throw new NotSupportedException("该数据库类型暂不支持该方法");

            return await DbHelper.QueryAsync<ViewModel>(sql);
        }

        public async Task<List<ViewModel>> GetSchemaViewListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaView];
            return await DbHelper.QueryAsync<ViewModel>(sql,
                new { schema_name = schemaName });
        }

        public async Task<List<DbProcModel>> GetProcListAsync()
        {
            var sql = QuerySqlMap[SystemOperatorConst.DbProc];
            if (sql.IsNullOrWhiteSpace())
                throw new NotSupportedException("该数据库类型暂不支持该方法");

            return await DbHelper.QueryAsync<DbProcModel>(sql);
        }

        public async Task<List<ProcModel>> GetSchemaProcListAsync(string schemaName)
        {
            var sql = QuerySqlMap[SystemOperatorConst.SchemaProc];
            return await DbHelper.QueryAsync<ProcModel>(sql,
                new { schema_name = schemaName });
        }
    }
}