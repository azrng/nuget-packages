using Azrng.Core.Model;
using Azrng.DataAccess.Helper;

namespace Azrng.DataAccess.DbBridge
{
    public class SqlServerBasicDbBridge : BasicDbBridge
    {
        public SqlServerBasicDbBridge(string connectionString) : base(connectionString) { }

        public SqlServerBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new()
            {
                {
                    SystemOperatorConst.SchemaName,
                    @"SELECT DISTINCT s.name AS Schema_name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;"
                },
                {
                    SystemOperatorConst.SchemaInfo,
                    @"SELECT DISTINCT s.name AS SchemaName,
       CAST(ep.value AS nvarchar(4000)) AS SchemaComment
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = s.schema_id
      AND ep.class = 3
      AND ep.name = 'MS_Description';"
                },
                {
                    SystemOperatorConst.SchemaTableName,
                    @"SELECT s.name AS SchemaName,
       t.name AS TableName
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList,
                    @"SELECT t.object_id AS TableId,
       t.name AS TableName,
       CAST(ep.value AS nvarchar(4000)) AS TableComment
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = t.object_id
      AND ep.minor_id = 0
      AND ep.name = 'MS_Description'
WHERE s.name = @schema_name;"
                },
                {
                    SystemOperatorConst.SchemaTableInfo,
                    @"SELECT t.object_id AS TableId,
       t.name AS TableName,
       CAST(ep.value AS nvarchar(4000)) AS TableComment
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = t.object_id
      AND ep.minor_id = 0
      AND ep.name = 'MS_Description'
WHERE s.name = @schema_name
  AND t.name = @table_name;"
                },
                {
                    SystemOperatorConst.TableColumn,
                    @"SELECT t.name AS TableName,
       c.name AS ColumnName,
       ty.name AS ColumnType,
       CASE
           WHEN c.max_length = -1 THEN 'MAX'
           WHEN ty.name IN ('nchar', 'nvarchar') THEN CAST(c.max_length / 2 AS varchar(50))
           ELSE CAST(c.max_length AS varchar(50))
           END AS ColumnLength,
       dc.definition AS ColumnDefault,
       CAST(ep.value AS nvarchar(4000)) AS ColumnComment,
       CONVERT(bit, COLUMNPROPERTY(c.object_id, c.name, 'IsIdentity')) AS IsIdentity,
       CONVERT(bit, c.is_nullable) AS IsNull,
       CONVERT(bit, CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END) AS IsPrimaryKey,
       CONVERT(bit, CASE WHEN fk.parent_column_id IS NULL THEN 0 ELSE 1 END) AS IsForeignKey,
       c.column_id AS RowNumber
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = c.object_id
      AND ep.minor_id = c.column_id
      AND ep.name = 'MS_Description'
LEFT JOIN (
    SELECT ic.object_id, ic.column_id
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic
            ON i.object_id = ic.object_id
           AND i.index_id = ic.index_id
    WHERE i.is_primary_key = 1
) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
LEFT JOIN sys.foreign_key_columns fk
       ON fk.parent_object_id = c.object_id
      AND fk.parent_column_id = c.column_id
WHERE s.name = @schema_name
  AND t.name = @table_name
ORDER BY c.column_id;"
                },
                {
                    SystemOperatorConst.SchemaColumn,
                    @"SELECT t.name AS TableName,
       c.name AS ColumnName,
       ty.name AS ColumnType,
       CASE
           WHEN c.max_length = -1 THEN 'MAX'
           WHEN ty.name IN ('nchar', 'nvarchar') THEN CAST(c.max_length / 2 AS varchar(50))
           ELSE CAST(c.max_length AS varchar(50))
           END AS ColumnLength,
       dc.definition AS ColumnDefault,
       CAST(ep.value AS nvarchar(4000)) AS ColumnComment,
       CONVERT(bit, COLUMNPROPERTY(c.object_id, c.name, 'IsIdentity')) AS IsIdentity,
       CONVERT(bit, c.is_nullable) AS IsNull,
       CONVERT(bit, CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END) AS IsPrimaryKey,
       CONVERT(bit, CASE WHEN fk.parent_column_id IS NULL THEN 0 ELSE 1 END) AS IsForeignKey,
       ROW_NUMBER() OVER (PARTITION BY t.object_id ORDER BY c.column_id) AS RowNumber
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = c.object_id
      AND ep.minor_id = c.column_id
      AND ep.name = 'MS_Description'
LEFT JOIN (
    SELECT ic.object_id, ic.column_id
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic
            ON i.object_id = ic.object_id
           AND i.index_id = ic.index_id
    WHERE i.is_primary_key = 1
) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
LEFT JOIN sys.foreign_key_columns fk
       ON fk.parent_object_id = c.object_id
      AND fk.parent_column_id = c.column_id
WHERE s.name = @schema_name
ORDER BY t.name, c.column_id;"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT kcu.TABLE_NAME AS TableName,
       kcu.COLUMN_NAME AS ColumnName,
       kcu.CONSTRAINT_NAME AS ColumnConstraintName
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
  ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
 AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND tc.TABLE_SCHEMA = @schema_name
  AND tc.TABLE_NAME = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT kcu.TABLE_NAME AS TableName,
       kcu.COLUMN_NAME AS ColumnName,
       kcu.CONSTRAINT_NAME AS ColumnConstraintName
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
  ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
 AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND tc.TABLE_SCHEMA = @schema_name;"
                },
                {
                    SystemOperatorConst.TableForeign,
                    @"SELECT pt.name AS TableName,
       pc.name AS ColumnName,
       fk.name AS ColumnConstraintName,
       rs.name AS ForeignSchemaName,
       rt.name AS ForeignTableName,
       rc.name AS ForeignColumnName
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables pt ON fkc.parent_object_id = pt.object_id
INNER JOIN sys.schemas ps ON pt.schema_id = ps.schema_id
INNER JOIN sys.columns pc
        ON fkc.parent_object_id = pc.object_id
       AND fkc.parent_column_id = pc.column_id
INNER JOIN sys.tables rt ON fkc.referenced_object_id = rt.object_id
INNER JOIN sys.schemas rs ON rt.schema_id = rs.schema_id
INNER JOIN sys.columns rc
        ON fkc.referenced_object_id = rc.object_id
       AND fkc.referenced_column_id = rc.column_id
WHERE ps.name = @schema_name
  AND pt.name = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaForeign,
                    @"SELECT pt.name AS TableName,
       pc.name AS ColumnName,
       fk.name AS ColumnConstraintName,
       rs.name AS ForeignSchemaName,
       rt.name AS ForeignTableName,
       rc.name AS ForeignColumnName
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables pt ON fkc.parent_object_id = pt.object_id
INNER JOIN sys.schemas ps ON pt.schema_id = ps.schema_id
INNER JOIN sys.columns pc
        ON fkc.parent_object_id = pc.object_id
       AND fkc.parent_column_id = pc.column_id
INNER JOIN sys.tables rt ON fkc.referenced_object_id = rt.object_id
INNER JOIN sys.schemas rs ON rt.schema_id = rs.schema_id
INNER JOIN sys.columns rc
        ON fkc.referenced_object_id = rc.object_id
       AND fkc.referenced_column_id = rc.column_id
WHERE ps.name = @schema_name;"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"SELECT t.name AS TableName,
       i.name AS IndexName,
       '' AS Indexdef,
       CONVERT(bit, i.is_unique) AS Indisunique,
       CONVERT(bit, i.is_primary_key) AS Indisprimary,
       CAST(ep.value AS nvarchar(4000)) AS Description,
       c.name AS ColumnName,
       ic.key_ordinal AS IndexPostion,
       CASE WHEN ic.is_descending_key = 1 THEN 'DESC' ELSE 'ASC' END AS IndexSort
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.index_columns ic
       ON i.object_id = ic.object_id
      AND i.index_id = ic.index_id
LEFT JOIN sys.columns c
       ON ic.object_id = c.object_id
      AND ic.column_id = c.column_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = i.object_id
      AND ep.minor_id = i.index_id
      AND ep.name = 'MS_Description'
WHERE s.name = @schema_name
  AND t.name = @table_name
  AND i.index_id > 0
ORDER BY i.name, ic.key_ordinal;"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"SELECT t.name AS TableName,
       i.name AS IndexName,
       '' AS Indexdef,
       CONVERT(bit, i.is_unique) AS Indisunique,
       CONVERT(bit, i.is_primary_key) AS Indisprimary,
       CAST(ep.value AS nvarchar(4000)) AS Description,
       c.name AS ColumnName,
       ic.key_ordinal AS IndexPostion,
       CASE WHEN ic.is_descending_key = 1 THEN 'DESC' ELSE 'ASC' END AS IndexSort
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.index_columns ic
       ON i.object_id = ic.object_id
      AND i.index_id = ic.index_id
LEFT JOIN sys.columns c
       ON ic.object_id = c.object_id
      AND ic.column_id = c.column_id
LEFT JOIN sys.extended_properties ep
       ON ep.major_id = i.object_id
      AND ep.minor_id = i.index_id
      AND ep.name = 'MS_Description'
WHERE s.name = @schema_name
  AND i.index_id > 0
ORDER BY t.name, i.name, ic.key_ordinal;"
                },
                {
                    SystemOperatorConst.SchemaView,
                    @"SELECT CAST(e.value AS nvarchar(4000)) AS ViewDescription,
       v.TABLE_NAME AS ViewName,
       u.name AS ViewOwner,
       v.VIEW_DEFINITION AS ViewDefinition
FROM INFORMATION_SCHEMA.VIEWS v
INNER JOIN sys.views sv ON sv.name = v.TABLE_NAME
INNER JOIN sys.schemas s ON sv.schema_id = s.schema_id
LEFT JOIN sys.extended_properties e
       ON e.major_id = sv.object_id
      AND e.minor_id = 0
      AND e.name = 'MS_Description'
LEFT JOIN sys.sysusers u ON u.uid = sv.principal_id
WHERE s.name = @schema_name;"
                },
                {
                    SystemOperatorConst.SchemaProc,
                    @"SELECT p.SPECIFIC_NAME AS ProcName,
       NULL AS InputParam,
       NULL AS OutputParam,
       p.ROUTINE_DEFINITION AS ProcDefinition,
       CAST(e.value AS nvarchar(4000)) AS ProcDescription
FROM INFORMATION_SCHEMA.ROUTINES p
INNER JOIN sys.procedures sp ON sp.name = p.SPECIFIC_NAME
INNER JOIN sys.schemas s ON sp.schema_id = s.schema_id
LEFT JOIN sys.extended_properties e
       ON e.major_id = sp.object_id
      AND e.minor_id = 0
      AND e.name = 'MS_Description'
WHERE p.ROUTINE_TYPE = 'PROCEDURE'
  AND s.name = @schema_name;"
                },
                { SystemOperatorConst.DbView, string.Empty },
                { SystemOperatorConst.DbProc, string.Empty }
            };

        public override DatabaseType DatabaseType => DatabaseType.SqlServer;

        private IDbHelper? _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= HasConnectionString
                ? new SqlServerDbHelper(ConnectionString)
                : new SqlServerDbHelper(DataSourceConfig);
    }
}
