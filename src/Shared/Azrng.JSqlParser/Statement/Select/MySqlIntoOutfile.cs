namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 表示 MySQL SELECT ... INTO OUTFILE/DUMPFILE 子句。
/// 移植自上游 JSqlParser commit c0e1d052 的 MySqlSelectIntoClause（简化版，不含 FIELDS/LINES 格式化子句）。
/// </summary>
public class MySqlIntoOutfile
{
    public enum OutfileType
    {
        OUTFILE,
        DUMPFILE
    }

    /// <summary>输出类型（OUTFILE 或 DUMPFILE）。</summary>
    public OutfileType Type { get; set; }

    /// <summary>目标文件路径（字符串字面量，含引号）。</summary>
    public string? FileName { get; set; }

    /// <summary>是否位于 FROM 之前（true=前置，false=尾部）。</summary>
    public bool BeforeFrom { get; set; }

    public override string ToString()
    {
        var type = Type == OutfileType.DUMPFILE ? "DUMPFILE" : "OUTFILE";
        return $"INTO {type} {FileName}";
    }
}
