using System.Text;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 标准递归 CTE 序列化子句，对齐上游 WithSearchClause。
/// 语法：SEARCH {BREADTH | DEPTH} FIRST BY cols SET seqcol
/// 用于递归 CTE 中指定遍历顺序和序列列（SQL:1999 标准）。
/// </summary>
public class WithSearchClause
{
    /// <summary>
    /// 搜索顺序：BREADTH（广度优先）或 DEPTH（深度优先）。
    /// </summary>
    public SearchOrder SearchOrder { get; set; }

    /// <summary>
    /// 搜索列（SEARCH ... FIRST BY col1, col2 中的列列表）。
    /// </summary>
    public List<string> SearchColumns { get; set; } = new();

    /// <summary>
    /// 序列列名（SET seqcol 生成的排序序列列名）。
    /// </summary>
    public string? SequenceColumnName { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("SEARCH ");
        sb.Append(SearchOrder.ToString()).Append(" FIRST BY ");
        sb.Append(string.Join(", ", SearchColumns));
        sb.Append(" SET ").Append(SequenceColumnName);
        return sb.ToString();
    }
}

/// <summary>
/// 递归 CTE 搜索顺序。
/// </summary>
public enum SearchOrder
{
    /// <summary>广度优先（BREADTH FIRST）。</summary>
    BREADTH,

    /// <summary>深度优先（DEPTH FIRST）。</summary>
    DEPTH
}
