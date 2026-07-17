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
/// 不对外公开。对应 LocalSqlParser.CollectOperators 的纯 AST 遍历部分，
/// 仅覆盖 And/Or/Binary/In/Between 五类运算符（与原逻辑一致）。
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
            case AndExpression and:
                // 左子沿用当前 linkType（首个条件可能为空），右子带 AND
                Collect(and.LeftExpression, result, linkType);
                Collect(and.RightExpression, result, "AND");
                break;

            case OrExpression or:
                Collect(or.LeftExpression, result, linkType);
                Collect(or.RightExpression, result, "OR");
                break;

            case BinaryExpression binary:
                result.Add(new WhereCondition
                {
                    LinkType = linkType,
                    LeftExpression = binary.LeftExpression,
                    RightExpression = binary.RightExpression,
                    Operator = binary.GetStringExpression(),
                    SqlInfo = binary.ToString() ?? string.Empty
                });
                break;

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
                // BETWEEN 拆成两个条件：[start, end]，对齐 LocalSqlParser line 183-188
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

            case Parenthesis parenthesis:
                // 递归穿透括号：括号只是分组，内部条件仍应被提取（比 LocalSqlParser 原逻辑更完整）
                Collect(parenthesis.Expression, result, linkType);
                break;
        }
    }
}
