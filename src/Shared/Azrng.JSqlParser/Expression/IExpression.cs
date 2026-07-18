using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Base interface for all SQL expressions in the AST.
/// </summary>
public interface IExpression : IASTNodeAccess, IModel
{
    T Accept<T, S>(IExpressionVisitor<T> visitor, S context);

    void Accept<T>(IExpressionVisitor<T> visitor) => Accept<T, object?>(visitor, default);
}
