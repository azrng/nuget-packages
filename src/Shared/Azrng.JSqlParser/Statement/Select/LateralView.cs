using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Hive/Spark LATERAL VIEW 子句，对齐上游 LateralView。
/// 语法：LATERAL VIEW [OUTER] generator_function() [tableAlias] AS colAlias [, colAlias]*
/// 与 <see cref="LateralSubSelect"/>（LATERAL (subquery)）不同，LATERAL VIEW 接的是表生成函数（如 explode()）。
/// </summary>
public class LateralView : ASTNodeAccessImpl, IFromItem
{
    /// <summary>是否 LATERAL VIEW OUTER（保留无匹配行）。</summary>
    public bool UsingOuter { get; set; }

    /// <summary>表生成函数（如 explode(arr)、posexplode(arr)）的原始文本，透传保 round-trip。</summary>
    public string? GeneratorFunction { get; set; }

    /// <summary>表别名。</summary>
    public Alias? TableAlias { get; set; }

    /// <summary>列别名（Hive 允许多列 AS c1, c2）。</summary>
    public List<string>? ColumnAliases { get; set; }

    /// <summary>IFromItem 接口的别名（通常 LateralView 不使用，别名在 TableAlias/ColumnAliases）。</summary>
    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("LATERAL VIEW");
        if (UsingOuter) sb.Append(" OUTER");
        // GeneratorFunction 含完整剩余文本（function() [tblAlias] AS col1, col2），透传保 round-trip
        if (!string.IsNullOrEmpty(GeneratorFunction)) sb.Append(' ').Append(GeneratorFunction);
        return sb.ToString();
    }
}
