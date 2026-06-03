using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Piped;

public class CallPipeOperator : PipeOperator
{
    public string FunctionName { get; set; } = "";
    public ExpressionList? Parameters { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> CALL ");
        builder.Append(FunctionName);
        if (Parameters != null)
        {
            builder.Append('(');
            builder.Append(Parameters);
            builder.Append(')');
        }
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
