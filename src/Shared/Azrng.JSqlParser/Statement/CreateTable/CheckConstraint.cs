namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// CHECK 约束，对齐上游 <c>net.sf.jsqlparser.statement.create.table.CheckConstraint</c>。
/// 在 <see cref="Constraint"/> 基础上持有 CHECK 表达式。
/// </summary>
public class CheckConstraint : Constraint
{
    /// <summary>CHECK 约束表达式。</summary>
    public Expression.Expression? Expression { get; set; }

    public override string ToString()
    {
        var prefix = Name != null ? $"CONSTRAINT {Name} " : "";
        return $"{prefix}CHECK ({Expression})";
    }
}
