using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Models;
using JExpression = Azrng.JSqlParser.Expression.Expression;
using Parenthesis = Azrng.JSqlParser.Expression.Parenthesis;

namespace Azrng.JSqlParser.Util;

/// <summary>
/// WHERE 条件结构化提取引擎：把 AND/OR 树拍平为中性 <see cref="WhereCondition"/> 列表。
/// </summary>
/// <remarks>
/// 不对外公开。<b>通用化设计</b>（方案 B）：
/// <list type="bullet">
/// <item>逻辑连接符（And/Or/Parenthesis）按结构递归——结构性判断，永远正确。</item>
/// <item>所有二元运算符（继承 <see cref="BinaryExpression"/>，含 =、&gt;、LIKE、!=、加减乘除等）统一提取，
/// 新增二元运算符自动覆盖，无需改库。</item>
/// <item><see cref="InExpression"/>/<see cref="Between"/> 结构特殊（非二元或需拆解），单独处理。</item>
/// <item><b>未匹配的叶子（IS NULL/EXISTS/单目运算符）兜底提取</b>为单目条件，Operator 取类型名，
/// 不再静默丢弃。</item>
/// </list>
/// 不含列归属反查、参数收集装配到业务 DTO 的逻辑——业务方按字段自行映射。
/// </remarks>
internal static class WhereConditionsExtractor
{
    /// <summary>提取 WHERE 表达式的拍平条件列表。</summary>
    public static IReadOnlyList<WhereCondition> Extract(JExpression? where)
    {
        var result = new List<WhereCondition>();
        Collect(where, result, linkType: string.Empty);
        return result;
    }

    private static void Collect(JExpression? expression, List<WhereCondition> result, string linkType)
    {
        if (expression == null) return;

        switch (expression)
        {
            // 逻辑连接符：按结构递归（左子沿用 linkType，右子带连接符）
            case AndExpression and:
                Collect(and.LeftExpression, result, linkType);
                Collect(and.RightExpression, result, "AND");
                break;

            case OrExpression or:
                Collect(or.LeftExpression, result, linkType);
                Collect(or.RightExpression, result, "OR");
                break;

            case Parenthesis parenthesis:
                // 递归穿透括号：括号只是分组，内部条件仍应被提取
                Collect(parenthesis.Expression, result, linkType);
                break;

            // 特殊结构：In（非二元，左右独立字段）/ Between（拆成 [start, end] 两条件）
            case InExpression inExpr:
                result.Add(new WhereCondition
                {
                    LinkType = linkType,
                    LeftExpression = inExpr.LeftExpression,
                    RightExpression = inExpr.RightExpression,
                    Operator = inExpr.Not ? "NOT IN" : "IN",
                    SqlInfo = inExpr.ToString() ?? string.Empty
                });
                break;

            case Between between:
                result.Add(new WhereCondition
                {
                    LinkType = linkType,
                    LeftExpression = between.LeftExpression,
                    RightExpression = between.BetweenExpressionStart,
                    Operator = between.Not ? "NOT BETWEEN" : "BETWEEN",
                    SqlInfo = between.ToString() ?? string.Empty
                });
                result.Add(new WhereCondition
                {
                    LinkType = "AND",
                    LeftExpression = between.LeftExpression,
                    RightExpression = between.BetweenExpressionEnd,
                    Operator = between.Not ? "NOT BETWEEN" : "BETWEEN",
                    SqlInfo = between.ToString() ?? string.Empty
                });
                break;

            // 所有二元运算符统一兜底（=、>、<、LIKE、!=、加减乘除、位运算等继承 BinaryExpression 的类型）
            case BinaryExpression binary:
                result.Add(new WhereCondition
                {
                    LinkType = linkType,
                    LeftExpression = binary.LeftExpression,
                    RightExpression = binary.RightExpression,
                    Operator = binary.OperatorSymbol,
                    SqlInfo = binary.ToString() ?? string.Empty
                });
                break;

            // 兜底：未识别的叶子（IS NULL/EXISTS/单目运算符等）也提取，不静默丢弃。
            // Operator 取类型名，RightExpression 为 null（单目语义）。
            default:
                result.Add(new WhereCondition
                {
                    LinkType = linkType,
                    LeftExpression = expression,
                    RightExpression = null,
                    Operator = expression.GetType().Name,
                    SqlInfo = expression.ToString() ?? string.Empty
                });
                break;
        }
    }
}
