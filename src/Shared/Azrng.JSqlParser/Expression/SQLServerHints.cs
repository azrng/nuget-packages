using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// SQL Server 表提示，形式 <c>WITH (INDEX(idx), NOLOCK)</c>，
/// 出现在 FROM 子句的表之后。与上游 SQLServerHints 对齐。
/// </summary>
public class SQLServerHints : ASTNodeAccessImpl, Model
{
    /// <summary>NOLOCK 提示标志，未指定时为 null。</summary>
    public bool? NoLock { get; set; }

    /// <summary>INDEX (name) 提示，未指定时为 null。</summary>
    public string? IndexName { get; set; }

    public SQLServerHints() { }

    public SQLServerHints WithNoLock(bool noLock = true)
    {
        NoLock = noLock;
        return this;
    }

    public SQLServerHints WithIndexName(string indexName)
    {
        IndexName = indexName;
        return this;
    }

    public override string ToString()
    {
        var hints = new List<string>();
        if (IndexName != null) hints.Add($"INDEX ({IndexName})");
        if (NoLock == true) hints.Add("NOLOCK");
        return $" WITH ({string.Join(", ", hints)})";
    }
}
