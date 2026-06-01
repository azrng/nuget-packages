using System.Text;
using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement.Select;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Exasol Skyline PREFERRING clause.
/// Used in SELECT, UPDATE, DELETE statements.
/// </summary>
public class PreferringClause : ASTNodeAccessImpl
{
    public Expression Preferring { get; set; } = null!;
    public ExpressionList? PartitionBy { get; set; }

    public PreferringClause() { }

    public PreferringClause(Expression preferring)
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
