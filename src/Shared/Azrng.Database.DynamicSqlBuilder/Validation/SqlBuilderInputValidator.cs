using System.Text.RegularExpressions;

namespace Azrng.Database.DynamicSqlBuilder.Validation;

internal static class SqlBuilderInputValidator
{
    private static readonly string[] SuspiciousSqlPatterns =
    {
        "--",
        "/*",
        "*/",
        ";",
        "'",
        "\""
    };

    private static readonly Regex NecessaryConditionPrefixPattern =
        new(@"^(WHERE|AND|OR)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static string NormalizeLogicalOperator(string logicalOperator)
    {
        if (string.IsNullOrWhiteSpace(logicalOperator))
        {
            return "AND";
        }

        var normalized = logicalOperator.Trim().ToUpperInvariant();
        return normalized switch
        {
            "AND" => "AND",
            "OR" => "OR",
            _ => throw new ArgumentException($"不支持的逻辑操作符: {logicalOperator}", nameof(logicalOperator))
        };
    }

    internal static string NormalizeNecessaryCondition(string necessaryCondition)
    {
        if (string.IsNullOrWhiteSpace(necessaryCondition))
        {
            return " where 1=1 ";
        }

        var normalized = necessaryCondition.Trim();
        if (!NecessaryConditionPrefixPattern.IsMatch(normalized))
        {
            throw new ArgumentException("necessaryCondition 必须以 WHERE、AND 或 OR 开头", nameof(necessaryCondition));
        }

        if (ContainsSuspiciousSqlPattern(normalized))
        {
            throw new ArgumentException("necessaryCondition 包含不安全的 SQL 片段", nameof(necessaryCondition));
        }

        if (!HasBalancedParentheses(normalized))
        {
            throw new ArgumentException("necessaryCondition 括号不匹配", nameof(necessaryCondition));
        }

        return $" {normalized} ";
    }

    private static bool ContainsSuspiciousSqlPattern(string input)
    {
        return SuspiciousSqlPatterns.Any(pattern =>
            input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasBalancedParentheses(string input)
    {
        var depth = 0;
        foreach (var ch in input)
        {
            if (ch == '(')
            {
                depth++;
            }
            else if (ch == ')')
            {
                depth--;
                if (depth < 0)
                {
                    return false;
                }
            }
        }

        return depth == 0;
    }
}
