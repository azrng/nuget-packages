using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// Represents a list of SQL statements.
/// </summary>
public class Statements : ASTNodeAccessImpl, Statement
{
    public System.Collections.Generic.List<Statement> StatementList { get; set; } = new();

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => string.Join(";\n", StatementList);
}
