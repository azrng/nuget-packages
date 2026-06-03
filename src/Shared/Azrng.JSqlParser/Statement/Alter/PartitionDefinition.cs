using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// MySQL ALTER TABLE ADD PARTITION definition.
/// Example: ADD PARTITION (PARTITION p2024 VALUES LESS THAN (2025))
/// </summary>
public class PartitionDefinition
{
    public string? Name { get; set; }
    public ExpressionList? ValuesLessThan { get; set; }
    public ExpressionList? ValuesIn { get; set; }

    public PartitionDefinition() { }

    public StringBuilder AppendTo(StringBuilder builder)
    {
        if (Name != null)
            builder.Append(Name);
        if (ValuesLessThan != null)
            builder.Append(" VALUES LESS THAN (").Append(ValuesLessThan).Append(')');
        if (ValuesIn != null)
            builder.Append(" VALUES IN (").Append(ValuesIn).Append(')');
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
