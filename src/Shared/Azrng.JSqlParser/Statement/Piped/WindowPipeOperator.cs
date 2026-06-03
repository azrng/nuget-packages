using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Piped;

public class WindowPipeOperator : PipeOperator
{
    public string WindowName { get; set; } = "";
    public Expression.Expression WindowExpression { get; set; } = null!;

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> WINDOW ");
        builder.Append(WindowName);
        builder.Append(" AS (");
        builder.Append(WindowExpression);
        builder.Append(')');
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
