using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

public class ExpressionList : ASTNodeAccessImpl, Expression
{
    public List<Expression> Expressions { get; set; } = new();

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => string.Join(", ", Expressions);
}
