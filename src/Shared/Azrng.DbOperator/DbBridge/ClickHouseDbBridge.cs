using Azrng.DbOperator.Helper;

namespace Azrng.DbOperator.DbBridge
{
    /// <summary>
    /// ch 系统操作
    /// </summary>
    public class ClickHouseBasicDbBridge : BasicBasicDbBridge
    {
        public ClickHouseBasicDbBridge(string connectionString) : base(connectionString) { }

        public ClickHouseBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new Dictionary<string, string>
            {
                { "Schema", "SELECT name as Schema_name FROM `system`.databases;" },
                {
                    "SchemaTable", "select name as TableName, `comment` as TableComment from `system`.tables where database = @schema_name;"
                },
                {
                    "TableColumn",
                    @"SELECT t.name                                                      AS TableName,
       c.name                                                      AS ColumnName,
       c.type                                                      AS ColumnType,
       CASE
           WHEN c.type LIKE 'Nullable(%' THEN TRUE
           ELSE FALSE
           END                                                     AS IsNull,
       -- 列长度（仅适用于某些类型，如 FixedString 和 Decimal）
       CASE
           WHEN c.type LIKE 'FixedString(%' THEN
               CAST(substring(c.type, 13, length(c.type) - 13) AS Int32)
           WHEN c.type LIKE 'String(%' THEN
               CAST(substring(c.type, 8, length(c.type) - 8) AS Int32)
           WHEN c.type LIKE 'Decimal(%' THEN
               CAST(substring(c.type, 8, position(c.type, ',') - 8) AS Int32)
           WHEN c.type LIKE 'Enum%' THEN
               CAST(substring(c.type, 6, position(c.type, '(') - 6) AS Int32)
           ELSE NULL
           END AS ColumnLength,
       c.default_expression                                        AS ColumnDefault,
       c.comment                                                   AS ColumnComment,
       c.is_in_primary_key                                        as IsPrimary,
       -- ClickHouse 不支持自增列（IDENTITY），因此这里返回 false
       false                                                       AS IsIdentity,
       -- ClickHouse 不支持外键，因此这里返回 false
       false                                                       AS IsForeignKey,
       -- 排序值（RowNumber）按列的顺序编号
       row_number() OVER (PARTITION BY t.name ORDER BY c.position) AS RowNumber
FROM system.tables t
         JOIN
     system.columns c ON t.name = c.table AND t.database = c.database
where database = @schema_name and `table` = @table_name
ORDER BY t.name, c.position;"
                },
                {
                    "SchemaColumn",
                    @"SELECT t.name                                                      AS TableName,
       c.name                                                      AS ColumnName,
       c.type                                                      AS ColumnType,
       CASE
           WHEN c.type LIKE 'Nullable(%' THEN TRUE
           ELSE FALSE
           END                                                     AS IsNull,
       -- 列长度（仅适用于某些类型，如 FixedString 和 Decimal）
       CASE
           WHEN c.type LIKE 'FixedString(%' THEN
               CAST(substring(c.type, 13, length(c.type) - 13) AS Int32)
           WHEN c.type LIKE 'String(%' THEN
               CAST(substring(c.type, 8, length(c.type) - 8) AS Int32)
           WHEN c.type LIKE 'Decimal(%' THEN
               CAST(substring(c.type, 8, position(c.type, ',') - 8) AS Int32)
           WHEN c.type LIKE 'Enum%' THEN
               CAST(substring(c.type, 6, position(c.type, '(') - 6) AS Int32)
           ELSE NULL
           END AS ColumnLength,
       c.default_expression                                        AS ColumnDefault,
       c.comment                                                   AS ColumnComment,
       c.is_in_primary_key                                        as IsPrimary,
       -- ClickHouse 不支持自增列（IDENTITY），因此这里返回 false
       false                                                       AS IsIdentity,
       -- ClickHouse 不支持外键，因此这里返回 false
       false                                                       AS IsForeignKey,
       -- 排序值（RowNumber）按列的顺序编号
       row_number() OVER (PARTITION BY t.name ORDER BY c.position) AS RowNumber
FROM system.tables t
         JOIN
     system.columns c ON t.name = c.table AND t.database = c.database
where database = @schema_name
ORDER BY t.table,t.name, c.position;"
                },
                {
                    "TablePrimary",
                    "select name as ColName, 'PRIMARY' AS ColConstraintName from `system`.columns c where database = @schema_name and `table` = @table_name and is_in_primary_key = TRUE;"
                },
                {
                    "SchemaPrimary",
                    "select `table` as TableName, name as ColName, 'PRIMARY' AS ColConstraintName from `system`.columns c where database = @schema_name and is_in_primary_key = TRUE;"
                },
                {
                    "TableForeign",
                    "select name as ColName, NULL AS ColConstraintName, NULL AS ForeignSchemaName, NULL AS ForeignTableName, NULL AS ForeignColumnName from `system`.columns c where database = @schema_name and `table` = @table_name LIMIT 0;"
                },
                {
                    "SchemaForeign",
                    "select name as ColName, NULL AS ColConstraintName, NULL AS ForeignSchemaName, NULL AS ForeignTableName, NULL AS ForeignColumnName from `system`.columns c where database = @schema_name LIMIT 0;"
                },
                {
                    "TableIndex",
                    "select NULL as IndexName, NULL as Indexdef, NULL as Indisunique, NULL as description, name as ColName, NULL as IndexPostion, NULL as IndexSort, NULL as Indisprimary from `system`.columns c where database = @schema_name and `table` = @table_name LIMIT 0;"
                },
                {
                    "SchemaIndex",
                    "select name as TableName, NULL as IndexName, NULL as Indexdef, NULL as Indisunique, NULL as description, name as ColName, NULL as IndexPostion, NULL as IndexSort, NULL as Indisprimary from `system`.columns c where database = @schema_name LIMIT 0;"
                },
            };

        public override DatabaseType DatabaseType => DatabaseType.ClickHouse;

        private IDbHelper _dbHelper;

        public override IDbHelper DbHelper => _dbHelper ??= new ClickHouseDbHelper(DataSourceConfig);

        public override Task<List<string>> GetSchemaNameListAsync()
        {
            var sql = QuerySqlMap.GetValueOrDefault(SystemOperatorConst.SchemaName);
            return sql is null ? null : Task.FromResult(new List<string>());
        }
    }
}