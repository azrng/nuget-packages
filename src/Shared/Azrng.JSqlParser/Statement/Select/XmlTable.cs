using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// PostgreSQL <c>XMLTABLE</c> 行集表函数：把 XML 文档按 XPath 展开为行集。
/// <para>
/// 形式：<c>XMLTABLE('//ROWS/ROW' PASSING data COLUMNS id int PATH '@id', n FOR ORDINALITY, ...)</c>。
/// </para>
/// </summary>
public class XmlTable : ASTNodeAccessImpl, IFromItem
{
    /// <summary>行 XPath 查询串（如 <c>'//ROWS/ROW'</c>）。</summary>
    public string? RowPath { get; set; }

    /// <summary>
    /// XMLNAMESPACES(...) 声明原始文本（如 <c>XMLNAMESPACES('http://x' AS x, DEFAULT 'http://d')</c>），
    /// 未指定时为 null。原样存取保 round-trip。
    /// </summary>
    public string? XmlNamespaces { get; set; }

    /// <summary>PASSING 子句传入的表达式列表（如 <c>data</c>）。</summary>
    public List<IExpression> Passing { get; } = new();

    /// <summary>列定义列表。</summary>
    public List<XmlTableColumn> Columns { get; } = new();

    /// <summary>FROM 子句别名（可选）。</summary>
    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("XMLTABLE(");
        if (XmlNamespaces != null) sb.Append(XmlNamespaces).Append(", ");
        sb.Append(RowPath);
        if (Passing.Count > 0)
            sb.Append(" PASSING ").Append(string.Join(", ", Passing));
        if (Columns.Count > 0)
        {
            sb.Append(" COLUMNS (");
            for (int i = 0; i < Columns.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Columns[i]);
            }
            sb.Append(')');
        }
        sb.Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}

/// <summary>XMLTABLE 列定义。</summary>
public class XmlTableColumn
{
    /// <summary>列名。</summary>
    public string Name { get; set; } = "";

    /// <summary>是否为 FOR ORDINALITY 序号列。</summary>
    public bool ForOrdinality { get; set; }

    /// <summary>列数据类型（如 int / text / float），FOR ORDINALITY 时为 null。</summary>
    public string? DataType { get; set; }

    /// <summary>列 XPath（PATH '...'），未指定时为 null。</summary>
    public string? Path { get; set; }

    /// <summary>DEFAULT 表达式，未指定时为 null。</summary>
    public IExpression? DefaultExpression { get; set; }

    public override string ToString()
    {
        if (ForOrdinality) return $"{Name} FOR ORDINALITY";
        var sb = new StringBuilder(Name);
        if (!string.IsNullOrEmpty(DataType)) sb.Append(' ').Append(DataType);
        if (Path != null) sb.Append(" PATH ").Append(Path);
        if (DefaultExpression != null) sb.Append(" DEFAULT ").Append(DefaultExpression);
        return sb.ToString();
    }
}
