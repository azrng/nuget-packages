using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class RollbackStatement : ASTNodeAccessImpl, IStatement
{
    public string? Savepoint { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Savepoint != null ? $"ROLLBACK TO {Savepoint}" : "ROLLBACK";
}
