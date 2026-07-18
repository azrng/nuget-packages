using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class IsBooleanExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression LeftExpression { get; set; }
    public bool Not { get; set; }
    public bool IsTrue { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() =>
        $"{LeftExpression} {(Not ? "IS NOT" : "IS")} {(IsTrue ? "TRUE" : "FALSE")}";
}
