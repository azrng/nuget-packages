using System.Text;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 表示 SELECT 语句中的 FOR UPDATE / FOR SHARE 锁定子句。
/// 移植自 JSqlParser commit 2b141568 的 ForUpdateClause 类。
/// <para>
/// 支持常见 SQL 方言：
/// <list type="bullet">
/// <item><c>FOR UPDATE</c> — 标准行锁</item>
/// <item><c>FOR UPDATE OF t1, t2</c> — 表级锁定（Oracle、PostgreSQL）</item>
/// <item><c>FOR UPDATE NOWAIT</c> — 行被锁立即失败（Oracle、PostgreSQL）</item>
/// <item><c>FOR UPDATE WAIT n</c> — 等待 n 秒（Oracle）</item>
/// <item><c>FOR UPDATE SKIP LOCKED</c> — 跳过被锁行（Oracle、PostgreSQL）</item>
/// <item><c>FOR SHARE</c> — 共享行锁（PostgreSQL）</item>
/// <item><c>FOR KEY SHARE</c> — 键级共享锁（PostgreSQL）</item>
/// <item><c>FOR NO KEY UPDATE</c> — 非键排他锁（PostgreSQL）</item>
/// </list>
/// </para>
/// </summary>
public class ForUpdateClause
{
    /// <summary>锁模式（UPDATE/SHARE/NO_KEY_UPDATE/KEY_SHARE）。</summary>
    public ForMode? Mode { get; set; }

    /// <summary>OF 子句中的表列表，未指定时为 null。</summary>
    public List<Table>? Tables { get; set; }

    /// <summary>WAIT n 等待超时，未指定时为 null。</summary>
    public Wait? Wait { get; set; }

    /// <summary>是否指定 NOWAIT。</summary>
    public bool NoWait { get; set; }

    /// <summary>是否指定 SKIP LOCKED。</summary>
    public bool SkipLocked { get; set; }

    /// <summary>返回 OF 子句中的第一个表，未指定时为 null。</summary>
    public Table? FirstTable => (Tables != null && Tables.Count > 0) ? Tables[0] : null;

    /// <summary>当 Mode 为 UPDATE 时返回 true。</summary>
    public bool IsForUpdate() => Mode == ForMode.Update;

    /// <summary>当 Mode 为 SHARE 时返回 true。</summary>
    public bool IsForShare() => Mode == ForMode.Share;

    /// <summary>当 OF 子句至少列出一个表时返回 true。</summary>
    public bool HasTableList() => Tables != null && Tables.Count > 0;

    /// <summary>将本子句追加到 builder，并返回 builder。</summary>
    public StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append(" FOR ").Append(Mode?.GetValue());
        if (Tables != null && Tables.Count > 0)
        {
            builder.Append(" OF ");
            for (int i = 0; i < Tables.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(Tables[i]);
            }
        }
        if (Wait != null)
        {
            builder.Append(Wait);
        }
        if (NoWait)
        {
            builder.Append(" NOWAIT");
        }
        else if (SkipLocked)
        {
            builder.Append(" SKIP LOCKED");
        }
        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
