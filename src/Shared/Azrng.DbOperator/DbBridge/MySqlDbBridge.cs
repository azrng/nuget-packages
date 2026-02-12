using Azrng.DbOperator.Helper;

namespace Azrng.DbOperator.DbBridge
{
    /// <summary>
    /// mysql 系统操作
    /// </summary>
    public class MySqlBasicDbBridge : BasicDbBridge
    {
        public MySqlBasicDbBridge(string connectionString) : base(connectionString) { }

        public MySqlBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new Dictionary<string, string>
            {
                {
                    SystemOperatorConst.SchemaName,
                    "SELECT SCHEMA_NAME as Schema_name FROM information_schema.SCHEMATA s where s.SCHEMA_NAME !='information_schema' and s.SCHEMA_NAME !='performance_schema' and s.SCHEMA_NAME !='sys' and s.SCHEMA_NAME !='mysql';"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    "SELECT TABLE_NAME as TableName, TABLE_COMMENT as TableComment FROM information_schema.TABLES t WHERE t.TABLE_SCHEMA = @schema_name;"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    "SELECT TABLE_NAME as TableName, TABLE_COMMENT as TableComment FROM information_schema.TABLES t WHERE t.TABLE_SCHEMA = @schema_name and TABLE_NAME = @table_name;"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT (case when EXTRA = 'auto_increment' then true else false end) as                                      IsIdentity,
                           TABLE_Name TableName,
                           COLUMN_NAME                                                   AS                                      ColumnName,
                           IF(COLUMN_TYPE like '%(%' && NUMERIC_PRECISION is not null && NUMERIC_SCALE is not null,
                              replace(replace(replace(COLUMN_TYPE, DATA_TYPE, ''), '(', ''), ')', ''), CHARACTER_MAXIMUM_LENGTH) ColumnLength,
                           DATA_TYPE                                                     AS                                      ColumnType,
                           COLUMN_DEFAULT                                                AS                                      ColumnDefault,
                           COLUMN_COMMENT                                                AS                                      ColumnComment,
                           (case IS_NULLABLE when 'YES' then true else false end)        AS                                      IsNull,
                           COLUMN_KEY ColumnKey,
                           ORDINAL_POSITION rowNumber
                    FROM information_schema.`COLUMNS`
                    where TABLE_SCHEMA = @schema_name
                      and TABLE_NAME = @table_name"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT (case when EXTRA = 'auto_increment' then true else false end) as                                      IsIdentity,
                           TABLE_Name TableName,
                           COLUMN_NAME                                                   AS                                      ColumnName,
                           IF(COLUMN_TYPE like '%(%' && NUMERIC_PRECISION is not null && NUMERIC_SCALE is not null,
                              replace(replace(replace(COLUMN_TYPE, DATA_TYPE, ''), '(', ''), ')', ''), CHARACTER_MAXIMUM_LENGTH) ColumnLength,
                           DATA_TYPE                                                     AS                                      ColumnType,
                           COLUMN_DEFAULT                                                AS                                      ColumnDefault,
                           COLUMN_COMMENT                                                AS                                      ColumnComment,
                           (case IS_NULLABLE when 'YES' then true else false end)        AS                                      IsNull,
                           COLUMN_KEY ColumnKey,
                           ORDINAL_POSITION rowNumber
                    FROM information_schema.`COLUMNS`
                    where TABLE_SCHEMA = @schema_name"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT TABLE_NAME TableName,COLUMN_NAME AS ColumnName, constraint_name as ColumnConstraintName,ORDINAL_POSITION as ColumnSort
                FROM INFORMATION_SCHEMA.`KEY_COLUMN_USAGE`
                WHERE table_name = @table_name
                  AND CONSTRAINT_SCHEMA = @schema_name
                  AND CONSTRAINT_NAME = 'PRIMARY'"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT TABLE_NAME TableName,COLUMN_NAME AS ColumnName, constraint_name as ColumnConstraintName,ORDINAL_POSITION as ColumnSort
                FROM INFORMATION_SCHEMA.`KEY_COLUMN_USAGE`
                WHERE TABLE_SCHEMA=@schema_name AND CONSTRAINT_NAME = 'PRIMARY'"
                },
                {
                    SystemOperatorConst.TableForeign,
                    "select COLUMN_NAME AS ColName, CONSTRAINT_NAME AS ColConstraintName, REFERENCED_TABLE_SCHEMA AS ForeignSchemaName, REFERENCED_TABLE_NAME AS ForeignTableName, REFERENCED_COLUMN_NAME AS ForeignColumnName from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where table_schema=@schema_name and table_name=@table_name AND CONSTRAINT_NAME !='PRIMARY' AND REFERENCED_TABLE_SCHEMA !=''"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    "select table_name as TableName,COLUMN_NAME AS ColName, CONSTRAINT_NAME AS ColConstraintName, REFERENCED_TABLE_SCHEMA AS ForeignSchemaName, REFERENCED_TABLE_NAME AS ForeignTableName, REFERENCED_COLUMN_NAME AS ForeignColumnName from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where table_schema=@schema_name AND CONSTRAINT_NAME !='PRIMARY' AND REFERENCED_TABLE_SCHEMA !=''"
                },
                {
                    SystemOperatorConst.TableIndex,
                    "SELECT a.index_name as IndexName, '' as Indexdef, a.non_unique as Indisunique, a.index_comment as description, a.column_name as ColName, a.SEQ_IN_INDEX as IndexPostion, (case a.COLLATION when 'D' then 'desc' else 'asc' end) as IndexSort, (case when (select c.constraint_name from information_schema.`TABLE_CONSTRAINTS` c where c.constraint_name = a.index_name and a.TABLE_SCHEMA=c.TABLE_SCHEMA and a.table_name=c.table_name ) != '' then 0 else 1 end) as Indisprimary FROM information_schema.statistics a where a.TABLE_SCHEMA=@schema_name and a.table_name=@table_name and a.index_name != 'PRIMARY'"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    "SELECT a.table_name as TableName,a.index_name as IndexName, '' as Indexdef, a.non_unique as Indisunique, a.index_comment as description, a.column_name as ColName, a.SEQ_IN_INDEX as IndexPostion, (case a.COLLATION when 'D' then 'desc' else 'asc' end) as IndexSort, (case when (select c.constraint_name from information_schema.`TABLE_CONSTRAINTS` c where c.constraint_name = a.index_name and a.TABLE_SCHEMA=c.TABLE_SCHEMA and a.table_name=c.table_name ) != '' then 0 else 1 end) as Indisprimary FROM information_schema.statistics a where a.TABLE_SCHEMA=@schema_name  and a.index_name != 'PRIMARY'"
                },
                { "ColumnSize", "SELECT '{3}' CKEY,(SUM(OCTET_LENGTH({0})))  CSIZE FROM {1}.{2};" }
            };

        public override DatabaseType DatabaseType => DatabaseType.MySql;

        private IDbHelper _dbHelper;

        public override IDbHelper DbHelper => _dbHelper ??= new MySqlDbHelper(DataSourceConfig);
    }
}