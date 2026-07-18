using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Piped;

public class ExtendPipeOperator : PipeOperator
{
    public required Expression.IExpression Expression { get; set; }
    public Alias? Alias { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> EXTEND ");
        builder.Append(Expression);
        if (Alias != null)
        {
            builder.Append(" AS ");
            builder.Append(Alias.Name);
        }
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
