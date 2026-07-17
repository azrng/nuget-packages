using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class ExplainStatement : ASTNodeAccessImpl, Statement
{
    public Statement? Statement { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"EXPLAIN {Statement}";
}
