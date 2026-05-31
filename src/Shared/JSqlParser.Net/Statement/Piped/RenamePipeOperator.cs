using System.Text;

namespace JSqlParser.Net.Statement.Piped;

public class RenamePipeOperator : PipeOperator
{
    public Dictionary<string, string> Renames { get; set; } = new();

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> RENAME ");
        int i = 0;
        foreach (var kvp in Renames)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(kvp.Key).Append(" AS ").Append(kvp.Value);
            i++;
        }
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
