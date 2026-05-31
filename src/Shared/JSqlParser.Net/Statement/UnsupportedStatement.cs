using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement;

public class UnsupportedStatement : ASTNodeAccessImpl, Statement
{
    public string Statement { get; set; } = "";

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Statement;
}
