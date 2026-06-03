using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Cnf;

/// <summary>
/// Represents a conjunction (AND) of multiple expressions.
/// Used by CNFConverter to represent the result of CNF conversion.
/// </summary>
public class MultiAndExpression : ASTNodeAccessImpl, Expression
{
    public List<Expression> Expressions { get; set; } = new();

    public MultiAndExpression() { }

    public MultiAndExpression(params Expression[] expressions)
    {
        Expressions.AddRange(expressions);
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => string.Join(" AND ", Expressions.Select(e => $"({e})"));
}
