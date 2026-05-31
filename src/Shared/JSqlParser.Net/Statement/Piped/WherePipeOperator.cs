using System.Text;
using JSqlParser.Net.Expression;

namespace JSqlParser.Net.Statement.Piped;

public class WherePipeOperator : PipeOperator
{
    public Expression.Expression Expression { get; set; } = null!;

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> WHERE ");
        builder.Append(Expression);
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
