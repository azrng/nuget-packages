using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class ShowStatement : ASTNodeAccessImpl, Statement
{
    public string Name { get; set; } = "";

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"SHOW {Name}";
}
