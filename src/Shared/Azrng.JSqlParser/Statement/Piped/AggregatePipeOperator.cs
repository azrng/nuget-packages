using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class AggregatePipeOperator : PipeOperator
{
    public List<SelectItem> SelectItems { get; set; } = new();
    public Expression.Expression? Having { get; set; }
    public List<Expression.Expression>? GroupBy { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> AGGREGATE ");

        for (int i = 0; i < SelectItems.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(SelectItems[i]);
        }

        if (GroupBy != null && GroupBy.Count > 0)
        {
            builder.Append(" GROUP BY ");
            for (int i = 0; i < GroupBy.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(GroupBy[i]);
            }
        }

        if (Having != null)
        {
            builder.Append(" HAVING ");
            builder.Append(Having);
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
