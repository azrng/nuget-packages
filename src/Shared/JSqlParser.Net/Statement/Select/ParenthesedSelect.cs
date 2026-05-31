using JSqlParser.Net.Expression;
using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Represents a parenthesized SELECT (subquery).
/// </summary>
public class ParenthesedSelect : ASTNodeAccessImpl, Expression.Expression, FromItem
{
    public Select Select { get; set; } = null!;
    public Alias? Alias { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => Select.Accept(visitor, context);
    public Alias? GetAlias() => Alias;
    public void SetAlias(Alias alias) { Alias = alias; }

    public PlainSelect GetPlainSelect()
    {
        if (Select is PlainSelect plainSelect) return plainSelect;
        return (PlainSelect)Select;
    }

    public override string ToString()
    {
        var sql = $"({Select})";
        return Alias != null ? $"{sql} {Alias}" : sql;
    }
}
