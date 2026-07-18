using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents an item in a SELECT list (expression with optional alias).
/// </summary>
public class SelectItem : ASTNodeAccessImpl
{
    public Expression.IExpression Expression { get; set; } = null!;
    public Alias? Alias { get; set; }

    public SelectItem() { }

    public SelectItem(Expression.IExpression expression, Alias? alias = null)
    {
        Expression = expression;
        Alias = alias;
    }

    public override string ToString()
    {
        if (Alias != null)
            return $"{Expression} {Alias}";
        return Expression.ToString() ?? string.Empty;
    }
}
