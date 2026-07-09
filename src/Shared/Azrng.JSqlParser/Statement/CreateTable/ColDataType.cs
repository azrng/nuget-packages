using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// 列数据类型，对齐上游 <c>net.sf.jsqlparser.statement.create.table.ColDataType</c>。
/// 结构化表达列类型名、参数列表、字符集与数组维度，取代此前 <see cref="ColumnDefinition"/> 中 DataType 为纯字符串的设计。
/// </summary>
/// <remarks>
/// 参数列表 <see cref="ArgumentsStringList"/> 透传原始字符串（如 <c>VARCHAR(255)</c> → ["255"]，<c>set('a','b')</c> → ["'a'","'b'"]），
/// 与上游 round-trip 行为一致。
/// </remarks>
public class ColDataType : ASTNodeAccessImpl
{
    /// <summary>类型名，可含点号（schema.type），如 <c>INT</c>、<c>my_schema.MY_TYPE</c>。</summary>
    public string DataType { get; set; } = "";

    /// <summary>括号内参数原始字符串列表，如 <c>DECIMAL(10,2)</c> → ["10","2"]。未指定时为 null。</summary>
    public System.Collections.Generic.List<string>? ArgumentsStringList { get; set; }

    /// <summary>数组维度列表，如 <c>text[]</c> → [null]，<c>int[3][4]</c> → [3,4]；null 元素表示无尺寸 <c>[]</c>。</summary>
    public System.Collections.Generic.List<int?>? ArrayData { get; set; }

    /// <summary>CHARACTER SET 指定的字符集名，如 <c>CHARACTER SET utf8</c>。未指定时为 null。</summary>
    public string? CharacterSet { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(DataType);
        if (ArgumentsStringList is { Count: > 0 })
        {
            sb.Append(" (").Append(string.Join(", ", ArgumentsStringList)).Append(')');
        }
        if (ArrayData is { Count: > 0 })
        {
            foreach (var dim in ArrayData)
            {
                sb.Append(" [");
                if (dim.HasValue) sb.Append(dim.Value);
                sb.Append(']');
            }
        }
        if (!string.IsNullOrEmpty(CharacterSet))
        {
            sb.Append(" CHARACTER SET ").Append(CharacterSet);
        }
        return sb.ToString();
    }
}
