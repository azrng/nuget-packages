using System.Text;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class SetPipeOperator : PipeOperator
{
    public List<SelectItem> SetItems { get; set; } = new();

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> SET ");
        for (int i = 0; i < SetItems.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(SetItems[i]);
        }
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
