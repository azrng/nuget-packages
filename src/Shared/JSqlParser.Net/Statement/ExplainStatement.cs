using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement;

public class ExplainStatement : ASTNodeAccessImpl, Statement
{
    public Statement? Statement { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"EXPLAIN {Statement}";
}
