using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// RESET 语句，对齐上游 ResetStatement。
/// 形式：<c>RESET name</c>，如 <c>RESET TimeZone</c>、<c>RESET ALL</c>。
/// </summary>
public class ResetStatement : ASTNodeAccessImpl, Statement
{
    public string Name { get; set; } = "";

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"RESET {Name}";
}
