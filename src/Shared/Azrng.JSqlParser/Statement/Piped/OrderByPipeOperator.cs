using System.Text;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class OrderByPipeOperator : PipeOperator
{
    public List<OrderByElement> OrderByElements { get; set; } = new();

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> ORDER BY ");

        for (int i = 0; i < OrderByElements.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(OrderByElements[i]);
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
