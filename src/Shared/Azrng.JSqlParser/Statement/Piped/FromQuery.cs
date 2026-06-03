using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class FromQuery : Select.Select
{
    public FromItem FromItem { get; set; } = null!;
    public bool UsingFromKeyword { get; set; } = true;
    public List<PipeOperator> PipeOperators { get; set; } = new();
    public List<Join>? Joins { get; set; }

    public FromQuery() { }

    public FromQuery(FromItem fromItem)
    {
        FromItem = fromItem;
    }

    public FromQuery(FromItem fromItem, bool usingFromKeyword)
    {
        FromItem = fromItem;
        UsingFromKeyword = usingFromKeyword;
    }

    public FromQuery Add(PipeOperator op)
    {
        PipeOperators.Add(op);
        return this;
    }

    public override T Accept<T, S>(SelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public T Accept<T, S>(FromQueryVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        if (UsingFromKeyword)
            builder.Append("FROM ");

        builder.Append(FromItem);

        if (Joins != null)
        {
            foreach (var join in Joins)
            {
                builder.Append(' ').Append(join);
            }
        }

        foreach (var op in PipeOperators)
        {
            builder.Append(' ');
            op.AppendTo(builder);
        }

        return builder;
    }
}
