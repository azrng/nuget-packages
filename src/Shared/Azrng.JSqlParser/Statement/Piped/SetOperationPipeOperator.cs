using System.Text;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class SetOperationPipeOperator : PipeOperator
{
    public SetOperation.OperationType OperationType { get; set; }
    public bool All { get; set; }

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> ");
        builder.Append(OperationType.ToString());
        if (All)
            builder.Append(" ALL");
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
