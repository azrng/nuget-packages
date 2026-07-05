using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 括号化的 INSERT（用于 CTE 形式 <c>WITH x AS (INSERT ... RETURNING ...)</c>）。
/// 继承自 <see cref="Insert.Insert"/>，使其可被任何消费 Insert 的代码使用，
/// 并额外保留 CTE 别名（如 <c>WITH x AS (INSERT ...) SELECT * FROM x</c> 中的 x）。
/// 与上游 ParenthesedInsert 对齐。
/// </summary>
public class ParenthesedInsert : Insert.Insert
{
    /// <summary>CTE 上下文中的别名（AS alias）。</summary>
    public Alias? Alias { get; set; }

    /// <summary>括号内包裹的 INSERT 语句。</summary>
    public Insert.Insert? Insert { get; set; }

    // 覆盖 Accept 走 StatementVisitor.Visit(ParenthesedInsert)，
    // 否则会因继承关系错误命中 Visit(Insert)。
    public new T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append('(').Append(Insert).Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}
