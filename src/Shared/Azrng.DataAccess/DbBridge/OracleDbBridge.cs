using Azrng.Core.Model;
using Azrng.DataAccess.Helper;

namespace Azrng.DataAccess.DbBridge
{
    public class OracleBasicDbBridge : BasicDbBridge
    {
        public OracleBasicDbBridge(string connectionString) : base(connectionString) { }

        public OracleBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new()
            {
                {
                    SystemOperatorConst.DbName,
                    @"SELECT USERNAME
FROM ALL_USERS
ORDER BY USERNAME"
                },
                {
                    SystemOperatorConst.SchemaName,
                    "SELECT SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AS Schema_name FROM dual"
                },
                {
                    SystemOperatorConst.SchemaInfo,
                    "SELECT SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AS SchemaName, NULL AS SchemaComment FROM dual"
                },
                {
                    SystemOperatorConst.SchemaTableName,
                    @"SELECT OWNER AS SchemaName,
       TABLE_NAME AS TableName
FROM ALL_TABLES
WHERE STATUS = 'VALID'"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    @"SELECT ao.OBJECT_ID AS TableId,
       at.TABLE_NAME AS TableName,
       tc.COMMENTS AS TableComment
FROM ALL_TABLES at
LEFT JOIN ALL_TAB_COMMENTS tc
       ON at.OWNER = tc.OWNER
      AND at.TABLE_NAME = tc.TABLE_NAME
LEFT JOIN ALL_OBJECTS ao
       ON ao.OWNER = at.OWNER
      AND ao.OBJECT_NAME = at.TABLE_NAME
      AND ao.OBJECT_TYPE = 'TABLE'
WHERE at.OWNER = UPPER(:schema_name)
  AND at.STATUS = 'VALID'"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    @"SELECT ao.OBJECT_ID AS TableId,
       at.TABLE_NAME AS TableName,
       tc.COMMENTS AS TableComment
FROM ALL_TABLES at
LEFT JOIN ALL_TAB_COMMENTS tc
       ON at.OWNER = tc.OWNER
      AND at.TABLE_NAME = tc.TABLE_NAME
LEFT JOIN ALL_OBJECTS ao
       ON ao.OWNER = at.OWNER
      AND ao.OBJECT_NAME = at.TABLE_NAME
      AND ao.OBJECT_TYPE = 'TABLE'
WHERE at.OWNER = UPPER(:schema_name)
  AND at.TABLE_NAME = UPPER(:table_name)
  AND at.STATUS = 'VALID'"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT a.TABLE_NAME AS TableName,
       a.COLUMN_NAME AS ColumnName,
       a.DATA_TYPE AS ColumnType,
       TO_CHAR(a.CHAR_COL_DECL_LENGTH) AS ColumnLength,
       a.DATA_DEFAULT AS ColumnDefault,
       b.COMMENTS AS ColumnComment,
       CASE WHEN a.IDENTITY_COLUMN = 'YES' THEN 1 ELSE 0 END AS IsIdentity,
       CASE WHEN a.NULLABLE = 'Y' THEN 1 ELSE 0 END AS IsNull,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM ALL_CONSTRAINTS c
               JOIN ALL_CONS_COLUMNS cc
                 ON c.OWNER = cc.OWNER
                AND c.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
               WHERE c.CONSTRAINT_TYPE = 'P'
                 AND c.OWNER = a.OWNER
                 AND cc.TABLE_NAME = a.TABLE_NAME
                 AND cc.COLUMN_NAME = a.COLUMN_NAME) THEN 1
           ELSE 0
           END AS IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM ALL_CONSTRAINTS c
               JOIN ALL_CONS_COLUMNS cc
                 ON c.OWNER = cc.OWNER
                AND c.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
               WHERE c.CONSTRAINT_TYPE = 'R'
                 AND c.OWNER = a.OWNER
                 AND cc.TABLE_NAME = a.TABLE_NAME
                 AND cc.COLUMN_NAME = a.COLUMN_NAME) THEN 1
           ELSE 0
           END AS IsForeignKey,
       ROW_NUMBER() OVER (PARTITION BY a.TABLE_NAME ORDER BY a.COLUMN_ID) AS RowNumber
FROM ALL_TAB_COLUMNS a
JOIN ALL_COL_COMMENTS b
  ON a.OWNER = b.OWNER
 AND a.TABLE_NAME = b.TABLE_NAME
 AND a.COLUMN_NAME = b.COLUMN_NAME
WHERE a.OWNER = UPPER(:schema_name)
  AND a.TABLE_NAME = UPPER(:table_name)"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT a.TABLE_NAME AS TableName,
       a.COLUMN_NAME AS ColumnName,
       a.DATA_TYPE AS ColumnType,
       TO_CHAR(a.CHAR_COL_DECL_LENGTH) AS ColumnLength,
       a.DATA_DEFAULT AS ColumnDefault,
       b.COMMENTS AS ColumnComment,
       CASE WHEN a.IDENTITY_COLUMN = 'YES' THEN 1 ELSE 0 END AS IsIdentity,
       CASE WHEN a.NULLABLE = 'Y' THEN 1 ELSE 0 END AS IsNull,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM ALL_CONSTRAINTS c
               JOIN ALL_CONS_COLUMNS cc
                 ON c.OWNER = cc.OWNER
                AND c.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
               WHERE c.CONSTRAINT_TYPE = 'P'
                 AND c.OWNER = a.OWNER
                 AND cc.TABLE_NAME = a.TABLE_NAME
                 AND cc.COLUMN_NAME = a.COLUMN_NAME) THEN 1
           ELSE 0
           END AS IsPrimaryKey,
       CASE
           WHEN EXISTS (
               SELECT 1
               FROM ALL_CONSTRAINTS c
               JOIN ALL_CONS_COLUMNS cc
                 ON c.OWNER = cc.OWNER
                AND c.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
               WHERE c.CONSTRAINT_TYPE = 'R'
                 AND c.OWNER = a.OWNER
                 AND cc.TABLE_NAME = a.TABLE_NAME
                 AND cc.COLUMN_NAME = a.COLUMN_NAME) THEN 1
           ELSE 0
           END AS IsForeignKey,
       ROW_NUMBER() OVER (PARTITION BY a.TABLE_NAME ORDER BY a.COLUMN_ID) AS RowNumber
FROM ALL_TAB_COLUMNS a
JOIN ALL_COL_COMMENTS b
  ON a.OWNER = b.OWNER
 AND a.TABLE_NAME = b.TABLE_NAME
 AND a.COLUMN_NAME = b.COLUMN_NAME
WHERE a.OWNER = UPPER(:schema_name)"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT b.TABLE_NAME AS TableName,
       b.COLUMN_NAME AS ColumnName,
       b.CONSTRAINT_NAME AS ColumnConstraintName
FROM ALL_CONSTRAINTS a
JOIN ALL_CONS_COLUMNS b
  ON a.CONSTRAINT_NAME = b.CONSTRAINT_NAME
 AND a.OWNER = b.OWNER
WHERE a.CONSTRAINT_TYPE = 'P'
  AND a.OWNER = UPPER(:schema_name)
  AND a.TABLE_NAME = UPPER(:table_name)"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT b.TABLE_NAME AS TableName,
       b.COLUMN_NAME AS ColumnName,
       b.CONSTRAINT_NAME AS ColumnConstraintName
FROM ALL_CONSTRAINTS a
JOIN ALL_CONS_COLUMNS b
  ON a.CONSTRAINT_NAME = b.CONSTRAINT_NAME
 AND a.OWNER = b.OWNER
WHERE a.CONSTRAINT_TYPE = 'P'
  AND a.OWNER = UPPER(:schema_name)"
                },
                {
                    SystemOperatorConst.TableForeign,
                    @"SELECT l_columns.TABLE_NAME AS TableName,
       l_columns.COLUMN_NAME AS ColumnName,
       constraints.CONSTRAINT_NAME AS ColumnConstraintName,
       r_columns.OWNER AS ForeignSchemaName,
       r_columns.TABLE_NAME AS ForeignTableName,
       r_columns.COLUMN_NAME AS ForeignColumnName
FROM ALL_CONSTRAINTS constraints
LEFT JOIN ALL_CONS_COLUMNS r_columns
       ON constraints.R_OWNER = r_columns.OWNER
      AND constraints.R_CONSTRAINT_NAME = r_columns.CONSTRAINT_NAME
LEFT JOIN ALL_CONS_COLUMNS l_columns
       ON constraints.OWNER = l_columns.OWNER
      AND constraints.CONSTRAINT_NAME = l_columns.CONSTRAINT_NAME
WHERE constraints.CONSTRAINT_TYPE = 'R'
  AND constraints.OWNER = UPPER(:schema_name)
  AND constraints.TABLE_NAME = UPPER(:table_name)"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    @"SELECT l_columns.TABLE_NAME AS TableName,
       l_columns.COLUMN_NAME AS ColumnName,
       constraints.CONSTRAINT_NAME AS ColumnConstraintName,
       r_columns.OWNER AS ForeignSchemaName,
       r_columns.TABLE_NAME AS ForeignTableName,
       r_columns.COLUMN_NAME AS ForeignColumnName
FROM ALL_CONSTRAINTS constraints
LEFT JOIN ALL_CONS_COLUMNS r_columns
       ON constraints.R_OWNER = r_columns.OWNER
      AND constraints.R_CONSTRAINT_NAME = r_columns.CONSTRAINT_NAME
LEFT JOIN ALL_CONS_COLUMNS l_columns
       ON constraints.OWNER = l_columns.OWNER
      AND constraints.CONSTRAINT_NAME = l_columns.CONSTRAINT_NAME
WHERE constraints.CONSTRAINT_TYPE = 'R'
  AND constraints.OWNER = UPPER(:schema_name)"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"SELECT a.TABLE_NAME AS TableName,
       b.INDEX_NAME AS IndexName,
       '' AS Indexdef,
       CASE WHEN a.UNIQUENESS = 'UNIQUE' THEN 1 ELSE 0 END AS Indisunique,
       CASE WHEN c.CONSTRAINT_TYPE = 'P' THEN 1 ELSE 0 END AS Indisprimary,
       '' AS Description,
       b.COLUMN_NAME AS ColumnName,
       b.COLUMN_POSITION AS IndexPostion,
       b.DESCEND AS IndexSort
FROM ALL_INDEXES a
LEFT JOIN ALL_IND_COLUMNS b
       ON a.TABLE_OWNER = b.TABLE_OWNER
      AND a.TABLE_NAME = b.TABLE_NAME
      AND a.INDEX_NAME = b.INDEX_NAME
LEFT JOIN ALL_CONSTRAINTS c
       ON c.OWNER = a.TABLE_OWNER
      AND c.TABLE_NAME = a.TABLE_NAME
      AND c.CONSTRAINT_NAME = b.INDEX_NAME
WHERE a.TABLE_OWNER = UPPER(:schema_name)
  AND a.TABLE_NAME = UPPER(:table_name)"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"SELECT a.TABLE_NAME AS TableName,
       b.INDEX_NAME AS IndexName,
       '' AS Indexdef,
       CASE WHEN a.UNIQUENESS = 'UNIQUE' THEN 1 ELSE 0 END AS Indisunique,
       CASE WHEN c.CONSTRAINT_TYPE = 'P' THEN 1 ELSE 0 END AS Indisprimary,
       '' AS Description,
       b.COLUMN_NAME AS ColumnName,
       b.COLUMN_POSITION AS IndexPostion,
       b.DESCEND AS IndexSort
FROM ALL_INDEXES a
LEFT JOIN ALL_IND_COLUMNS b
       ON a.TABLE_OWNER = b.TABLE_OWNER
      AND a.TABLE_NAME = b.TABLE_NAME
      AND a.INDEX_NAME = b.INDEX_NAME
LEFT JOIN ALL_CONSTRAINTS c
       ON c.OWNER = a.TABLE_OWNER
      AND c.TABLE_NAME = a.TABLE_NAME
      AND c.CONSTRAINT_NAME = b.INDEX_NAME
WHERE a.TABLE_OWNER = UPPER(:schema_name)"
                },
                { SystemOperatorConst.DbView, string.Empty },
                { SystemOperatorConst.SchemaView, string.Empty },
                { SystemOperatorConst.DbProc, string.Empty },
                { SystemOperatorConst.SchemaProc, string.Empty },
                { "ColumnSize", "SELECT '{3}' CKEY,(SUM(LENGTHB(\"{0}\"))) CSIZE FROM \"{1}\".\"{2}\" UNION " }
            };

        public override DatabaseType DatabaseType => DatabaseType.Oracle;

        private IDbHelper? _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= HasConnectionString
                ? new OracleDbHelper(ConnectionString)
                : new OracleDbHelper(DataSourceConfig);
    }
}
