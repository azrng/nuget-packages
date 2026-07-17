using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class UnsupportedStatement : ASTNodeAccessImpl, Statement
{
    public string Statement { get; set; } = "";

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Statement;
}
