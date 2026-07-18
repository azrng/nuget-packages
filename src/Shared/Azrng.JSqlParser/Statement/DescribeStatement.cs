using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class DescribeStatement : ASTNodeAccessImpl, IStatement
{
    public string Name { get; set; } = "";

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"DESCRIBE {Name}";
}
