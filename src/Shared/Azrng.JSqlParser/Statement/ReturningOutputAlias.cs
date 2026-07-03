using System.Text;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// RETURNING WITH 子句中的输出别名定义，如 RETURNING WITH (OLD AS o, NEW AS n)。
/// 移植自上游 JSqlParser commit f47a8b30 的 ReturningOutputAlias。
/// </summary>
public class ReturningOutputAlias
{
    public ReturningReferenceType ReferenceType { get; set; }
    public string? Alias { get; set; }

    public ReturningOutputAlias() { }

    public ReturningOutputAlias(ReturningReferenceType referenceType, string? alias)
    {
        ReferenceType = referenceType;
        Alias = alias;
    }

    public StringBuilder AppendTo(StringBuilder builder) =>
        builder.Append(ReferenceType).Append(" AS ").Append(Alias);

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
