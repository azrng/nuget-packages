using Azrng.Core.Model;
using Azrng.DataAccess.Helper;

namespace Azrng.DataAccess.DbBridge
{
    public class PostgreBasicDbBridge : BasicDbBridge
    {
        public PostgreBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public PostgreBasicDbBridge(string connectionString) : base(connectionString) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new()
            {
                {
                    SystemOperatorConst.DbName,
                    @"SELECT datname
FROM pg_database
WHERE datistemplate = false
ORDER BY datname;"
                },
                {
                    SystemOperatorConst.SchemaName,
                    @"SELECT schema_name
FROM information_schema.schemata
WHERE schema_name NOT LIKE 'pg_%'
  AND schema_name <> 'information_schema';"
                },
                {
                    SystemOperatorConst.SchemaInfo,
                    @"SELECT n.nspname AS SchemaName,
       d.description AS SchemaComment
FROM pg_namespace n
LEFT JOIN pg_description d ON n.oid = d.objoid AND d.objsubid = 0
WHERE n.nspname NOT LIKE 'pg_%'
  AND n.nspname <> 'information_schema';"
                },
                {
                    SystemOperatorConst.SchemaTableName,
                    @"SELECT n.nspname AS SchemaName,
       a.relname AS TableName
FROM pg_class a
INNER JOIN pg_namespace n ON a.relnamespace = n.oid
WHERE a.relkind = 'r'
ORDER BY a.relname;"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    @"SELECT a.oid AS TableId,
       a.relname AS TableName,
       b.description AS TableComment
FROM pg_class a
LEFT JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
WHERE a.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
  AND a.relkind = 'r'
ORDER BY a.relname;"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    @"SELECT a.oid AS TableId,
       a.relname AS TableName,
       b.description AS TableComment
FROM pg_class a
LEFT JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
WHERE a.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
  AND a.relkind = 'r'
  AND a.relname = @table_name
ORDER BY a.relname;"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT DISTINCT @table_name AS TableName,
       col.ColumnName,
       col.ColumnLength,
       col.Sort AS RowNumber,
       col.ColumnType,
       col.ColumnDefault,
       col.IsIdentity,
       colInfo.ColumnComment,
       colInfo.IsNull,
       colInfo.IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM pg_constraint
               WHERE conrelid = colInfo.attrelid
                 AND conkey[1] = colInfo.attnum
                 AND contype = 'f') THEN TRUE
           ELSE FALSE
           END AS IsForeignKey
FROM (
    SELECT column_name AS ColumnName,
           CASE
               WHEN numeric_precision IS NOT NULL THEN
                   CASE WHEN numeric_scale <= 0 THEN CAST(numeric_precision AS text)
                        ELSE concat_ws(',', numeric_precision, numeric_scale) END
               WHEN datetime_precision IS NOT NULL THEN CAST(datetime_precision AS text)
               WHEN interval_precision IS NOT NULL THEN CAST(datetime_precision AS text)
               ELSE CAST(character_maximum_length AS text)
               END AS ColumnLength,
           ordinal_position AS Sort,
           data_type AS ColumnType,
           column_default AS ColumnDefault,
           CASE WHEN column_default LIKE '%nextval%' THEN TRUE ELSE FALSE END AS IsIdentity
    FROM information_schema.columns
    WHERE table_name = @table_name
      AND table_schema = @schema_name
) col
INNER JOIN (
    SELECT DISTINCT col_description(a.attrelid, a.attnum) AS ColumnComment,
           a.attname AS name,
           NOT a.attnotnull AS IsNull,
           CASE
               WHEN (
                   SELECT COUNT(*)
                   FROM pg_constraint
                   WHERE conrelid = a.attrelid
                     AND a.attnum = ANY (conkey)
                     AND contype = 'p') > 0 THEN TRUE
               ELSE FALSE
               END AS IsPrimaryKey,
           a.attrelid,
           a.attnum
    FROM pg_class c
    JOIN pg_attribute a ON a.attrelid = c.oid
    WHERE c.relname = @table_name
      AND c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
      AND a.attnum > 0
) colInfo ON colInfo.name = col.ColumnName
ORDER BY RowNumber;"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT DISTINCT col.TableName,
       col.ColumnName,
       col.ColumnLength,
       col.Sort AS RowNumber,
       col.ColumnType,
       col.ColumnDefault,
       col.IsIdentity,
       colInfo.ColumnComment,
       colInfo.IsNull,
       colInfo.IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM pg_constraint pc
               WHERE pc.conrelid = colInfo.attrelid
                 AND colInfo.attnum = ANY (pc.conkey)
                 AND pc.contype = 'f') THEN TRUE
           ELSE FALSE
           END AS IsForeignKey
FROM (
    SELECT table_name AS TableName,
           column_name AS ColumnName,
           CASE
               WHEN numeric_precision IS NOT NULL THEN
                   CASE WHEN numeric_scale <= 0 THEN CAST(numeric_precision AS text)
                        ELSE concat_ws(',', numeric_precision, numeric_scale) END
               WHEN datetime_precision IS NOT NULL THEN CAST(datetime_precision AS text)
               WHEN interval_precision IS NOT NULL THEN CAST(datetime_precision AS text)
               ELSE CAST(character_maximum_length AS text)
               END AS ColumnLength,
           ordinal_position AS Sort,
           data_type AS ColumnType,
           column_default AS ColumnDefault,
           CASE WHEN column_default LIKE '%nextval%' THEN TRUE ELSE FALSE END AS IsIdentity
    FROM information_schema.columns
    WHERE table_schema = @schema_name
) col
INNER JOIN (
    SELECT DISTINCT c.relname AS TableName,
           col_description(a.attrelid, a.attnum) AS ColumnComment,
           a.attname AS name,
           NOT a.attnotnull AS IsNull,
           CASE
               WHEN (
                   SELECT COUNT(*)
                   FROM pg_constraint
                   WHERE conrelid = a.attrelid
                     AND a.attnum = ANY (conkey)
                     AND contype = 'p') > 0 THEN TRUE
               ELSE FALSE
               END AS IsPrimaryKey,
           a.attrelid,
           a.attnum
    FROM pg_class c
    JOIN pg_attribute a ON a.attrelid = c.oid
    WHERE c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
      AND a.attnum > 0
) colInfo ON colInfo.name = col.ColumnName AND colInfo.TableName = col.TableName
ORDER BY col.TableName, RowNumber;"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT tc.table_name AS TableName,
       kcu.column_name AS ColumnName,
       tc.constraint_name AS ColumnConstraintName
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
  ON tc.constraint_name = kcu.constraint_name
 AND tc.table_schema = kcu.table_schema
WHERE tc.constraint_type = 'PRIMARY KEY'
  AND tc.table_schema = @schema_name
  AND tc.table_name = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT tc.table_name AS TableName,
       kcu.column_name AS ColumnName,
       tc.constraint_name AS ColumnConstraintName
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
  ON tc.constraint_name = kcu.constraint_name
 AND tc.table_schema = kcu.table_schema
WHERE tc.constraint_type = 'PRIMARY KEY'
  AND tc.table_schema = @schema_name;"
                },
                {
                    SystemOperatorConst.TableForeign,
                    @"SELECT tc.table_name AS TableName,
       kcu.column_name AS ColumnName,
       tc.constraint_name AS ColumnConstraintName,
       ccu.table_schema AS ForeignSchemaName,
       ccu.table_name AS ForeignTableName,
       ccu.column_name AS ForeignColumnName
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
  ON tc.constraint_name = kcu.constraint_name
 AND tc.table_schema = kcu.table_schema
 AND tc.constraint_schema = kcu.constraint_schema
JOIN information_schema.constraint_column_usage ccu
  ON ccu.constraint_name = tc.constraint_name
 AND ccu.table_schema = tc.table_schema
 AND ccu.constraint_schema = tc.constraint_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
  AND tc.table_schema = @schema_name
  AND tc.table_name = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    @"SELECT tc.table_name AS TableName,
       kcu.column_name AS ColumnName,
       tc.constraint_name AS ColumnConstraintName,
       ccu.table_schema AS ForeignSchemaName,
       ccu.table_name AS ForeignTableName,
       ccu.column_name AS ForeignColumnName
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
  ON tc.constraint_name = kcu.constraint_name
 AND tc.table_schema = kcu.table_schema
 AND tc.constraint_schema = kcu.constraint_schema
JOIN information_schema.constraint_column_usage ccu
  ON ccu.constraint_name = tc.constraint_name
 AND ccu.table_schema = tc.table_schema
 AND ccu.constraint_schema = tc.constraint_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
  AND tc.table_schema = @schema_name;"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"SELECT e.relname AS TableName,
       a.indexname AS IndexName,
       a.indexdef AS Indexdef,
       c.indisunique AS Indisunique,
       c.indisprimary AS Indisprimary,
       d.description AS Description,
       g.attname AS ColumnName,
       g.attnum AS IndexPostion,
       CASE WHEN c.indoption::text = '0' THEN 'ASC' ELSE 'DESC' END AS IndexSort
FROM pg_am b
LEFT JOIN pg_class f ON b.oid = f.relam
LEFT JOIN pg_stat_all_indexes e ON f.oid = e.indexrelid
LEFT JOIN pg_index c ON e.indexrelid = c.indexrelid
LEFT JOIN pg_description d ON c.indexrelid = d.objoid
LEFT JOIN pg_attribute g ON f.oid = g.attrelid,
     pg_indexes a
WHERE a.schemaname = e.schemaname
  AND a.tablename = e.relname
  AND a.indexname = e.indexrelname
  AND e.schemaname = @schema_name
  AND e.relname = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"SELECT e.relname AS TableName,
       a.indexname AS IndexName,
       a.indexdef AS Indexdef,
       c.indisunique AS Indisunique,
       c.indisprimary AS Indisprimary,
       d.description AS Description,
       g.attname AS ColumnName,
       g.attnum AS IndexPostion,
       CASE WHEN c.indoption::text = '0' THEN 'ASC' ELSE 'DESC' END AS IndexSort
FROM pg_am b
LEFT JOIN pg_class f ON b.oid = f.relam
LEFT JOIN pg_stat_all_indexes e ON f.oid = e.indexrelid
LEFT JOIN pg_index c ON e.indexrelid = c.indexrelid
LEFT JOIN pg_description d ON c.indexrelid = d.objoid
LEFT JOIN pg_attribute g ON f.oid = g.attrelid,
     pg_indexes a
WHERE a.schemaname = e.schemaname
  AND a.tablename = e.relname
  AND a.indexname = e.indexrelname
  AND e.schemaname = @schema_name;"
                },
                {
                    SystemOperatorConst.DbView,
                    @"SELECT b.description AS ViewDescription,
       c.viewname AS ViewName,
       c.viewowner AS ViewOwner,
       c.definition AS ViewDefinition
FROM pg_class a
LEFT JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
INNER JOIN pg_views c ON a.relname = c.viewname
INNER JOIN pg_namespace d ON a.relnamespace = d.oid
WHERE c.schemaname NOT IN ('pg_catalog', 'information_schema');"
                },
                {
                    SystemOperatorConst.SchemaView,
                    @"SELECT b.description AS ViewDescription,
       c.viewname AS ViewName,
       c.viewowner AS ViewOwner,
       c.definition AS ViewDefinition
FROM pg_class a
LEFT JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
INNER JOIN pg_views c ON a.relname = c.viewname AND c.schemaname = @schema_name
INNER JOIN pg_namespace d ON a.relnamespace = d.oid AND d.nspname = @schema_name;"
                },
                {
                    SystemOperatorConst.SchemaProc,
                    @"SELECT proname AS ProcName,
       COALESCE(pg_get_function_arguments(a.oid), '') AS InputParam,
       COALESCE(pg_get_function_result(a.oid), '') AS OutputParam,
       pg_get_functiondef(a.oid) AS ProcDefinition,
       b.description AS ProcDescription
FROM pg_proc a
LEFT JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
INNER JOIN pg_namespace d ON a.pronamespace = d.oid AND d.nspname = @schema_name
WHERE a.proname NOT IN ('median');"
                },
                {
                    SystemOperatorConst.DbProc,
                    @"SELECT n.nspname AS SchemaName,
       a.proname AS ProcName,
       COALESCE(pg_get_function_arguments(a.oid), '') AS InputParam,
       COALESCE(pg_get_function_result(a.oid), '') AS OutputParam,
       b.description AS ProcDescription,
       pg_get_functiondef(a.oid) AS ProcDefinition
FROM pg_proc a
LEFT JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
INNER JOIN pg_namespace n ON n.oid = a.pronamespace
WHERE n.nspname NOT IN ('pg_catalog', 'information_schema');"
                }
            };

        public override DatabaseType DatabaseType => DatabaseType.PostgresSql;

        private IDbHelper? _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= HasConnectionString
                ? new PostgresSqlDbHelper(ConnectionString)
                : new PostgresSqlDbHelper(DataSourceConfig);
    }
}
