namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// 外键引用动作，对齐上游 <c>net.sf.jsqlparser.statement.ReferentialAction</c>。
/// 表示 FOREIGN KEY 的 <c>ON DELETE</c>/<c>ON UPDATE</c> 子句的动作。
/// </summary>
public class ReferentialAction
{
    /// <summary>引用动作触发的时机（DELETE 或 UPDATE）。</summary>
    public ReferentialActionType Type { get; set; }

    /// <summary>引用动作的具体行为。</summary>
    public ReferentialActionMode Action { get; set; }

    public override string ToString()
    {
        var actionStr = Action switch
        {
            ReferentialActionMode.Cascade => "CASCADE",
            ReferentialActionMode.Restrict => "RESTRICT",
            ReferentialActionMode.NoAction => "NO ACTION",
            ReferentialActionMode.SetNull => "SET NULL",
            ReferentialActionMode.SetDefault => "SET DEFAULT",
            _ => Action.ToString().ToUpperInvariant(),
        };
        var typeStr = Type == ReferentialActionType.Delete ? "DELETE" : "UPDATE";
        return $"ON {typeStr} {actionStr}";
    }
}

/// <summary>引用动作触发时机。</summary>
public enum ReferentialActionType
{
    /// <summary>ON DELETE</summary>
    Delete,

    /// <summary>ON UPDATE</summary>
    Update,
}

/// <summary>引用动作行为。</summary>
public enum ReferentialActionMode
{
    /// <summary>CASCADE</summary>
    Cascade,

    /// <summary>RESTRICT</summary>
    Restrict,

    /// <summary>NO ACTION</summary>
    NoAction,

    /// <summary>SET NULL</summary>
    SetNull,

    /// <summary>SET DEFAULT</summary>
    SetDefault,
}
