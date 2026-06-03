using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents (function()).* — e.g., (COUNT(*).*)
/// </summary>
public class FunctionAllColumns : AllColumns
{
    public Function Function { get; set; } = null!;

    public FunctionAllColumns() { }

    public FunctionAllColumns(Function function)
    {
        Function = function;
    }

    public new T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append('(').Append(Function).Append(").*");
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
