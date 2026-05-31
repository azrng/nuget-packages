using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

public class FullTextSearch : ASTNodeAccessImpl, Expression
{
    public System.Collections.Generic.List<string> Columns { get; set; } = new();
    public Expression MatchExpression { get; set; } = null!;
    public string? Filter { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"MATCH({string.Join(", ", Columns)}) AGAINST({MatchExpression})";
}
