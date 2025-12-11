using CommonCollect.GenDbSqlBridge.Model;
using System.Text;

namespace CommonCollect.GenDbSqlBridge;

/// <summary>
/// pgsql桥接
/// </summary>
public class PgsqlBridge : GenSqlBase
{
    public PgsqlBridge(StructDbTypeEnum originDbType) : base(originDbType) { }

    public override StructDbTypeEnum CurrDbType => StructDbTypeEnum.PG;

    public override StringBuilder AddSchemaSql(params string[] schema)
    {
        var sb = new StringBuilder();
        foreach (var item in schema)
        {
            sb.Append($"create schema if not exists {item};");
        }

        return sb;
    }

    public override StringBuilder RemoveSchemaSql(params string[] schema)
    {
        var sb = new StringBuilder();
        foreach (var item in schema)
        {
            sb.Append($"drop schema {item} cascade;");
        }

        return sb;
    }

    public override StringBuilder AddTableSql(List<TableStructInfo> tableStructInfos)
    {
        var sb = new StringBuilder();
        var commentSql = new StringBuilder();
        foreach (var table in tableStructInfos)
        {
            sb.Append($"create schema if not exists {table.SchemaName};")
              .Append($"CREATE TABLE {table.SchemaName}.{table.TableName}(");
            if (table.TableComment.IsNotNullOrWhiteSpace())
                commentSql.Append($"COMMENT ON TABLE {table.SchemaName}.{table.TableName} IS '{table.TableComment}';");

            foreach (var column in table.Columns)
            {
                var primarySql = column.IsPrimary ? $"constraint {table.TableName}_pk primary key" : "";

                var columnType = ConvertColumnType(column.StructColType, column.StructColLength);
                var defaultValue = column.StructDefaultValue.IsNullOrWhiteSpace()
                    ? string.Empty
                    : $" default '{column.StructDefaultValue}'";
                sb.Append($"{column.ColName} {columnType} {primarySql} {(column.Is_Null ? "NULL" : "NOT NULL")} {defaultValue},");

                if (column.ColComment.IsNotNullOrWhiteSpace())
                    commentSql.Append($"COMMENT ON COLUMN {table.SchemaName}.{table.TableName}.{column.ColName} IS '{column.ColComment}';");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(");");
        }

        sb.Append(commentSql);

        return sb;
    }

    public override StringBuilder RemoveTableSql(List<RemoveTableInfo> tableStructInfos)
    {
        var sb = new StringBuilder();
        foreach (var table in tableStructInfos)
        {
            sb.Append($"drop table {table.SchemaName}.{table.TableName} cascade;");
        }

        return sb;
    }

    public override StringBuilder AddColumnSql(List<ColumnBaseBo> columnBaseBos)
    {
        var sb = new StringBuilder();
        var commentSql = new StringBuilder();
        foreach (var column in columnBaseBos)
        {
            var columnType = ConvertColumnType(column.ColType);
            sb.Append($"alter table {column.SchemaName}.{column.TableName} add {column.ColName} {columnType};");

            if (column.ColComment.IsNotNullOrWhiteSpace())
                commentSql.Append($"COMMENT ON COLUMN {column.SchemaName}.{column.TableName}.{column.ColName} IS '{column.ColComment}';");
        }

        sb.Append(commentSql);
        return sb;
    }

    public override StringBuilder RemoveColumnSql(List<RemoveColumnInfo> columnInfos)
    {
        var sb = new StringBuilder();
        foreach (var column in columnInfos)
        {
            sb.Append($"alter table {column.SchemaName}.{column.TableName}  drop column {column.ColumnName};");
        }

        return sb;
    }
}