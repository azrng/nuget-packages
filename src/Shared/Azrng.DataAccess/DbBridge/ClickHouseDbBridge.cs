using Azrng.Core.Model;
using Azrng.DataAccess.Helper;

namespace Azrng.DataAccess.DbBridge
{
    public class ClickHouseBasicDbBridge : BasicDbBridge
    {
        public ClickHouseBasicDbBridge(string connectionString) : base(connectionString) { }

        public ClickHouseBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new()
            {
                { SystemOperatorConst.SchemaName, "SELECT name AS Schema_name FROM system.databases;" },
                { SystemOperatorConst.SchemaInfo, "SELECT name AS SchemaName, NULL AS SchemaComment FROM system.databases;" },
                {
                    SystemOperatorConst.SchemaTableName,
                    @"SELECT database AS SchemaName,
       name AS TableName
FROM system.tables;"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    @"SELECT 0 AS TableId,
       name AS TableName,
       comment AS TableComment
FROM system.tables
WHERE database = @schema_name;"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    @"SELECT 0 AS TableId,
       name AS TableName,
       comment AS TableComment
FROM system.tables
WHERE database = @schema_name
  AND name = @table_name;"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT t.name AS TableName,
       c.name AS ColumnName,
       c.type AS ColumnType,
       CASE
           WHEN c.type LIKE 'FixedString(%' THEN CAST(substring(c.type, 13, length(c.type) - 13) AS Int32)
           WHEN c.type LIKE 'String(%' THEN CAST(substring(c.type, 8, length(c.type) - 8) AS Int32)
           WHEN c.type LIKE 'Decimal(%' THEN CAST(substring(c.type, 8, position(c.type, ',') - 8) AS Int32)
           WHEN c.type LIKE 'Enum%' THEN CAST(substring(c.type, 6, position(c.type, '(') - 6) AS Int32)
           ELSE NULL
           END AS ColumnLength,
       c.default_expression AS ColumnDefault,
       c.comment AS ColumnComment,
       FALSE AS IsIdentity,
       CASE WHEN c.type LIKE 'Nullable(%' THEN TRUE ELSE FALSE END AS IsNull,
       c.is_in_primary_key AS IsPrimaryKey,
       FALSE AS IsForeignKey,
       row_number() OVER (PARTITION BY t.name ORDER BY c.position) AS RowNumber
FROM system.tables t
JOIN system.columns c ON t.name = c.table AND t.database = c.database
WHERE t.database = @schema_name
  AND c.table = @table_name
ORDER BY t.name, c.position;"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT t.name AS TableName,
       c.name AS ColumnName,
       c.type AS ColumnType,
       CASE
           WHEN c.type LIKE 'FixedString(%' THEN CAST(substring(c.type, 13, length(c.type) - 13) AS Int32)
           WHEN c.type LIKE 'String(%' THEN CAST(substring(c.type, 8, length(c.type) - 8) AS Int32)
           WHEN c.type LIKE 'Decimal(%' THEN CAST(substring(c.type, 8, position(c.type, ',') - 8) AS Int32)
           WHEN c.type LIKE 'Enum%' THEN CAST(substring(c.type, 6, position(c.type, '(') - 6) AS Int32)
           ELSE NULL
           END AS ColumnLength,
       c.default_expression AS ColumnDefault,
       c.comment AS ColumnComment,
       FALSE AS IsIdentity,
       CASE WHEN c.type LIKE 'Nullable(%' THEN TRUE ELSE FALSE END AS IsNull,
       c.is_in_primary_key AS IsPrimaryKey,
       FALSE AS IsForeignKey,
       row_number() OVER (PARTITION BY t.name ORDER BY c.position) AS RowNumber
FROM system.tables t
JOIN system.columns c ON t.name = c.table AND t.database = c.database
WHERE t.database = @schema_name
ORDER BY t.name, c.position;"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT c.table AS TableName,
       c.name AS ColumnName,
       'PRIMARY' AS ColumnConstraintName
FROM system.columns c
WHERE c.database = @schema_name
  AND c.table = @table_name
  AND c.is_in_primary_key = TRUE;"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT c.table AS TableName,
       c.name AS ColumnName,
       'PRIMARY' AS ColumnConstraintName
FROM system.columns c
WHERE c.database = @schema_name
  AND c.is_in_primary_key = TRUE;"
                },
                {
                    SystemOperatorConst.TableForeign,
                    @"SELECT @table_name AS TableName,
       CAST(NULL AS Nullable(String)) AS ColumnName,
       CAST(NULL AS Nullable(String)) AS ColumnConstraintName,
       CAST(NULL AS Nullable(String)) AS ForeignSchemaName,
       CAST(NULL AS Nullable(String)) AS ForeignTableName,
       CAST(NULL AS Nullable(String)) AS ForeignColumnName
FROM system.one
WHERE 1 = 0;"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    @"SELECT CAST(NULL AS Nullable(String)) AS TableName,
       CAST(NULL AS Nullable(String)) AS ColumnName,
       CAST(NULL AS Nullable(String)) AS ColumnConstraintName,
       CAST(NULL AS Nullable(String)) AS ForeignSchemaName,
       CAST(NULL AS Nullable(String)) AS ForeignTableName,
       CAST(NULL AS Nullable(String)) AS ForeignColumnName
FROM system.one
WHERE 1 = 0;"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"SELECT @table_name AS TableName,
       CAST(NULL AS Nullable(String)) AS IndexName,
       CAST(NULL AS Nullable(String)) AS Indexdef,
       FALSE AS Indisunique,
       FALSE AS Indisprimary,
       CAST(NULL AS Nullable(String)) AS Description,
       CAST(NULL AS Nullable(String)) AS ColumnName,
       CAST(NULL AS Nullable(Int32)) AS IndexPostion,
       CAST(NULL AS Nullable(String)) AS IndexSort
FROM system.one
WHERE 1 = 0;"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"SELECT CAST(NULL AS Nullable(String)) AS TableName,
       CAST(NULL AS Nullable(String)) AS IndexName,
       CAST(NULL AS Nullable(String)) AS Indexdef,
       FALSE AS Indisunique,
       FALSE AS Indisprimary,
       CAST(NULL AS Nullable(String)) AS Description,
       CAST(NULL AS Nullable(String)) AS ColumnName,
       CAST(NULL AS Nullable(Int32)) AS IndexPostion,
       CAST(NULL AS Nullable(String)) AS IndexSort
FROM system.one
WHERE 1 = 0;"
                },
                { SystemOperatorConst.DbView, string.Empty },
                { SystemOperatorConst.SchemaView, string.Empty },
                { SystemOperatorConst.DbProc, string.Empty },
                { SystemOperatorConst.SchemaProc, string.Empty }
            };

        public override DatabaseType DatabaseType => DatabaseType.ClickHouse;

        private IDbHelper? _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= HasConnectionString
                ? new ClickHouseDbHelper(ConnectionString)
                : new ClickHouseDbHelper(DataSourceConfig);
    }
}
