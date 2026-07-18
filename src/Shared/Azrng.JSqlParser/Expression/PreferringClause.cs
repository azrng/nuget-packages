using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Exasol Skyline PREFERRING clause.
/// Used in SELECT, UPDATE, DELETE statements.
/// </summary>
public class PreferringClause : ASTNodeAccessImpl
{
    public required IExpression Preferring { get; set; }
    public ExpressionList? PartitionBy { get; set; }

    public PreferringClause() { }

    [SetsRequiredMembers]
    public PreferringClause(IExpression preferring)
    {
        Preferring = preferring;
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("PREFERRING ").Append(Preferring);
        if (PartitionBy != null)
            builder.Append(" PARTITION BY ").Append(PartitionBy);
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
