using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

public class IsBooleanExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public bool Not { get; set; }
    public bool IsTrue { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() =>
        $"{LeftExpression} {(Not ? "IS NOT" : "IS")} {(IsTrue ? "TRUE" : "FALSE")}";
}
