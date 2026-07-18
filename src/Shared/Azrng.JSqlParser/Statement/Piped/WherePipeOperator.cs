using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Piped;

public class WherePipeOperator : PipeOperator
{
    public required Expression.IExpression Expression { get; set; }

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
