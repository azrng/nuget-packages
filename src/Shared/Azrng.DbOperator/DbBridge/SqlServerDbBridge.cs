using Azrng.DbOperator.Helper;

namespace Azrng.DbOperator.DbBridge
{
    /// <summary>
    /// sqlserver 系统操作
    /// </summary>
    public class SqlServerBasicDbBridge : BasicBasicDbBridge
    {
        public SqlServerBasicDbBridge(string connectionString) : base(connectionString) { }

        public SqlServerBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new Dictionary<string, string>
            {
                {
                    "Schema",
                    "SELECT distinct b.name as Schema_name, ep.value AS Schema_comment from sys.tables a inner join sys.schemas b on a.schema_id=b.schema_id LEFT JOIN  sys.extended_properties ep ON ep.major_id = b.schema_id AND ep.class = 3 AND ep.name = 'MS_Description';"
                },
                {
                    "SchemaTable",
                    "SELECT a.name as TableName,id,(select top 1 value from sys.extended_properties where id = major_id) as TableComment from sys.sysobjects a inner join sys.tables b on a.id = b.object_id inner join sys.schemas c on b.schema_id = c.schema_id where xtype = 'U' and c.name = @schema_name"
                },
                {
                    "TableColumn",
                    "SELECT [Is_IDENTITY] = case when COLUMNPROPERTY([Columns].object_id,[Columns].name,'IsIdentity')=1   then 1 else 0 end,[ColName] = [Columns].name , [ColType] = [Types].name , [ColLength] = [Columns].max_length , [Is_Null] = [Columns].is_nullable , [ColComment] = [Properties].value, [INFORMATION].COLUMN_DEFAULT as ColDefault FROM sys.tables AS [Tables] INNER JOIN sys.columns AS [Columns] ON [Tables].object_id = [Columns].object_id INNER JOIN sys.types AS [Types] ON [Columns].system_type_id = [Types].system_type_id AND is_user_defined = 0 AND [Types].name <> 'sysname' inner join INFORMATION_SCHEMA.COLUMNS [INFORMATION] on [INFORMATION].TABLE_NAME=[Tables].name and [INFORMATION].COLUMN_NAME=[Columns].name LEFT OUTER JOIN sys.extended_properties AS [Properties] ON [Properties].major_id = [Tables].object_id AND [Properties].minor_id = [Columns].column_id AND [Properties].name = 'MS_Description' WHERE [INFORMATION].TABLE_SCHEMA =@schema_name and [Tables].name =@table_name ORDER BY [Columns].column_id"
                },
                {
                    "SchemaColumn",
                    "SELECT   obj.name AS [TableName],col.is_identity AS [Is_IDENTITY],col.name AS [ColName],typ.name AS [ColType],col.max_length AS [ColLength],col.is_nullable AS [Is_Null],  sep.value AS [ColComment], def.definition AS [ColDefault] FROM sys.columns col INNER JOIN sys.objects obj ON col.object_id = obj.object_id INNER JOIN sys.schemas sch ON obj.schema_id = sch.schema_id LEFT JOIN  sys.types typ ON col.user_type_id = typ.user_type_id LEFT JOIN  sys.extended_properties sep ON col.object_id = sep.major_id AND col.column_id = sep.minor_id  AND sep.class = 1  AND sep.name = 'MS_Description' LEFT JOIN sys.default_constraints def ON col.default_object_id = def.object_id WHERE sch.name = @schema_name ORDER BY col.column_id"
                },
                {
                    "TablePrimary",
                    "SELECT COLUMN_NAME as ColName, kcu.CONSTRAINT_NAME as ColConstraintName from INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc  JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' and tc.TABLE_NAME = @table_name and kcu.CONSTRAINT_SCHEMA = @schema_name"
                },
                {
                    "SchemaPrimary",
                    "SELECT kcu.TABLE_NAME as TableName,COLUMN_NAME as ColName, kcu.CONSTRAINT_NAME as ColConstraintName from INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc  JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' and kcu.CONSTRAINT_SCHEMA = @schema_name"
                },
                {
                    "TableForeign",
                    "SELECT fk.name as ColConstraintName , fcn.name as ColName , rtable.name as ForeignTableName, rcn.name as ForeignColumnName, 'dbo' as ForeignSchemaName FROM sysforeignkeys JOIN sysobjects fk ON sysforeignkeys.constid = fk.id JOIN sysobjects ftable ON sysforeignkeys.fkeyid = ftable.id JOIN sysobjects rtable ON sysforeignkeys.rkeyid = rtable.id JOIN syscolumns fcn ON sysforeignkeys.fkeyid = fcn.id AND sysforeignkeys.fkey = fcn.colid JOIN syscolumns rcn ON sysforeignkeys.rkeyid = rcn.id AND sysforeignkeys.rkey = rcn.colid WHERE ftable.name = @table_name"
                },
                {
                    "SchemaForeign",
                    "SELECT  ftable.name as TableName,fk.name as ColConstraintName , fcn.name as ColName , rtable.name as ForeignTableName, rcn.name as ForeignColumnName, 'dbo' as ForeignSchemaName FROM sysforeignkeys JOIN sysobjects fk ON sysforeignkeys.constid = fk.id JOIN sysobjects ftable ON sysforeignkeys.fkeyid = ftable.id JOIN sysobjects rtable ON sysforeignkeys.rkeyid = rtable.id JOIN syscolumns fcn ON sysforeignkeys.fkeyid = fcn.id AND sysforeignkeys.fkey = fcn.colid JOIN syscolumns rcn ON sysforeignkeys.rkeyid = rcn.id AND sysforeignkeys.rkey = rcn.colid"
                },
                {
                    "TableIndex",
                    " SELECT a.name as IndexName, '' as Indexdef, a.is_unique as Indisunique, a.is_primary_key as Indisprimary, '' as description, d.ORDINAL_POSITION as IndexPostion, (CASE INDEXKEY_PROPERTY(b.[object_id],b.index_id,b.index_column_id,'IsDescending') WHEN 1 THEN 'DESC' WHEN 0 THEN 'ASC' ELSE '' END) as IndexSort, e.name as ColName FROM sys.indexes a (NOLOCK) INNER JOIN sys.index_columns b (NOLOCK) ON a.object_id = b.object_id and a.index_id = b.index_id INNER JOIN sysindexkeys c (NOLOCK) ON a.object_id = c.id and b.index_id = c.indid and b.column_id = c.colid INNER JOIN INFORMATION_SCHEMA.COLUMNS d (NOLOCK) ON a.object_id = object_id(d.TABLE_NAME) and c.keyno = d.ORDINAL_POSITION left join syscolumns e (nolock) on e.id = c.id and c.colid=e.colid WHERE a.object_id = object_id(@table_name) ORDER BY a.name,d.ORDINAL_POSITION"
                },
                {
                    "SchemaIndex",
                    "SELECT object_name(a.object_id) as TableName, a.name as IndexName, '' as Indexdef, a.is_unique as Indisunique, a.is_primary_key as Indisprimary, '' as description, d.ORDINAL_POSITION as IndexPostion, (CASE INDEXKEY_PROPERTY(b.[object_id],b.index_id,b.index_column_id,'IsDescending') WHEN 1 THEN 'DESC' WHEN 0 THEN 'ASC' ELSE '' END) as IndexSort, e.name as ColName FROM sys.indexes a (NOLOCK) INNER JOIN sys.index_columns b (NOLOCK) ON a.object_id = b.object_id and a.index_id = b.index_id INNER JOIN sysindexkeys c (NOLOCK) ON a.object_id = c.id and b.index_id = c.indid and b.column_id = c.colid INNER JOIN INFORMATION_SCHEMA.COLUMNS d (NOLOCK) ON a.object_id = object_id(d.TABLE_NAME) and c.keyno = d.ORDINAL_POSITION left join syscolumns e (nolock) on e.id = c.id and c.colid=e.colid  ORDER BY a.name,d.ORDINAL_POSITION"
                },
                {
                    "SchemaView",
                    @"SELECT e.value  as ViewDescription,v.TABLE_NAME as ViewName,  l.name   as ViewOwner, v.VIEW_DEFINITION as ViewDefinition from sys.sysobjects s inner join INFORMATION_SCHEMA.VIEWS v on s.name = v.TABLE_NAME and s.type = 'V' and v.TABLE_SCHEMA = @schema_name inner join sys.schemas m on s.uid = m.schema_id and m.name = @schema_name left join sys.extended_properties e on e.major_id = s.id and e.name = 'MS_Description' left join sys.sysusers u on u.uid = s.uid  left join sys.syslogins l on l.sid = u.sid"
                },
                {
                    "SchemaProc",
                    @"SELECT p.SPECIFIC_NAME  as ProcName, p.ROUTINE_DEFINITION as ProcDefinition, e.value as ProcDescription from sys.sysobjects s inner join INFORMATION_SCHEMA.ROUTINES p on s.name = p.SPECIFIC_NAME and s.type='P' and p.SPECIFIC_SCHEMA=@schema_name inner join sys.schemas m on s.uid = m.schema_id and m.name = @schema_name left join sys.extended_properties e on e.major_id = s.id and e.name = 'MS_Description'"
                }
            };

        public override DatabaseType DatabaseType => DatabaseType.SqlServer;

        private IDbHelper _dbHelper;

        public override IDbHelper DbHelper => _dbHelper ??= new SqlServerDbHelper(DataSourceConfig);
    }
}