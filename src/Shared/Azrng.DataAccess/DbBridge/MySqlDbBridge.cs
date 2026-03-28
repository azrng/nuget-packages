using Azrng.Core.Model;
using Azrng.DataAccess.Helper;

namespace Azrng.DataAccess.DbBridge
{
    public class MySqlBasicDbBridge : BasicDbBridge
    {
        public MySqlBasicDbBridge(string connectionString) : base(connectionString) { }

        public MySqlBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new()
            {
                {
                    SystemOperatorConst.DbName,
                    "SHOW DATABASES;"
                },
                {
                    SystemOperatorConst.SchemaName,
                    @"SELECT SCHEMA_NAME AS Schema_name
FROM information_schema.SCHEMATA
WHERE SCHEMA_NAME NOT IN ('information_schema', 'performance_schema', 'sys', 'mysql');"
                },
                {
                    SystemOperatorConst.SchemaInfo,
                    @"SELECT SCHEMA_NAME AS SchemaName,
       NULL AS SchemaComment
FROM information_schema.SCHEMATA
WHERE SCHEMA_NAME NOT IN ('information_schema', 'performance_schema', 'sys', 'mysql');"
                },
                {
                    SystemOperatorConst.SchemaTableName,
                    @"SELECT TABLE_SCHEMA AS SchemaName,
       TABLE_NAME AS TableName
FROM information_schema.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA NOT IN ('information_schema', 'performance_schema', 'sys', 'mysql');"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    @"SELECT 0 AS TableId,
       TABLE_NAME AS TableName,
       TABLE_COMMENT AS TableComment
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = @schema_name
  AND TABLE_TYPE = 'BASE TABLE';"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    @"SELECT 0 AS TableId,
       TABLE_NAME AS TableName,
       TABLE_COMMENT AS TableComment
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = @schema_name
  AND TABLE_NAME = @table_name
  AND TABLE_TYPE = 'BASE TABLE';"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT c.TABLE_NAME AS TableName,
       c.COLUMN_NAME AS ColumnName,
       IF(c.COLUMN_TYPE LIKE '%(%' AND c.NUMERIC_PRECISION IS NOT NULL AND c.NUMERIC_SCALE IS NOT NULL,
          REPLACE(REPLACE(REPLACE(c.COLUMN_TYPE, c.DATA_TYPE, ''), '(', ''), ')', ''),
          c.CHARACTER_MAXIMUM_LENGTH) AS ColumnLength,
       c.DATA_TYPE AS ColumnType,
       c.COLUMN_DEFAULT AS ColumnDefault,
       c.COLUMN_COMMENT AS ColumnComment,
       CASE WHEN c.EXTRA = 'auto_increment' THEN TRUE ELSE FALSE END AS IsIdentity,
       CASE WHEN c.IS_NULLABLE = 'YES' THEN TRUE ELSE FALSE END AS IsNull,
       CASE WHEN c.COLUMN_KEY = 'PRI' THEN TRUE ELSE FALSE END AS IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM information_schema.KEY_COLUMN_USAGE k
               WHERE k.TABLE_SCHEMA = c.TABLE_SCHEMA
                 AND k.TABLE_NAME = c.TABLE_NAME
                 AND k.COLUMN_NAME = c.COLUMN_NAME
                 AND k.REFERENCED_TABLE_NAME IS NOT NULL) THEN TRUE
           ELSE FALSE
           END AS IsForeignKey,
       c.ORDINAL_POSITION AS RowNumber
FROM information_schema.COLUMNS c
WHERE c.TABLE_SCHEMA = @schema_name
  AND c.TABLE_NAME = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT c.TABLE_NAME AS TableName,
       c.COLUMN_NAME AS ColumnName,
       IF(c.COLUMN_TYPE LIKE '%(%' AND c.NUMERIC_PRECISION IS NOT NULL AND c.NUMERIC_SCALE IS NOT NULL,
          REPLACE(REPLACE(REPLACE(c.COLUMN_TYPE, c.DATA_TYPE, ''), '(', ''), ')', ''),
          c.CHARACTER_MAXIMUM_LENGTH) AS ColumnLength,
       c.DATA_TYPE AS ColumnType,
       c.COLUMN_DEFAULT AS ColumnDefault,
       c.COLUMN_COMMENT AS ColumnComment,
       CASE WHEN c.EXTRA = 'auto_increment' THEN TRUE ELSE FALSE END AS IsIdentity,
       CASE WHEN c.IS_NULLABLE = 'YES' THEN TRUE ELSE FALSE END AS IsNull,
       CASE WHEN c.COLUMN_KEY = 'PRI' THEN TRUE ELSE FALSE END AS IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM information_schema.KEY_COLUMN_USAGE k
               WHERE k.TABLE_SCHEMA = c.TABLE_SCHEMA
                 AND k.TABLE_NAME = c.TABLE_NAME
                 AND k.COLUMN_NAME = c.COLUMN_NAME
                 AND k.REFERENCED_TABLE_NAME IS NOT NULL) THEN TRUE
           ELSE FALSE
           END AS IsForeignKey,
       c.ORDINAL_POSITION AS RowNumber
FROM information_schema.COLUMNS c
WHERE c.TABLE_SCHEMA = @schema_name;"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT TABLE_NAME AS TableName,
       COLUMN_NAME AS ColumnName,
       CONSTRAINT_NAME AS ColumnConstraintName
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_NAME = @table_name
  AND CONSTRAINT_SCHEMA = @schema_name
  AND CONSTRAINT_NAME = 'PRIMARY';"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT TABLE_NAME AS TableName,
       COLUMN_NAME AS ColumnName,
       CONSTRAINT_NAME AS ColumnConstraintName
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = @schema_name
  AND CONSTRAINT_NAME = 'PRIMARY';"
                },
                {
                    SystemOperatorConst.TableForeign,
                    @"SELECT TABLE_NAME AS TableName,
       COLUMN_NAME AS ColumnName,
       CONSTRAINT_NAME AS ColumnConstraintName,
       REFERENCED_TABLE_SCHEMA AS ForeignSchemaName,
       REFERENCED_TABLE_NAME AS ForeignTableName,
       REFERENCED_COLUMN_NAME AS ForeignColumnName
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = @schema_name
  AND TABLE_NAME = @table_name
  AND CONSTRAINT_NAME <> 'PRIMARY'
  AND REFERENCED_TABLE_SCHEMA <> '';"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    @"SELECT TABLE_NAME AS TableName,
       COLUMN_NAME AS ColumnName,
       CONSTRAINT_NAME AS ColumnConstraintName,
       REFERENCED_TABLE_SCHEMA AS ForeignSchemaName,
       REFERENCED_TABLE_NAME AS ForeignTableName,
       REFERENCED_COLUMN_NAME AS ForeignColumnName
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = @schema_name
  AND CONSTRAINT_NAME <> 'PRIMARY'
  AND REFERENCED_TABLE_SCHEMA <> '';"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"SELECT a.TABLE_NAME AS TableName,
       a.INDEX_NAME AS IndexName,
       '' AS Indexdef,
       CASE WHEN a.NON_UNIQUE = 0 THEN TRUE ELSE FALSE END AS Indisunique,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM information_schema.TABLE_CONSTRAINTS c
               WHERE c.CONSTRAINT_NAME = a.INDEX_NAME
                 AND c.TABLE_SCHEMA = a.TABLE_SCHEMA
                 AND c.TABLE_NAME = a.TABLE_NAME
                 AND c.CONSTRAINT_TYPE = 'PRIMARY KEY') THEN TRUE
           ELSE FALSE
           END AS Indisprimary,
       a.INDEX_COMMENT AS Description,
       a.COLUMN_NAME AS ColumnName,
       a.SEQ_IN_INDEX AS IndexPostion,
       CASE a.COLLATION WHEN 'D' THEN 'DESC' ELSE 'ASC' END AS IndexSort
FROM information_schema.STATISTICS a
WHERE a.TABLE_SCHEMA = @schema_name
  AND a.TABLE_NAME = @table_name
  AND a.INDEX_NAME <> 'PRIMARY';"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"SELECT a.TABLE_NAME AS TableName,
       a.INDEX_NAME AS IndexName,
       '' AS Indexdef,
       CASE WHEN a.NON_UNIQUE = 0 THEN TRUE ELSE FALSE END AS Indisunique,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM information_schema.TABLE_CONSTRAINTS c
               WHERE c.CONSTRAINT_NAME = a.INDEX_NAME
                 AND c.TABLE_SCHEMA = a.TABLE_SCHEMA
                 AND c.TABLE_NAME = a.TABLE_NAME
                 AND c.CONSTRAINT_TYPE = 'PRIMARY KEY') THEN TRUE
           ELSE FALSE
           END AS Indisprimary,
       a.INDEX_COMMENT AS Description,
       a.COLUMN_NAME AS ColumnName,
       a.SEQ_IN_INDEX AS IndexPostion,
       CASE a.COLLATION WHEN 'D' THEN 'DESC' ELSE 'ASC' END AS IndexSort
FROM information_schema.STATISTICS a
WHERE a.TABLE_SCHEMA = @schema_name
  AND a.INDEX_NAME <> 'PRIMARY';"
                },
                {
                    SystemOperatorConst.DbView,
                    @"SELECT COALESCE(t.TABLE_COMMENT, '') AS ViewDescription,
       v.TABLE_NAME AS ViewName,
       v.TABLE_SCHEMA AS ViewOwner,
       CONCAT(
           'CREATE VIEW `',
           REPLACE(v.TABLE_SCHEMA, '`', '``'),
           '`.`',
           REPLACE(v.TABLE_NAME, '`', '``'),
           '` AS',
           CHAR(10),
           COALESCE(v.VIEW_DEFINITION, '')) AS ViewDefinition
FROM information_schema.VIEWS v
LEFT JOIN information_schema.TABLES t
       ON t.TABLE_SCHEMA = v.TABLE_SCHEMA
      AND t.TABLE_NAME = v.TABLE_NAME
WHERE v.TABLE_SCHEMA NOT IN ('information_schema', 'performance_schema', 'sys', 'mysql')
ORDER BY v.TABLE_SCHEMA, v.TABLE_NAME;"
                },
                {
                    SystemOperatorConst.SchemaView,
                    @"SELECT COALESCE(t.TABLE_COMMENT, '') AS ViewDescription,
       v.TABLE_NAME AS ViewName,
       v.TABLE_SCHEMA AS ViewOwner,
       CONCAT(
           'CREATE VIEW `',
           REPLACE(v.TABLE_SCHEMA, '`', '``'),
           '`.`',
           REPLACE(v.TABLE_NAME, '`', '``'),
           '` AS',
           CHAR(10),
           COALESCE(v.VIEW_DEFINITION, '')) AS ViewDefinition
FROM information_schema.VIEWS v
LEFT JOIN information_schema.TABLES t
       ON t.TABLE_SCHEMA = v.TABLE_SCHEMA
      AND t.TABLE_NAME = v.TABLE_NAME
WHERE v.TABLE_SCHEMA = @schema_name
ORDER BY v.TABLE_NAME;"
                },
                { SystemOperatorConst.DbProc, string.Empty },
                {
                    SystemOperatorConst.SchemaProc,
                    @"SELECT ROUTINE_NAME AS ProcName,
       '' AS InputParam,
       CASE
           WHEN ROUTINE_TYPE = 'FUNCTION' THEN COALESCE(DTD_IDENTIFIER, '')
           ELSE ''
       END AS OutputParam,
       COALESCE(ROUTINE_DEFINITION, '') AS ProcDefinition,
       COALESCE(ROUTINE_COMMENT, '') AS ProcDescription
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = @schema_name
  AND ROUTINE_TYPE = 'PROCEDURE'
ORDER BY ROUTINE_NAME;"
                },
                {
                    SystemOperatorConst.SchemaRoutine,
                    @"SELECT ROUTINE_SCHEMA AS SchemaName,
       ROUTINE_NAME AS RoutineName,
       ROUTINE_TYPE AS RoutineType,
       '' AS InputParam,
       CASE
           WHEN ROUTINE_TYPE = 'FUNCTION' THEN COALESCE(DTD_IDENTIFIER, '')
           ELSE ''
       END AS OutputParam,
       COALESCE(ROUTINE_DEFINITION, '') AS RoutineDefinition,
       COALESCE(ROUTINE_COMMENT, '') AS RoutineDescription
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = @schema_name
ORDER BY ROUTINE_TYPE, ROUTINE_NAME;"
                },
                {
                    SystemOperatorConst.TableTimestamp,
                    @"SELECT create_time AS CreateTime,
       update_time AS ModifyTime
FROM information_schema.tables
WHERE table_schema = @schema_name
  AND table_name = @table_name
LIMIT 1;"
                },
                { "ColumnSize", "SELECT '{3}' CKEY,(SUM(OCTET_LENGTH({0}))) CSIZE FROM {1}.{2};" }
            };

        public override DatabaseType DatabaseType => DatabaseType.MySql;

        private IDbHelper? _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= HasConnectionString
                ? new MySqlDbHelper(ConnectionString)
                : new MySqlDbHelper(DataSourceConfig);
    }
}
