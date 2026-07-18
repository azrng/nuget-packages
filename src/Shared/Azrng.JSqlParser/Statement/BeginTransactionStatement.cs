using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// BEGIN [WORK|TRANSACTION] 事务开始语句（PostgreSQL/MySQL）。
/// 上游不支持，Azrng 增强。裸 BEGIN 走 blockStatement（PL/SQL 块）。
/// </summary>
public class BeginTransactionStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>是否使用 TRANSACTION 关键字（true=TRANSACTION，false=WORK）。</summary>
    public bool UseTransactionKeyword { get; set; } = true;

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => UseTransactionKeyword ? "BEGIN TRANSACTION" : "BEGIN WORK";
}
