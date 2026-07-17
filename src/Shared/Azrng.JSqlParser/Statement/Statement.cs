namespace Azrng.JSqlParser.Statement;

/// <summary>
/// Base interface for all SQL statements in the AST.
/// </summary>
public interface Statement : IModel
{
    T Accept<T, S>(IStatementVisitor<T> visitor, S context);

    void Accept<T>(IStatementVisitor<T> visitor) => Accept<T, object?>(visitor, default);
}
