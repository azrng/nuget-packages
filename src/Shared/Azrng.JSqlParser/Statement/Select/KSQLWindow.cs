using System.Text;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// ksqlDB 流式窗口子句，对齐上游 KSQLWindow。
/// 语法形式：
/// <list type="bullet">
/// <item>HOPPING (SIZE n unit, ADVANCE BY n unit)</item>
/// <item>TUMBLING (SIZE n unit)</item>
/// <item>SESSION (n unit)</item>
/// </list>
/// 三种窗口类型互斥。
/// </summary>
public class KSQLWindow
{
    /// <summary>是否 HOPPING 窗口。</summary>
    public bool Hopping { get; set; }

    /// <summary>是否 TUMBLING 窗口。</summary>
    public bool Tumbling { get; set; }

    /// <summary>是否 SESSION 窗口。</summary>
    public bool Session { get; set; }

    /// <summary>窗口大小（数值部分）。</summary>
    public long SizeDuration { get; set; }

    /// <summary>窗口大小时间单位。</summary>
    public KSQLTimeUnit SizeTimeUnit { get; set; }

    /// <summary>HOPPING 窗口前进间隔（仅 HOPPING 使用）。</summary>
    public long AdvanceDuration { get; set; }

    /// <summary>HOPPING 窗口前进间隔时间单位。</summary>
    public KSQLTimeUnit AdvanceTimeUnit { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Hopping)
        {
            sb.Append("HOPPING (SIZE ").Append(SizeDuration).Append(' ').Append(SizeTimeUnit)
              .Append(", ADVANCE BY ").Append(AdvanceDuration).Append(' ').Append(AdvanceTimeUnit).Append(')');
        }
        else if (Session)
        {
            sb.Append("SESSION (").Append(SizeDuration).Append(' ').Append(SizeTimeUnit).Append(')');
        }
        else
        {
            sb.Append("TUMBLING (SIZE ").Append(SizeDuration).Append(' ').Append(SizeTimeUnit).Append(')');
        }
        return sb.ToString();
    }
}

/// <summary>
/// ksqlDB 流式 JOIN 的 WITHIN 窗口，对齐上游 KSQLJoinWindow。
/// 语法形式：WITHIN (n unit) 单值 或 WITHIN (n unit, n unit) before/after 双值。
/// </summary>
public class KSQLJoinWindow
{
    /// <summary>是否 before/after 双值窗口。</summary>
    public bool BeforeAfter { get; set; }

    /// <summary>单值窗口时长。</summary>
    public long Duration { get; set; }

    /// <summary>单值窗口时间单位。</summary>
    public KSQLTimeUnit TimeUnit { get; set; }

    /// <summary>双值窗口 before 时长。</summary>
    public long BeforeDuration { get; set; }

    /// <summary>双值窗口 before 时间单位。</summary>
    public KSQLTimeUnit BeforeTimeUnit { get; set; }

    /// <summary>双值窗口 after 时长。</summary>
    public long AfterDuration { get; set; }

    /// <summary>双值窗口 after 时间单位。</summary>
    public KSQLTimeUnit AfterTimeUnit { get; set; }

    public override string ToString()
    {
        if (BeforeAfter)
        {
            return $"({BeforeDuration} {BeforeTimeUnit}, {AfterDuration} {AfterTimeUnit})";
        }
        return $"({Duration} {TimeUnit})";
    }
}

/// <summary>
/// ksqlDB 时间单位（含单数和复数形式）。
/// </summary>
public enum KSQLTimeUnit
{
    DAY,
    HOUR,
    MINUTE,
    SECOND,
    MILLISECOND,
    DAYS,
    HOURS,
    MINUTES,
    SECONDS,
    MILLISECONDS
}
