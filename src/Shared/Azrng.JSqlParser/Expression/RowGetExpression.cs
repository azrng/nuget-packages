using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 表示 PostgreSQL 复合类型的字段访问，如 <c>(expr).field</c> 或 <c>(expr::type).field1.field2</c>。
/// 移植自上游 JSqlParser 的 RowGetExpression。
/// </summary>
public class RowGetExpression : ASTNodeAccessImpl, Expression
{
    public Expression? Expression { get; set; }
    public string ColumnName { get; set; } = "";

    public RowGetExpression() { }

    public RowGetExpression(Expression expression, string columnName)
    {
        Expression = expression;
        ColumnName = columnName;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{Expression}.{ColumnName}";
}
