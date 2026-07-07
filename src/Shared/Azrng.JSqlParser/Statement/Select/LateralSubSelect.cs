using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// LATERAL 子查询（FROM 子句中的 <c>LATERAL (SELECT ...)</c>），对齐上游 LateralSubSelect。
///
/// 继承自 <see cref="ParenthesedSelect"/>，增加 <see cref="Prefix"/> 字段（默认 "LATERAL"）。
/// 上游支持 APPLY 形式时 prefix 可能是 "CROSS APPLY"/"OUTER APPLY"。
/// </summary>
public class LateralSubSelect : ParenthesedSelect
{
    /// <summary>LATERAL 前缀（或 CROSS APPLY / OUTER APPLY），默认 "LATERAL"。</summary>
    public string Prefix { get; set; } = "LATERAL";

    public LateralSubSelect() { }

    public LateralSubSelect(string prefix) => Prefix = prefix;

    public override string ToString() => Prefix + " " + base.ToString();
}
