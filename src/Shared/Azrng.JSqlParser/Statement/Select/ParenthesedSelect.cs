using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a parenthesized SELECT (subquery).
/// </summary>
public class ParenthesedSelect : ASTNodeAccessImpl, Expression.IExpression, IFromItem
{
    public Select Select { get; set; } = null!;
    public Alias? Alias { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => Select.Accept(visitor, context);
    [Obsolete("改用 " + nameof(Alias) + " 属性")]
    public Alias? GetAlias() => Alias;
    [Obsolete("改用 " + nameof(Alias) + " 属性")]
    public void SetAlias(Alias alias) { Alias = alias; }

    public PlainSelect GetPlainSelect()
    {
        if (Select is PlainSelect plainSelect) return plainSelect;
        // L8 修复：非 PlainSelect（如 SetOperationList/Values）时抛明确异常，而非裸 InvalidCastException
        throw new JSqlParserException(
            $"Subquery is not a PlainSelect but {Select?.GetType().Name ?? "null"}.");
    }

    public override string ToString()
    {
        var sql = $"({Select})";
        return Alias != null ? $"{sql} {Alias}" : sql;
    }
}
