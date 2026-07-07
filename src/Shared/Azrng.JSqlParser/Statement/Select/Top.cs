using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// SELECT TOP n 量词，对齐上游 Top.java。
/// 支持 <c>TOP n</c>、<c>TOP (expr)</c>、<c>TOP n PERCENT</c>、<c>TOP n WITH TIES</c> 组合。
/// </summary>
public class Top : ASTNodeAccessImpl
{
    /// <summary>是否使用括号包裹表达式 <c>TOP (n)</c>。</summary>
    public bool HasParenthesis { get; set; }

    /// <summary>是否带 PERCENT。</summary>
    public bool IsPercentage { get; set; }

    /// <summary>是否带 WITH TIES。</summary>
    public bool IsWithTies { get; set; }

    /// <summary>行数表达式（通常为 LongValue，括号形式下可为任意表达式）。</summary>
    public Expression.Expression Expression { get; set; } = null!;

    public override string ToString()
    {
        var result = "TOP ";
        if (HasParenthesis) result += "(";
        result += Expression;
        if (HasParenthesis) result += ")";
        if (IsPercentage) result += " PERCENT";
        if (IsWithTies) result += " WITH TIES";
        return result;
    }
}
