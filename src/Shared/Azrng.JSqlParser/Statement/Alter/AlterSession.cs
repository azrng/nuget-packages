using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// ALTER SESSION 语句（Oracle），对齐上游 AlterSession。
/// 形式：<c>ALTER SESSION SET param = value</c> / <c>ALTER SESSION CLOSE DATABASE LINK link</c> 等。
/// </summary>
public class AlterSession : ASTNodeAccessImpl, Statement
{
    /// <summary>操作类型字符串（如 SET、CLOSE）。</summary>
    public string Operation { get; set; } = "";

    /// <summary>参数列表。</summary>
    public List<string> Parameters { get; } = new();

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder($"ALTER SESSION {Operation}");
        if (Parameters.Count > 0) sb.Append(' ').Append(string.Join(' ', Parameters));
        return sb.ToString();
    }
}
