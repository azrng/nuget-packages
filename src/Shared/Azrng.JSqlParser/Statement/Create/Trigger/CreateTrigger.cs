using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Create.Trigger;

/// <summary>
/// MySQL CREATE [DEFINER = user@host] TRIGGER name (BEFORE|AFTER) (INSERT|UPDATE|DELETE)
/// ON table FOR EACH ROW [FOLLOWS|PRECEDES other] body（#2548）。
/// </summary>
public class CreateTrigger : ASTNodeAccessImpl, IStatement
{
    /// <summary>DEFINER = 账户原文（含 @host），未指定时为 null。</summary>
    public string? Definer { get; set; }

    /// <summary>触发器名。</summary>
    public Table? Trigger { get; set; }

    public TriggerTiming Timing { get; set; }

    public TriggerEvent Event { get; set; }

    /// <summary>触发目标表。</summary>
    public Table? Table { get; set; }

    /// <summary>FOLLOWS/PRECEDES 顺序与参照触发器，未指定时为 null。</summary>
    public TriggerOrder? Order { get; set; }

    /// <summary>触发体语句（BEGIN...END 块结构化；单语句场景为 null）。</summary>
    public IStatement? Body { get; set; }

    /// <summary>触发体原文透传（单语句场景，如 INSERT INTO log VALUES (1)），Body 为 null 时使用。</summary>
    public string? BodyText { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE ");
        if (Definer != null) sb.Append("DEFINER = ").Append(Definer).Append(' ');
        sb.Append("TRIGGER ").Append(Trigger);
        sb.Append(' ').Append(Timing.ToString().ToUpperInvariant());
        sb.Append(' ').Append(Event.ToString().ToUpperInvariant());
        sb.Append(" ON ").Append(Table);
        sb.Append(" FOR EACH ROW");
        if (Order != null) sb.Append(' ').Append(Order);
        if (Body != null) sb.Append(' ').Append(Body);
        else if (BodyText != null) sb.Append(' ').Append(BodyText);
        return sb.ToString();
    }
}

/// <summary>触发时机。</summary>
public enum TriggerTiming
{
    Before,
    After
}

/// <summary>触发事件。</summary>
public enum TriggerEvent
{
    Insert,
    Update,
    Delete
}

/// <summary>FOLLOWS/PRECEDES 触发器顺序（MySQL 5.7+）。</summary>
public class TriggerOrder
{
    public bool Follows { get; set; }

    /// <summary>参照的既有触发器。</summary>
    public Table? OtherTrigger { get; set; }

    public override string ToString()
        => $"{(Follows ? "FOLLOWS" : "PRECEDES")} {OtherTrigger}";
}
