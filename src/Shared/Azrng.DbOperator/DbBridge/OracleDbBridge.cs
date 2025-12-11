using Azrng.DbOperator.Helper;

namespace Azrng.DbOperator.DbBridge
{
    /// <summary>
    /// oracle 系统操作
    /// </summary>
    public class OracleBasicDbBridge : BasicBasicDbBridge
    {
        public OracleBasicDbBridge(string connectionString) : base(connectionString) { }
        public OracleBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
        }

        public override Dictionary<string, string> QuerySqlMap => new Dictionary<string, string>
        {
            {"Schema",  $"select '{DataSourceConfig.DbName}' as Schema_name from dual" },
            {"SchemaTable", "select tc.TABLE_NAME AS TableName,COMMENTS AS TableComment from all_tables  tb join all_tab_comments  tc on tb.TABLE_NAME=tc.TABLE_NAME and tb.OWNER=tc.OWNER where tb.OWNER = :schema_name and tb.STATUS='VALID'"},
            {"TableColumn", "select a.table_name as TableName,a.COLUMN_NAME AS ColName, a.DATA_TYPE AS ColType, a.CHAR_COL_DECL_LENGTH AS ColLength, a.DATA_DEFAULT AS ColDefault, b.COMMENTS as ColComment,1 as IsIdentity, (case a.NULLABLE when 'Y' then 0 else 1 end) AS Is_Null from all_tab_columns a INNER JOIN all_col_comments b on a.table_name =b.table_name and a.column_name =b.column_name and a.OWNER=b.OWNER where a.OWNER=:schema_name and A.Table_Name=:table_name"},
            {"SchemaColumn", "select a.table_name as TableName,a.COLUMN_NAME AS ColName, a.DATA_TYPE AS ColType, a.CHAR_COL_DECL_LENGTH AS ColLength, a.DATA_DEFAULT AS ColDefault, b.COMMENTS as ColComment,1 as IsIdentity, (case a.NULLABLE when 'Y' then 0 else 1 end) AS Is_Null from all_tab_columns a INNER JOIN all_col_comments b on a.table_name =b.table_name and a.column_name =b.column_name and a.OWNER=b.OWNER where a.OWNER=:schema_name"},
            {"TablePrimary", " Select b.constraint_name as ColConstraintName, b.column_name as ColName From all_Constraints a, all_Cons_Columns b Where a.Constraint_Type = 'P' and a.Constraint_Name = b.Constraint_Name And a.Owner = b.Owner And a.table_name = b.table_name and a.owner= upper(:schema_name) And a.table_name= upper(:table_name)"},
            {"SchemaPrimary", "Select a.table_name as TableName,b.constraint_name as ColConstraintName, b.column_name as ColName From all_Constraints a, all_Cons_Columns b Where a.Constraint_Type = 'P' and a.Constraint_Name = b.Constraint_Name And a.Owner = b.Owner And a.table_name = b.table_name and a.owner= upper(:schema_name)"},
            {"TableForeign", "select constraints.CONSTRAINT_NAME as ColConstraintName, l_columns.COLUMN_NAME as ColName, r_columns.OWNER as ForeignSchemaName, r_columns.TABLE_NAME as ForeignTableName, r_columns.COLUMN_NAME as ForeignColumnName from all_constraints constraints left join all_cons_columns r_columns on constraints.R_CONSTRAINT_NAME = r_columns.CONSTRAINT_NAME left join all_cons_columns l_columns on constraints.CONSTRAINT_NAME = l_columns.CONSTRAINT_NAME WHERE constraints.constraint_type= 'R' and constraints.owner = :schema_name AND constraints.table_name= :table_name"},
            {"SchemaForeign", "select constraints.table_name as TableName,constraints.CONSTRAINT_NAME as ColConstraintName, l_columns.COLUMN_NAME as ColName, r_columns.OWNER as ForeignSchemaName, r_columns.TABLE_NAME as ForeignTableName, r_columns.COLUMN_NAME as ForeignColumnName from all_constraints constraints left join all_cons_columns r_columns on constraints.R_CONSTRAINT_NAME = r_columns.CONSTRAINT_NAME left join all_cons_columns l_columns on constraints.CONSTRAINT_NAME = l_columns.CONSTRAINT_NAME WHERE constraints.constraint_type= 'R' and constraints.owner = :schema_name"},
            {"TableIndex", "select (case a.UNIQUENESS when 'UNIQUE' then 0 else 1 end) as Indisunique, b.COLUMN_NAME as ColName, b.INDEX_NAME as IndexName, '' as Indexdef, '' as description, b.DESCEND as IndexSort, b.COLUMN_POSITION as IndexPostion, (case c.constraint_type WHEN 'P' then 0 else 1 end) as Indisprimary from all_indexes a left join all_ind_columns b on a.table_name = b.table_name and a.INDEX_NAME = b.INDEX_NAME left join all_constraints c on c.TABLE_NAME=a.TABLE_NAME and c.constraint_name = b.INDEX_NAME where a.TABLE_OWNER=:schema_name and a.table_name=:table_name"},
            { "SchemaIndex", "select a.table_name as TableName,(case a.UNIQUENESS when 'UNIQUE' then 0 else 1 end) as Indisunique, b.COLUMN_NAME as ColName, b.INDEX_NAME as IndexName, '' as Indexdef, '' as description, b.DESCEND as IndexSort, b.COLUMN_POSITION as IndexPostion, (case c.constraint_type WHEN 'P' then 0 else 1 end) as Indisprimary from all_indexes a left join all_ind_columns b on a.table_name = b.table_name and a.INDEX_NAME = b.INDEX_NAME left join all_constraints c on c.TABLE_NAME=a.TABLE_NAME and c.constraint_name = b.INDEX_NAME where a.TABLE_OWNER=:schema_name"},
            { "ColumnSize", "SELECT '{3}' CKEY,(SUM(LENGTHB(\"{0}\")))  CSIZE FROM \"{1}\".\"{2}\"  UNION "}
        };
        public override DatabaseType DatabaseType => DatabaseType.Oracle;

        private IDbHelper _dbHelper;
        public override IDbHelper DbHelper => _dbHelper ??= new OracleDbHelper(DataSourceConfig);
    }
}