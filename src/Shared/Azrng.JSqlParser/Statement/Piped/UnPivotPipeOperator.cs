using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Piped;

public class UnPivotPipeOperator : PipeOperator
{
    public string? FunctionName { get; set; }
    public List<Expression.IExpression> InExpressions { get; set; } = new();
    public List<SelectItem> SelectItems { get; set; } = new();

    public override T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context)
    {
        return visitor.Visit(this, context);
    }

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append("|> UNPIVOT ");
        if (FunctionName != null)
            builder.Append(FunctionName).Append('(');

        for (int i = 0; i < SelectItems.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(SelectItems[i]);
        }

        if (FunctionName != null)
            builder.Append(')');

        if (InExpressions.Count > 0)
        {
            builder.Append(" IN (");
            for (int i = 0; i < InExpressions.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(InExpressions[i]);
            }
            builder.Append(')');
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
