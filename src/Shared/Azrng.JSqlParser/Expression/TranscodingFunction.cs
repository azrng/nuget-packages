using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 字符集转码/类型转换函数 CONVERT，支持两种风格：
/// <list type="bullet">
/// <item>转码风格（MySQL）：<c>CONVERT(expr USING transcodingName)</c></item>
/// <item>类型转换风格（SQL Server）：<c>CONVERT(dataType, expr[, style])</c>，关键字可为 TRY_CONVERT / SAFE_CONVERT</item>
/// </list>
/// 与上游 TranscodingFunction 对齐。注：Azrng 用 string 表达 DataType（与 CastExpression 一致）。
/// </summary>
public class TranscodingFunction : ASTNodeAccessImpl, Expression
{
    public string Keyword { get; set; } = "CONVERT";

    /// <summary>true 表示转码风格 (expr USING name)；false 表示类型转换风格 (dataType, expr[, style])。</summary>
    public bool IsTranscodeStyle { get; set; } = true;

    /// <summary>类型转换风格下的目标数据类型（与 CastExpression.DataType 一致用 string 表达）。</summary>
    public string? ColDataType { get; set; }

    public Expression? Expression { get; set; }

    /// <summary>转码风格下为字符集名；类型转换风格下为可选的 SQL Server style 整数（字符串形式）。</summary>
    public string? TranscodingName { get; set; }

    public TranscodingFunction() { }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (IsTranscodeStyle)
        {
            return $"{Keyword}( {Expression} USING {TranscodingName} )";
        }

        var style = !string.IsNullOrEmpty(TranscodingName) ? $", {TranscodingName}" : "";
        return $"{Keyword}( {ColDataType}, {Expression}{style} )";
    }
}
