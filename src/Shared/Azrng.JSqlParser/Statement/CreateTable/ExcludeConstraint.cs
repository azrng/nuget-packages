namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// EXCLUDE 约束（PostgreSQL），对齐上游 <c>net.sf.jsqlparser.statement.create.table.ExcludeConstraint</c>。
/// 表示 <c>EXCLUDE WHERE (expr)</c>。
/// </summary>
public class ExcludeConstraint : Constraint
{
    /// <summary>EXCLUDE WHERE 表达式。</summary>
    public Expression.IExpression? Expression { get; set; }

    public override string ToString()
    {
        return $"EXCLUDE WHERE ({Expression})";
    }
}
