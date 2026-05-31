using System.Text;
using JSqlParser.Net.Statement.Select;

namespace JSqlParser.Net.Statement.Piped;

public class JoinPipeOperator : PipeOperator
{
    public Join Join { get; set; } = null!;

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
