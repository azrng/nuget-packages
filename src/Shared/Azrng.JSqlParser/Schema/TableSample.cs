namespace Azrng.JSqlParser.Schema;

/// <summary>
/// TABLESAMPLE 子句（FROM 子句采样），对齐上游 SampleClause。
/// 形式：<c>TABLESAMPLE (expression) [BERNOULLI|SYSTEM]</c> 或 <c>TABLESAMPLE BERNOULLI (n)</c>。
/// </summary>
public class TableSample
{
    /// <summary>采样方法（BERNOULLI / SYSTEM），未指定为空。</summary>
    public string? SamplingMethod { get; set; }

    /// <summary>采样比例/行数表达式。</summary>
    public Expression.Expression SampleSize { get; set; } = null!;

    /// <summary>是否带 PERCENT。</summary>
    public bool Percentage { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("TABLESAMPLE ");
        if (!string.IsNullOrEmpty(SamplingMethod)) sb.Append(SamplingMethod).Append(' ');
        sb.Append('(').Append(SampleSize).Append(')');
        if (Percentage) sb.Append(" PERCENT");
        return sb.ToString();
    }
}
