using Azrng.DbOperator.Helper;

namespace Azrng.DbOperator.DbBridge
{
    public class PostgreBasicDbBridge : BasicDbBridge
    {
        public PostgreBasicDbBridge(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        public PostgreBasicDbBridge(string connectionString) : base(connectionString) { }

        public override Dictionary<string, string> QuerySqlMap =>
            new Dictionary<string, string>
            {
                {
                    SystemOperatorConst.SchemaName, @"select schema_name
                from information_schema.schemata
                where schema_name not like 'pg_%'
                  and schema_name != 'information_schema';"
                },
                {
                    SystemOperatorConst.SchemaInfo, @"SELECT n.nspname AS SchemaName, d.description AS SchemaComment
FROM pg_namespace n
         LEFT JOIN pg_description d ON n.oid = d.objoid AND d.objsubid = 0
where n.nspname not like 'pg_%'
  and n.nspname != 'information_schema';"
                },
                {
                    SystemOperatorConst.SchemaTableName, @"SELECT a.relname AS TableName, n.nspname schemaname
FROM pg_class a
         inner join pg_namespace n on a.relnamespace = n.oid
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
WHERE a.relkind = 'r'
ORDER BY a.relname"
                },
                {
                    SystemOperatorConst.SchemaTableInfoList, @"SELECT a.oid TableId, a.relname AS TableName, b.description AS TableComment
FROM pg_class a
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
WHERE a.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
  AND a.relkind = 'r'
ORDER BY a.relname"
                },
                {
                    SystemOperatorConst.SchemaTableInfo, @"SELECT a.oid TableId, a.relname AS TableName, b.description AS TableComment
FROM pg_class a
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
WHERE a.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
  AND a.relkind = 'r' and a.relname = @table_name
ORDER BY a.relname"
                },
                {
                    SystemOperatorConst.TableColumn, @"SELECT DISTINCT col.ColumnName,
                col.ColumnLength,
                col.Sort,
                col.ColumnType,
                col.ColumnDefault,
                col.IsIdentity,
                colInfo.ColumnComment,
                colInfo.IsNull,
                colInfo.IsPrimaryKey,
                CASE
                    WHEN EXISTS (SELECT 1
                                 FROM pg_constraint
                                 WHERE conrelid = colInfo.attrelid
                                   AND conkey[1] = colInfo.attnum
                                   AND contype = 'f') THEN true
                    ELSE false
                    END AS IsForeignKey
from (select column_name                                                             ColumnName,
             (case
                  when (numeric_precision is not null) then (case
                                                                 when numeric_scale <= 0
                                                                     then cast(numeric_precision AS text)
                                                                 else concat_ws(',', numeric_precision, numeric_scale) end)
                  when (datetime_precision IS NOT NULL) then cast(datetime_precision AS text)
                  when (interval_precision IS NOT NULL) then cast(datetime_precision AS text)
                  else cast(character_maximum_length AS text) end)                as ColumnLength,
             ordinal_position                                                        Sort,
             data_type                                                               ColumnType,
             column_default                                                          ColumnDefault,
             (case when column_default like '%nextval%' then true else false end) as IsIdentity
      from information_schema.columns
      where table_name = @table_name
        and table_schema = @schema_name) col
         inner join (SELECT distinct col_description(a.attrelid, a.attnum) as ColumnComment,
                                     a.attname                             as name,
                                     not a.attnotnull                      as IsNull,
                                     (
                                         CASE
                                             WHEN (SELECT COUNT(*)
                                                   FROM pg_constraint
                                                   WHERE conrelid = a.attrelid
                                                     AND a.attnum = ANY (conkey)
                                                     AND contype = 'p') > 0 THEN true
                                             ELSE false
                                             END
                                         )                                 AS IsPrimaryKey,
                                     a.attrelid,
                                     a.attnum
                     FROM pg_class as c,
                          pg_attribute as a
                     where c.relname = @table_name
                       and c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
                       and a.attrelid = c.oid
                       and a.attnum > 0) colInfo on colInfo.name = col.ColumnName
order by Sort;"
                },
                {
                    SystemOperatorConst.SchemaColumn, @"SELECT distinct *
from (select table_name                                                              TableName,
             column_name                                                             ColumnName,
             (case
                  when (numeric_precision is not null) then (case
                                                                 when numeric_scale <= 0
                                                                     then cast(numeric_precision AS text)
                                                                 else concat_ws(',', numeric_precision, numeric_scale) end)
                  when (datetime_precision IS NOT NULL) then cast(datetime_precision AS text)
                  when (interval_precision IS NOT NULL) then cast(datetime_precision AS text)
                  else cast(character_maximum_length AS text) end)                as ColumnLength,
             ordinal_position                                                        Sort,
             data_type                                                               ColumnType,
             column_default                                                          ColumnDefault,
             (case when column_default like '%nextval%' then true else false end) as IsIdentity
      from information_schema.columns
      where table_schema = @schema_name) col
         inner join (SELECT distinct c.relname                                TableName,
                                     col_description(a.attrelid, a.attnum) as ColumnComment,
                                     a.attname                             as name,
                                     not a.attnotnull                          as IsNull,
                                     (
                                         CASE
                                             WHEN (SELECT COUNT(*)
                                                   FROM pg_constraint
                                                   WHERE conrelid = a.attrelid
                                                     AND a.attnum = ANY (conkey)
                                                     AND contype = 'p') > 0 THEN true
                                             ELSE false
                                             END
                                         )                                 AS IsPrimaryKey
                     FROM pg_class as c,
                          pg_attribute as a
                     where c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema_name)
                       and a.attrelid = c.oid
                       and a.attnum > 0) colInfo on colInfo.name = col.ColumnName and colInfo.TableName = col.TableName
order by col.TableName,Sort"
                },
                {
                    SystemOperatorConst.TablePrimary,
                    @"SELECT tc.table_name as TableName, tc.constraint_name as ColumnConstraintName, kcu.column_name as ColumnName
FROM information_schema.table_constraints AS tc
         JOIN information_schema.key_column_usage AS kcu
              ON tc.constraint_name = kcu.constraint_name and tc.table_schema = kcu.table_schema
WHERE constraint_type = 'PRIMARY KEY'
  and tc.table_schema =@schema_name
  AND tc.table_name = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaPrimary,
                    @"SELECT tc.table_name as TableName, tc.constraint_name as ColumnConstraintName, kcu.column_name as ColumnName
FROM information_schema.table_constraints AS tc
         JOIN information_schema.key_column_usage AS kcu
              ON tc.constraint_name = kcu.constraint_name and tc.table_schema = kcu.table_schema
WHERE constraint_type = 'PRIMARY KEY'
  and tc.table_schema =@schema_name"
                },
                {
                    SystemOperatorConst.TableForeign, @"SELECT tc.table_name      as TableName,
       kcu.column_name    as ColumnName,
       tc.constraint_name as ColConstraintName,
       ccu.table_schema   as ForeignSchemaName,
       ccu.table_name     as ForeignTableName,
       ccu.column_name    as ForeignColumnName
FROM information_schema.table_constraints AS tc
         JOIN information_schema.key_column_usage AS kcu
              ON tc.constraint_name = kcu.constraint_name and tc.table_schema = kcu.table_schema
                  and tc.constraint_schema = kcu.constraint_schema
         JOIN information_schema.constraint_column_usage AS ccu
              ON ccu.constraint_name = tc.constraint_name and ccu.table_schema = tc.table_schema
                  and ccu.constraint_schema = tc.constraint_schema
WHERE constraint_type = 'FOREIGN KEY'
  and tc.table_schema = @schema_name
  AND tc.table_name = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaForeign, @"SELECT tc.table_name      as TableName,
       kcu.column_name    as ColumnName,
       tc.constraint_name as ColConstraintName,
       ccu.table_schema   as ForeignSchemaName,
       ccu.table_name     as ForeignTableName,
       ccu.column_name    as ForeignColumnName
FROM information_schema.table_constraints AS tc
         JOIN information_schema.key_column_usage AS kcu
              ON tc.constraint_name = kcu.constraint_name and tc.table_schema = kcu.table_schema
                  and tc.constraint_schema = kcu.constraint_schema
         JOIN information_schema.constraint_column_usage AS ccu
              ON ccu.constraint_name = tc.constraint_name and ccu.table_schema = tc.table_schema
                  and ccu.constraint_schema = tc.constraint_schema
WHERE constraint_type = 'FOREIGN KEY'
  and tc.table_schema = @schema_name;"
                },
                {
                    SystemOperatorConst.TableIndex,
                    @"select E.RELNAME                                                      as TableName,
       A.INDEXNAME                                                    as IndexName,
       A.INDEXDEF                                                     as Indexdef,
       C.INDISUNIQUE                                                  as Indisunique,
       C.INDISPRIMARY                                                 as Indisprimary,
       D.DESCRIPTION                                                  as description,
       G.attname                                                      as ColumnName,
       G.attnum                                                       as IndexPostion,
       (case when C.indoption::text = '0' then 'ASC' else 'DESC' end) as IndexSort
from PG_AM B
         left join PG_CLASS F on B.OID = F.RELAM
         left join PG_STAT_ALL_INDEXES E on F.OID = E.INDEXRELID
         left join PG_INDEX C on E.INDEXRELID = C.INDEXRELID
         left outer join PG_DESCRIPTION D on C.INDEXRELID = D.OBJOID
         left join pg_attribute G on F.oid = G.attrelid,
     PG_INDEXES A
where A.SCHEMANAME = E.SCHEMANAME
  and A.TABLENAME = E.RELNAME
  and A.INDEXNAME = E.INDEXRELNAME
  and E.SCHEMANAME = @schema_name
  and E.RELNAME = @table_name;"
                },
                {
                    SystemOperatorConst.SchemaIndex,
                    @"select E.RELNAME                                                      as TableName,
       A.INDEXNAME                                                    as IndexName,
       A.INDEXDEF                                                     as Indexdef,
       C.INDISUNIQUE                                                  as Indisunique,
       C.INDISPRIMARY                                                 as Indisprimary,
       D.DESCRIPTION                                                  as description,
       G.attname                                                      as ColumnName,
       G.attnum                                                       as IndexPostion,
       (case when C.indoption::text = '0' then 'ASC' else 'DESC' end) as IndexSort
from PG_AM B
         left join PG_CLASS F on B.OID = F.RELAM
         left join PG_STAT_ALL_INDEXES E on F.OID = E.INDEXRELID
         left join PG_INDEX C on E.INDEXRELID = C.INDEXRELID
         left outer join PG_DESCRIPTION D on C.INDEXRELID = D.OBJOID
         left join pg_attribute G on F.oid = G.attrelid,
     PG_INDEXES A
where A.SCHEMANAME = E.SCHEMANAME
  and A.TABLENAME = E.RELNAME
  and A.INDEXNAME = E.INDEXRELNAME
  and E.SCHEMANAME = @schema_name"
                },
                {
                    SystemOperatorConst.DbView,
                    @"SELECT b.description as ViewDescription,
       c.viewname    as ViewName,
       c.viewowner   as ViewOwner,
       c.definition  as ViewDefinition
FROM pg_class a
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
         inner join pg_views c on a.relname = c.viewname
         inner join pg_namespace d on a.relnamespace = d.oid
where c.schemaname not in ('pg_catalog', 'information_schema')"
                },
                {
                    SystemOperatorConst.SchemaView,
                    @"SELECT b.description as ViewDescription,
       c.viewname    as ViewName,
       c.viewowner   as ViewOwner,
       c.definition  as ViewDefinition
FROM pg_class a
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
         inner join pg_views c on a.relname = c.viewname and c.schemaname =@schema_name
         inner join pg_namespace d on a.relnamespace = d.oid and nspname =@schema_name"
                },
                {
                    SystemOperatorConst.SchemaProc,
                    @"SELECT proname  as ProcName,
       coalesce(pg_get_function_arguments(a.oid),'') inputParam,
       coalesce(pg_get_function_result(a.oid),'') outputParam,
       pg_get_functiondef(a.oid)                          as ProcDefinition,
       b.description                                      as ProcDescription
FROM pg_proc a
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
         inner join pg_namespace d on a.pronamespace = d.oid and d.nspname =@schema_name
where a.proname not in ('median')
-- median is an aggregate function 这个名称是public域下隐藏的系统聚合函数 无法查找定义.."
                },
                {
                    SystemOperatorConst.DbProc,
                    @"SELECT n.nspname                 SchemaName,
       a.proname                 ProcName,
       coalesce(pg_get_function_arguments(a.oid),'') inputParam,
       coalesce(pg_get_function_result(a.oid),'') outputParam,
       b.description             ProcDescription,
       pg_get_functiondef(a.oid) ProcDefinition
FROM pg_proc a
         LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
         inner join pg_namespace n ON n.oid = a.pronamespace
WHERE n.nspname not in ('pg_catalog', 'information_schema');"
                }
            };

        public override DatabaseType DatabaseType => DatabaseType.PostgresSql;

        private IDbHelper _dbHelper;

        public override IDbHelper DbHelper =>
            _dbHelper ??= ConnectionString is null
                ? new PostgresSqlDbHelper(DataSourceConfig)
                : new PostgresSqlDbHelper(ConnectionString);
    }
}