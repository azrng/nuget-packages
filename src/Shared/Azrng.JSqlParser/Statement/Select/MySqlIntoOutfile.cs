using System.Text;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 表示 MySQL SELECT ... INTO OUTFILE/DUMPFILE 子句。
/// 移植自上游 JSqlParser commit c0e1d052 的 MySqlSelectIntoClause（含 FIELDS/LINES 格式化子句）。
/// </summary>
public class MySqlIntoOutfile
{
    public enum OutfileType
    {
        OUTFILE,
        DUMPFILE
    }

    public enum FieldsKeyword
    {
        FIELDS,
        COLUMNS
    }

    /// <summary>输出类型（OUTFILE 或 DUMPFILE）。</summary>
    public OutfileType Type { get; set; }

    /// <summary>目标文件路径（字符串字面量，含引号）。</summary>
    public string? FileName { get; set; }

    /// <summary>是否位于 FROM 之前（true=前置，false=尾部）。</summary>
    public bool BeforeFrom { get; set; }

    /// <summary>CHARACTER SET 字符集名（可选，仅 OUTFILE）。</summary>
    public string? CharacterSet { get; set; }

    /// <summary>FIELDS/COLUMNS 关键字选择（可选，仅 OUTFILE）。</summary>
    public FieldsKeyword? FieldsKeywordValue { get; set; }

    public string? FieldsTerminatedBy { get; set; }

    public bool FieldsOptionallyEnclosed { get; set; }

    public string? FieldsEnclosedBy { get; set; }

    public string? FieldsEscapedBy { get; set; }

    public string? LinesStartingBy { get; set; }

    public string? LinesTerminatedBy { get; set; }

    public bool HasFieldsClause =>
        FieldsKeywordValue != null || FieldsTerminatedBy != null
        || FieldsEnclosedBy != null || FieldsEscapedBy != null;

    public bool HasLinesClause =>
        LinesStartingBy != null || LinesTerminatedBy != null;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("INTO ");
        sb.Append(Type == OutfileType.DUMPFILE ? "DUMPFILE" : "OUTFILE");
        sb.Append(' ').Append(FileName);

        // DUMPFILE 不支持格式化子句（与上游一致）
        if (Type == OutfileType.DUMPFILE)
        {
            return sb.ToString();
        }

        if (!string.IsNullOrEmpty(CharacterSet))
        {
            sb.Append(" CHARACTER SET ").Append(CharacterSet);
        }

        if (HasFieldsClause)
        {
            AppendFieldsClause(sb);
        }

        if (HasLinesClause)
        {
            sb.Append(" LINES");
            if (LinesStartingBy != null)
            {
                sb.Append(" STARTING BY ").Append(LinesStartingBy);
            }
            if (LinesTerminatedBy != null)
            {
                sb.Append(" TERMINATED BY ").Append(LinesTerminatedBy);
            }
        }

        return sb.ToString();
    }

    private void AppendFieldsClause(StringBuilder sb)
    {
        sb.Append(' ').Append(FieldsKeywordValue == FieldsKeyword.COLUMNS ? "COLUMNS" : "FIELDS");
        if (FieldsTerminatedBy != null)
        {
            sb.Append(" TERMINATED BY ").Append(FieldsTerminatedBy);
        }
        if (FieldsEnclosedBy != null)
        {
            if (FieldsOptionallyEnclosed)
            {
                sb.Append(" OPTIONALLY");
            }
            sb.Append(" ENCLOSED BY ").Append(FieldsEnclosedBy);
        }
        if (FieldsEscapedBy != null)
        {
            sb.Append(" ESCAPED BY ").Append(FieldsEscapedBy);
        }
    }
}
