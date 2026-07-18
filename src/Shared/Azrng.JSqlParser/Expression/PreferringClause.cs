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
    public IExpression Preferring { get; set; } = null!;
    public ExpressionList? PartitionBy { get; set; }

    public PreferringClause() { }

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
