namespace Azrng.JSqlParser.Expression;

/// <summary>
/// INTERVAL 限定符结构化模型（#1728）：<c>unit [(p)] [TO toUnit [(toP)]]</c>，如
/// <c>DAY</c>、<c>DAY TO SECOND(6)</c>、<c>HOUR(2) TO MINUTE</c>。
/// </summary>
public class IntervalQualifier
{
    /// <summary>起始字段名（YEAR/MONTH/DAY/HOUR/MINUTE/SECOND）。</summary>
    public string Unit { get; set; } = "";

    /// <summary>起始字段精度（unit(p)），未指定时为 null。</summary>
    public int? Precision { get; set; }

    /// <summary>TO 结束字段名（DAY TO SECOND 的 SECOND），未指定时为 null。</summary>
    public string? ToUnit { get; set; }

    /// <summary>结束字段精度（SECOND(6)），未指定时为 null。</summary>
    public int? ToPrecision { get; set; }

    public override string ToString()
    {
        var unit = Precision != null ? $"{Unit}({Precision})" : Unit;
        if (ToUnit == null) return unit;
        var toUnit = ToPrecision != null ? $"{ToUnit}({ToPrecision})" : ToUnit;
        return $"{unit} TO {toUnit}";
    }
}
