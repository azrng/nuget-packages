using System.Text;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class JoinPipeOperator : PipeOperator
{
    public required Join Join { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> ");
        builder.Append(Join);
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
