using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class CommitStatement : ASTNodeAccessImpl, Statement
{
    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => "COMMIT";
}
