using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class FromQuery : Select.Select
{
    public required IFromItem IFromItem { get; set; }
    public bool UsingFromKeyword { get; set; } = true;
    public List<PipeOperator> PipeOperators { get; set; } = new();
    public List<Join>? Joins { get; set; }

    public FromQuery() { }

    [SetsRequiredMembers]
    public FromQuery(IFromItem fromItem)
    {
        IFromItem = fromItem;
    }

    [SetsRequiredMembers]
    public FromQuery(IFromItem fromItem, bool usingFromKeyword)
    {
        IFromItem = fromItem;
        UsingFromKeyword = usingFromKeyword;
    }

    public FromQuery Add(PipeOperator op)
    {
        PipeOperators.Add(op);
        return this;
    }

    public override T Accept<T, S>(ISelectVisitor<T> selectVisitor, S context)
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

        builder.Append(IFromItem);

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
