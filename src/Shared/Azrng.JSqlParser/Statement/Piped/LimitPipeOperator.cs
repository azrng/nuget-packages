using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Piped;

public class LimitPipeOperator : PipeOperator
{
    public Expression.IExpression? Expression { get; set; }
    public Expression.IExpression? Offset { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> LIMIT ");
        if (Expression != null)
            builder.Append(Expression);

        if (Offset != null)
            builder.Append(" OFFSET ").Append(Offset);

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
