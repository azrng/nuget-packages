using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement;

public class UseStatement : ASTNodeAccessImpl, Statement
{
    public string Name { get; set; } = "";

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"USE {Name}";
}
