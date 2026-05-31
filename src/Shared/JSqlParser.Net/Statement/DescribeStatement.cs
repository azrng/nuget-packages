using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement;

public class DescribeStatement : ASTNodeAccessImpl, Statement
{
    public string Name { get; set; } = "";

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"DESCRIBE {Name}";
}
