namespace Azrng.JSqlParser.Test.Helpers.NativeSqlParse;

public sealed class NativeSqlSelectColumnInfo
{
    public string? TableAlias { get; set; }

    public string? ColumnName { get; set; }

    public string? ColumnAlias { get; set; }

    public bool IsVirtual { get; set; }

    public string? ExpressionSql { get; set; }

    public string? ColumnType { get; set; }

    public string? SourceTableAlias { get; set; }

    public string? SourceColumnName { get; set; }
}
