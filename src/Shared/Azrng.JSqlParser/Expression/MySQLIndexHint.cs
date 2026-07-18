using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// MySQL 索引提示：USE/IGNORE/FORCE INDEX/KEY (idx1, idx2, ...)，
/// 出现在 FROM 子句的表之后。与上游 MySQLIndexHint 对齐。
/// <para>
/// 示例：<c>SELECT * FROM t USE INDEX (idx1, idx2)</c>、
/// <c>SELECT * FROM t FORCE KEY (pk)</c>。
/// </para>
/// </summary>
public class MySQLIndexHint : ASTNodeAccessImpl, IModel
{
    /// <summary>动作：USE / IGNORE / FORCE。</summary>
    public string Action { get; set; } = "";

    /// <summary>索引限定符：INDEX / KEY。</summary>
    public string IndexQualifier { get; set; } = "";

    /// <summary>索引名列表。</summary>
    public List<string> IndexNames { get; set; } = new();

    /// <summary>
    /// MySQL 限定符：FOR JOIN / FOR ORDER BY / FOR GROUP BY。未指定时为 null。
    /// 对齐上游 MySQLIndexHint.forClause（join/order/group）。
    /// </summary>
    public string? ForClause { get; set; }

    public MySQLIndexHint() { }

    public MySQLIndexHint(string action, string indexQualifier, List<string> indexNames)
    {
        Action = action;
        IndexQualifier = indexQualifier;
        IndexNames = indexNames;
    }

    public override string ToString()
    {
        // use|ignore|force key|index [FOR JOIN|ORDER BY|GROUP BY] (index1,...,indexN)
        var forSegment = string.IsNullOrEmpty(ForClause) ? "" : $" {ForClause}";
        return $" {Action} {IndexQualifier}{forSegment} ({string.Join(",", IndexNames)})";
    }
}
