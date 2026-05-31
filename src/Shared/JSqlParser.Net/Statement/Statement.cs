namespace JSqlParser.Net.Statement;

/// <summary>
/// Base interface for all SQL statements in the AST.
/// </summary>
public interface Statement : Model
{
    T Accept<T, S>(StatementVisitor<T> visitor, S context);

    void Accept<T>(StatementVisitor<T> visitor) => Accept<T, object?>(visitor, default);
}
