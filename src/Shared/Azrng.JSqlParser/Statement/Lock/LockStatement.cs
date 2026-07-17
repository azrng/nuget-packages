using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Lock;

/// <summary>
/// 表示 LOCK TABLE 语句。
/// <para>示例：<c>LOCK TABLE t IN EXCLUSIVE MODE</c>、<c>LOCK TABLE t IN SHARE MODE NOWAIT</c>。</para>
/// 移植自上游 JSqlParser commit 6697c063 的 LockStatement 类。
/// </summary>
public class LockStatement : ASTNodeAccessImpl, Statement
{
    private bool _noWait;
    private long? _waitSeconds;

    public Table? Table { get; set; }
    public LockMode LockMode { get; set; }

    /// <summary>是否指定 NOWAIT（立即失败而非等待锁释放）。设置时校验与 WAIT 互斥。</summary>
    public bool NoWait
    {
        get => _noWait;
        set
        {
            _noWait = value;
            CheckValidState();
        }
    }

    /// <summary>WAIT 超时秒数，为 null 表示未指定 WAIT 子句。设置时校验与 NOWAIT 互斥。</summary>
    public long? WaitSeconds
    {
        get => _waitSeconds;
        set
        {
            _waitSeconds = value;
            CheckValidState();
        }
    }

    public LockStatement() { }

    public LockStatement(Table? table, LockMode lockMode)
    {
        Table = table;
        LockMode = lockMode;
    }

    /// <summary>校验 NOWAIT 与 WAIT 不能同时指定。</summary>
    private void CheckValidState()
    {
        if (_noWait && _waitSeconds != null)
        {
            throw new InvalidOperationException("A LOCK statement cannot have NOWAIT and WAIT at the same time");
        }
    }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        return "LOCK TABLE "
            + Table?.GetFullyQualifiedName()
            + " IN "
            + LockMode.GetValue()
            + " MODE"
            + (NoWait ? " NOWAIT" : "")
            + (WaitSeconds != null ? " WAIT " + WaitSeconds : "");
    }
}
