namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Oracle ROW MOVEMENT 选项，对齐上游 <c>net.sf.jsqlparser.statement.create.table.RowMovement</c>。
/// 表示 <c>ENABLE ROW MOVEMENT</c> 或 <c>DISABLE ROW MOVEMENT</c>。
/// </summary>
public class RowMovement
{
    /// <summary>ROW MOVEMENT 模式（ENABLE 或 DISABLE）。</summary>
    public RowMovementMode Mode { get; set; }

    public override string ToString()
    {
        var mode = Mode == RowMovementMode.Enable ? "ENABLE" : "DISABLE";
        return $"{mode} ROW MOVEMENT";
    }
}

/// <summary>ROW MOVEMENT 模式，对齐上游 RowMovementMode。</summary>
public enum RowMovementMode
{
    /// <summary>ENABLE ROW MOVEMENT</summary>
    Enable,

    /// <summary>DISABLE ROW MOVEMENT</summary>
    Disable,
}
