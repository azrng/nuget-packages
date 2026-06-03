using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a SQL function call.
/// </summary>
public class Function : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public Expression? Parameters { get; set; }
    public bool AllColumns { get; set; }
    public List<OrderByElement>? WithinGroupOrderByElements { get; set; }
    public Expression? FilterExpression { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var value = AllColumns ? $"{Name}(*)" : $"{Name}({Parameters})";

        if (WithinGroupOrderByElements != null && WithinGroupOrderByElements.Count > 0)
            value += $" WITHIN GROUP (ORDER BY {string.Join(", ", WithinGroupOrderByElements)})";

        if (FilterExpression != null)
            value += $" FILTER (WHERE {FilterExpression})";

        return value;
    }
}
