namespace Azrng.JSqlParser.Test.Helpers.NativeSqlParse;

public sealed class NativeSqlOperatorInfo
{
    public string? Alias { get; set; }

    public string? LinkType { get; set; }

    public string? TableName { get; set; }

    public NativeSqlExpressionInfo? LeftExpression { get; set; }

    public NativeSqlExpressionInfo? RightExpression { get; set; }

    public string? StringExpression { get; set; }

    public string? SqlInfo { get; set; }

    public int Order { get; set; }
}
