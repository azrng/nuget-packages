using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 标准递归 CTE 环检测子句，对齐上游 WithCycleClause（#2566）。
/// 语法：CYCLE col1, col2 SET markcol [TO mark DEFAULT nomark] USING pathcol（USING 必填，TO/DEFAULT 成对可选）。
/// 用于递归 CTE 中检测循环并产出环标记列与路径列。
/// </summary>
public class WithCycleClause
{
    /// <summary>参与环检测的列名列表（CYCLE col1, col2）。</summary>
    public List<string> CycleColumns { get; set; } = new();

    /// <summary>环标记列名（SET markcol）。</summary>
    public string MarkColumnName { get; set; } = "";

    /// <summary>检测到环时的标记值（TO mark），未指定时为 null。</summary>
    public IExpression? MarkValue { get; set; }

    /// <summary>未检测到环时的默认标记值（DEFAULT nomark），未指定时为 null。</summary>
    public IExpression? MarkDefault { get; set; }

    /// <summary>路径数组列名（USING pathcol）。</summary>
    public string PathColumnName { get; set; } = "";

    public override string ToString()
    {
        var sb = new StringBuilder("CYCLE ");
        sb.Append(string.Join(", ", CycleColumns));
        sb.Append(" SET ").Append(MarkColumnName);
        if (MarkValue != null) sb.Append(" TO ").Append(MarkValue);
        if (MarkDefault != null) sb.Append(" DEFAULT ").Append(MarkDefault);
        sb.Append(" USING ").Append(PathColumnName);
        return sb.ToString();
    }
}
