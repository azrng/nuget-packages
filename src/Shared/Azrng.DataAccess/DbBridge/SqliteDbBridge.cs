using Azrng.Core.Model;
using Azrng.DataAccess.Helper;

namespace Azrng.DataAccess.DbBridge
{
    public class SqliteBasicDbBridge : BasicDbBridge
    {
        public SqliteBasicDbBridge(string connectionString) : base(connectionString) { }

        public SqliteBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new()
            {
                { SystemOperatorConst.SchemaName, "SELECT 'main' AS Schema_name;" },
                { SystemOperatorConst.SchemaInfo, "SELECT 'main' AS SchemaName, NULL AS SchemaComment;" },
                {
                    SystemOperatorConst.SchemaTableName,
                    @"SELECT 'main' AS SchemaName,
       name AS TableName
FROM sqlite_master
WHERE type = 'table'
  AND name NOT LIKE 'sqlite_%';"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    @"SELECT 0 AS TableId,
       name AS TableName,
       '' AS TableComment
FROM sqlite_master
WHERE type = 'table'
  AND name NOT LIKE 'sqlite_%';"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    @"SELECT 0 AS TableId,
       name AS TableName,
       '' AS TableComment
FROM sqlite_master
WHERE type = 'table'
  AND name = @table_name;"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT @table_name AS TableName,
       p.name AS ColumnName,
       p.type AS ColumnType,
       NULL AS ColumnLength,
       p.dflt_value AS ColumnDefault,
       NULL AS ColumnComment,
       0 AS IsIdentity,
       CASE WHEN p.""notnull"" = 1 THEN 0 ELSE 1 END AS IsNull,
       CASE WHEN p.pk > 0 THEN 1 ELSE 0 END AS IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM pragma_foreign_key_list(@table_name) fk
               WHERE fk.""from"" = p.name) THEN 1
           ELSE 0
           END AS IsForeignKey,
       p.cid + 1 AS RowNumber
FROM pragma_table_info(@table_name) p
ORDER BY p.cid;"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT m.name AS TableName,
       p.name AS ColumnName,
       p.type AS ColumnType,
       NULL AS ColumnLength,
       p.dflt_value AS ColumnDefault,
       NULL AS ColumnComment,
       0 AS IsIdentity,
       CASE WHEN p.""notnull"" = 1 THEN 0 ELSE 1 END AS IsNull,
       CASE WHEN p.pk > 0 THEN 1 ELSE 0 END AS IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM pragma_foreign_key_list(m.name) fk
               WHERE fk.""from"" = p.name) THEN 1
           ELSE 0
           END AS IsForeignKey,
       p.cid + 1 AS RowNumber
FROM sqlite_master m
JOIN pragma_table_info(m.name) p
WHERE m.type = 'table'
  AND m.name NOT LIKE 'sqlite_%'
ORDER BY m.name, p.cid;"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT @table_name AS TableName,
       p.name AS ColumnName,
       'PRIMARY' AS ColumnConstraintName
FROM pragma_table_info(@table_name) p
WHERE p.pk > 0
ORDER BY p.pk;"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT m.name AS TableName,
       p.name AS ColumnName,
       'PRIMARY' AS ColumnConstraintName
FROM sqlite_master m
JOIN pragma_table_info(m.name) p
WHERE m.type = 'table'
  AND m.name NOT LIKE 'sqlite_%'
  AND p.pk > 0
ORDER BY m.name, p.pk;"
                },
                {
                    SystemOperatorConst.TableForeign,
                    @"SELECT @table_name AS TableName,
       fk.""from"" AS ColumnName,
       'FOREIGN KEY' AS ColumnConstraintName,
       'main' AS ForeignSchemaName,
       fk.""table"" AS ForeignTableName,
       fk.""to"" AS ForeignColumnName
FROM pragma_foreign_key_list(@table_name) fk;"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    @"SELECT m.name AS TableName,
       fk.""from"" AS ColumnName,
       'FOREIGN KEY' AS ColumnConstraintName,
       'main' AS ForeignSchemaName,
       fk.""table"" AS ForeignTableName,
       fk.""to"" AS ForeignColumnName
FROM sqlite_master m
JOIN pragma_foreign_key_list(m.name) fk
WHERE m.type = 'table'
  AND m.name NOT LIKE 'sqlite_%';"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"SELECT @table_name AS TableName,
       il.name AS IndexName,
       '' AS Indexdef,
       CASE WHEN il.""unique"" = 1 THEN 1 ELSE 0 END AS Indisunique,
       CASE WHEN il.origin = 'pk' THEN 1 ELSE 0 END AS Indisprimary,
       '' AS Description,
       ii.name AS ColumnName,
       ii.seqno + 1 AS IndexPostion,
       'ASC' AS IndexSort
FROM pragma_index_list(@table_name) il
JOIN pragma_index_info(il.name) ii
WHERE il.origin IN ('c', 'u', 'pk');"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"SELECT m.name AS TableName,
       il.name AS IndexName,
       '' AS Indexdef,
       CASE WHEN il.""unique"" = 1 THEN 1 ELSE 0 END AS Indisunique,
       CASE WHEN il.origin = 'pk' THEN 1 ELSE 0 END AS Indisprimary,
       '' AS Description,
       ii.name AS ColumnName,
       ii.seqno + 1 AS IndexPostion,
       'ASC' AS IndexSort
FROM sqlite_master m
JOIN pragma_index_list(m.name) il
JOIN pragma_index_info(il.name) ii
WHERE m.type = 'table'
  AND m.name NOT LIKE 'sqlite_%'
  AND il.origin IN ('c', 'u', 'pk');"
                },
                {
                    SystemOperatorConst.DbView,
                    @"SELECT name AS ViewName,
       'main' AS ViewOwner,
       sql AS ViewDefinition,
       NULL AS ViewDescription
FROM sqlite_master
WHERE type = 'view';"
                },
                {
                    SystemOperatorConst.SchemaView,
                    @"SELECT name AS ViewName,
       'main' AS ViewOwner,
       sql AS ViewDefinition,
       NULL AS ViewDescription
FROM sqlite_master
WHERE type = 'view';"
                },
                { SystemOperatorConst.DbProc, string.Empty },
                { SystemOperatorConst.SchemaProc, string.Empty }
            };

        public override DatabaseType DatabaseType => DatabaseType.Sqlite;

        private IDbHelper? _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= HasConnectionString
                ? new SqliteDbHelper(ConnectionString)
                : new SqliteDbHelper(DataSourceConfig);
    }
}
