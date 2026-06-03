using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Base interface for all SQL expressions in the AST.
/// </summary>
public interface Expression : ASTNodeAccess, Model
{
    T Accept<T, S>(ExpressionVisitor<T> visitor, S context);

    void Accept<T>(ExpressionVisitor<T> visitor) => Accept<T, object?>(visitor, default);
}
