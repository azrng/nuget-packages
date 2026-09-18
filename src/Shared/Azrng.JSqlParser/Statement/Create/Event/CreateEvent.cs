using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Create.Event;

/// <summary>
/// MySQL CREATE EVENT [IF NOT EXISTS] e ON SCHEDULE ... [ON COMPLETION [NOT] PRESERVE]
/// [ENABLE|DISABLE [ON SLAVE]] [COMMENT '...'] DO stmt（#2547）。
/// 调度子句原文透传，body 结构化。
/// </summary>
public class CreateEvent : ASTNodeAccessImpl, IStatement
{
    public bool IfNotExists { get; set; }

    /// <summary>事件名。</summary>
    public Table? Event { get; set; }

    /// <summary>ON SCHEDULE 子句原文（AT ... / EVERY n unit [STARTS...] [ENDS...]），透传。</summary>
    public string? ScheduleText { get; set; }

    /// <summary>ON COMPLETION PRESERVE 标志：true=PRESERVE，false=NOT PRESERVE，null=未指定。</summary>
    public bool? OnCompletionPreserve { get; set; }

    /// <summary>事件状态：true=ENABLE，false=DISABLE，null=未指定。</summary>
    public bool? Enabled { get; set; }

    /// <summary>DISABLE ON SLAVE 标志（仅 Enabled == false 时有意义）。</summary>
    public bool DisableOnSlave { get; set; }

    /// <summary>COMMENT 原文（含引号），未指定时为 null。</summary>
    public string? Comment { get; set; }

    /// <summary>DO 之后的执行体语句。</summary>
    public IStatement? Body { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE EVENT ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Event);
        if (ScheduleText != null) sb.Append(" ON SCHEDULE ").Append(ScheduleText);
        if (OnCompletionPreserve != null)
            sb.Append(" ON COMPLETION ").Append(OnCompletionPreserve == false ? "NOT " : "").Append("PRESERVE");
        if (Enabled != null)
        {
            sb.Append(Enabled == true ? " ENABLE" : " DISABLE");
            if (DisableOnSlave) sb.Append(" ON SLAVE");
        }
        if (Comment != null) sb.Append(" COMMENT ").Append(Comment);
        if (Body != null) sb.Append(" DO ").Append(Body);
        return sb.ToString();
    }
}
