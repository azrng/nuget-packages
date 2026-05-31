using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement;

public class SetStatement : ASTNodeAccessImpl, Statement
{
    public string Name { get; set; } = "";
    public Expression.Expression? Value { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Value != null ? $"SET {Name} = {Value}" : $"SET {Name}";
}
