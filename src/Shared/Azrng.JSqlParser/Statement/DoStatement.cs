using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// DO 语句（#2589 PostgreSQL 匿名代码块，如 <c>DO $$ BEGIN ... END $$;</c>，
/// 也覆盖 MySQL DO expr 形式）。DO 之后整体透传保 round-trip。
/// </summary>
public class DoStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>DO 之后的原文（LANGUAGE 前缀、代码块字面量等），透传。Language/Code 未填时使用。</summary>
    public string? Text { get; set; }

    /// <summary>LANGUAGE 语言名（如 plpgsql），未指定时为 null。</summary>
    public string? Language { get; set; }

    /// <summary>代码块字面量原文（$$...$$ 或 '...'）。结构化路径填充。</summary>
    public string? Code { get; set; }

    /// <summary>LANGUAGE 位于代码块之前（PG 支持 DO LANGUAGE plpgsql $$...$$）。</summary>
    public bool LanguageBeforeCode { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("DO");
        if (Code != null)
        {
            if (Language != null && LanguageBeforeCode)
                sb.Append(" LANGUAGE ").Append(Language);
            sb.Append(' ').Append(Code);
            if (Language != null && !LanguageBeforeCode)
                sb.Append(" LANGUAGE ").Append(Language);
            return sb.ToString();
        }
        if (!string.IsNullOrEmpty(Text)) sb.Append(' ').Append(Text);
        return sb.ToString();
    }
}
