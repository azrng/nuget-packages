using System.Text;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Piped;

public class AsPipeOperator : PipeOperator
{
    public required Alias Alias { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> AS ");
        builder.Append(Alias.Name);
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
