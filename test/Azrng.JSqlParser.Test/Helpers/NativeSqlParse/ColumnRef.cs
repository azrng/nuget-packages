namespace Azrng.JSqlParser.Test.Helpers.NativeSqlParse;

/// <summary>BuildVirtualColumn 内部用的列引用元组。</summary>
public sealed class ColumnRef
{
    public string? TableAlias { get; set; }

    public string? ColumnName { get; set; }
}
