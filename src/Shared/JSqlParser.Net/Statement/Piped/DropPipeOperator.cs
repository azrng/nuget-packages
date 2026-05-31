using System.Text;

namespace JSqlParser.Net.Statement.Piped;

public class DropPipeOperator : PipeOperator
{
    public List<string> ColumnNames { get; set; } = new();

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> DROP ");
        builder.Append(string.Join(", ", ColumnNames));
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
