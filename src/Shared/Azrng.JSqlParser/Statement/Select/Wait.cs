namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 表示 Oracle 风格的 WAIT n 锁等待超时子句。
/// 移植自 JSqlParser 5.4 Wait 类。
/// </summary>
public class Wait
{
    /// <summary>等待超时秒数。</summary>
    public long Timeout { get; set; }

    /// <summary>返回 " WAIT {Timeout}"。</summary>
    public override string ToString() => " WAIT " + Timeout;

    public Wait WithTimeout(long timeout)
    {
        Timeout = timeout;
        return this;
    }
}
