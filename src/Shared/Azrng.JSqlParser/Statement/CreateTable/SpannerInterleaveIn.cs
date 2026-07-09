using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Spanner INTERLEAVE IN PARENT 子句，对齐上游 <c>net.sf.jsqlparser.expression.SpannerInterleaveIn</c>。
/// 表示 <c>, INTERLEAVE IN PARENT table [ON DELETE CASCADE|NO ACTION]</c>。
/// </summary>
public class SpannerInterleaveIn
{
    /// <summary>父表名。</summary>
    public Table? Table { get; set; }

    /// <summary>ON DELETE 引用动作。未指定时为 null。</summary>
    public ReferentialAction? OnDelete { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder($", INTERLEAVE IN PARENT {Table}");
        if (OnDelete != null) sb.Append(' ').Append(OnDelete);
        return sb.ToString();
    }
}
