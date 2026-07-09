using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Represents a column definition in CREATE TABLE.
/// </summary>
/// <remarks>
/// <see cref="ColDataType"/> 为结构化类型对象（对齐上游 <c>ColDataType</c>），
/// <see cref="ColumnSpecs"/> 透传原始字符串（NOT NULL / DEFAULT / AUTO_INCREMENT / MATERIALIZED 等），与上游 round-trip 一致。
/// </remarks>
public class ColumnDefinition : ASTNodeAccessImpl
{
    public string ColumnName { get; set; } = "";

    /// <summary>列数据类型。破坏性变更：由 string 改为结构化 <see cref="ColDataType"/>，对齐上游。</summary>
    public ColDataType ColDataType { get; set; } = new();

    /// <summary>列规格原始字符串列表（NOT NULL / DEFAULT expr / AUTO_INCREMENT / COMMENT '...' / MATERIALIZED 等）。</summary>
    public System.Collections.Generic.List<string> ColumnSpecs { get; set; } = new();

    public override string ToString()
    {
        var specs = ColumnSpecs.Count > 0 ? " " + string.Join(" ", ColumnSpecs) : "";
        return $"{ColumnName} {ColDataType}{specs}";
    }
}
