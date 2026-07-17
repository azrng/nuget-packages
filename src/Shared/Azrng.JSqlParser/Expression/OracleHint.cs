using System.Text.RegularExpressions;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Oracle 优化器提示（Optimizer Hint）。
/// <para>
/// 两种形式：
/// <list type="bullet">
/// <item>多行：<c>/*+ INDEX(t idx) */</c></item>
/// <item>单行：<c>--+ INDEX(t idx)</c></item>
/// </list>
/// </para>
/// 出现位置：SELECT/INSERT/UPDATE/DELETE/MERGE 关键字之后。
/// 与上游 OracleHint 对齐。
/// </summary>
public class OracleHint : ASTNodeAccessImpl, Expression
{
    private static readonly Regex SingleLinePattern = new(@"--\+ *(.+)", RegexOptions.Compiled);
    private static readonly Regex MultiLinePattern = new(@"/\*\+ *(.*?) *\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>提示内容（去掉外层包装后的纯文本）。</summary>
    public string? Value { get; set; }

    /// <summary>true 表示单行形式 <c>--+ ...</c>；false 表示多行形式 <c>/*+ ... */</c>。</summary>
    public bool SingleLine { get; set; }

    public OracleHint() { }

    /// <summary>从原始注释字符串构造并解析提示内容。</summary>
    public OracleHint(string comment)
    {
        var m = SingleLinePattern.Match(comment);
        if (m.Success)
        {
            Value = m.Groups[1].Value.Trim();
            SingleLine = true;
            return;
        }
        m = MultiLinePattern.Match(comment);
        if (m.Success)
        {
            Value = m.Groups[1].Value.Trim();
            SingleLine = false;
        }
    }

    /// <summary>判断注释字符串是否为 Oracle Hint。</summary>
    public static bool IsHintMatch(string comment)
        => SingleLinePattern.IsMatch(comment) || MultiLinePattern.IsMatch(comment);

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
        => SingleLine ? $"--+ {Value}\n" : $"/*+ {Value} */";
}
