using System.Text;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class SelectPipeOperator : PipeOperator
{
    public string OperatorName { get; set; } = "SELECT";
    public string? Modifier { get; set; }
    public List<SelectItem> SelectItems { get; set; } = new();

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> SELECT ");
        if (Modifier != null)
            builder.Append(Modifier).Append(' ');

        for (int i = 0; i < SelectItems.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(SelectItems[i]);
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
