using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

public class ExpressionList : ASTNodeAccessImpl, Expression
{
    public List<Expression> Expressions { get; set; } = new();

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => string.Join(", ", Expressions);
}
