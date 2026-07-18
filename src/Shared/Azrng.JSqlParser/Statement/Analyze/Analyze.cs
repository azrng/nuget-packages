using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Analyze;

/// <summary>
/// ANALYZE 语句，对齐上游 Analyze。
/// 形式：<c>ANALYZE &lt;table&gt;</c>（收集表统计信息）。
/// </summary>
public class Analyze : ASTNodeAccessImpl, IStatement
{
    public Table Table { get; set; } = null!;

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"ANALYZE {Table}";
}
