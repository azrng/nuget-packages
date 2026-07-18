using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

public class ExpressionList : ASTNodeAccessImpl, IExpression
{
    public List<IExpression> Expressions { get; set; } = new();

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => string.Join(", ", Expressions);
}
