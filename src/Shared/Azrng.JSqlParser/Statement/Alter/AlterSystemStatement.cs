using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// ALTER SYSTEM 语句（Oracle），对齐上游 AlterSystemStatement。
/// 形式：<c>ALTER SYSTEM SET param = value</c> / <c>ALTER SYSTEM CHECKPOINT</c> 等。
/// </summary>
public class AlterSystemStatement : ASTNodeAccessImpl, Statement
{
    /// <summary>操作类型字符串（如 SET、CHECKPOINT、SWITCH LOGFILE）。</summary>
    public string Operation { get; set; } = "";

    /// <summary>参数列表。</summary>
    public List<string> Parameters { get; } = new();

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder($"ALTER SYSTEM {Operation}");
        if (Parameters.Count > 0) sb.Append(' ').Append(string.Join(' ', Parameters));
        return sb.ToString();
    }
}
