using JSqlParser.Net.Expression;
using JSqlParser.Net.Expression.Operators.Arithmetic;

namespace JSqlParser.Net.Expression.Operators.Relational;

public class RegExpMatchOperator : BinaryExpression
{
    public string Operator { get; set; } = "REGEXP";
    public bool Not { get; set; }

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string GetStringExpression() => Not ? $"NOT {Operator}" : Operator;
}
