using Azrng.Core.Model;
using System.Text;
using Azrng.Database.DynamicSqlBuilder.Model;

namespace Azrng.Database.DynamicSqlBuilder.Services;

/// <summary>
/// LIKE操作符转义助手 - 防止LIKE注入和通配符意外匹配
/// </summary>
public static class LikeEscapeHelper
{
    /// <summary>
    /// 转义LIKE模式中的特殊字符
    /// </summary>
    /// <param name="pattern">原始模式</param>
    /// <param name="dialect">数据库方言</param>
    /// <returns>转义后的模式</returns>
    public static string EscapeLikePattern(string pattern, DatabaseType dialect = DatabaseType.PostgresSql)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        var sb = new StringBuilder();
        var escapeChar = SqlDialectService.GetLikeEscapeCharacter(dialect);

        foreach (char c in pattern)
        {
            switch (c)
            {
                case '\\':
                    // 反斜杠需要转义
                    sb.Append(escapeChar).Append('\\');
                    break;
                case '%':
                    // 百分号是通配符，需要转义
                    sb.Append(escapeChar).Append('%');
                    break;
                case '_':
                    // 下划线是单字符通配符，需要转义
                    sb.Append(escapeChar).Append('_');
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 为SQL查询创建安全的LIKE表达式
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="pattern">搜索模式</param>
    /// <param name="matchOperator">匹配操作符（Like或NotLike）</param>
    /// <param name="dialect">数据库方言</param>
    /// <returns>SQL表达式</returns>
    public static string CreateSafeLikeExpression(
        string fieldName,
        string pattern,
        MatchOperator matchOperator = MatchOperator.Like,
        DatabaseType dialect = DatabaseType.PostgresSql)
    {
        var escapedPattern = EscapeLikePattern(pattern, dialect);
        var escapeChar = SqlDialectService.GetLikeEscapeCharacter(dialect);

        var operatorStr = matchOperator == MatchOperator.Like ? "LIKE" : "NOT LIKE";

        // 某些数据库需要显式指定ESCAPE子句
        var escapeClause = dialect switch
        {
            DatabaseType.PostgresSql => $" ESCAPE '{escapeChar}'",
            DatabaseType.MySql => $" ESCAPE '{escapeChar}'",
            DatabaseType.SqlServer => $" ESCAPE '{escapeChar}'",
            DatabaseType.Oracle => $" ESCAPE '{escapeChar}'",
            DatabaseType.Sqlite => $" ESCAPE '{escapeChar}'",
            _ => ""
        };

        return $"{fieldName} {operatorStr} {escapedPattern}{escapeClause}";
    }

    /// <summary>
    /// 创建包含通配符的搜索模式
    /// </summary>
    /// <param name="searchText">搜索文本</param>
    /// <param name="matchType">匹配类型</param>
    /// <param name="dialect">数据库方言</param>
    /// <returns>转义后的模式</returns>
    public static string CreateSearchPattern(
        string searchText,
        LikeMatchType matchType = LikeMatchType.Contains,
        DatabaseType dialect = DatabaseType.PostgresSql)
    {
        if (string.IsNullOrEmpty(searchText))
            return "%";

        var escapedText = EscapeLikePattern(searchText, dialect);

        return matchType switch
        {
            LikeMatchType.Contains => $"%{escapedText}%",
            LikeMatchType.StartsWith => $"{escapedText}%",
            LikeMatchType.EndsWith => $"%{escapedText}",
            LikeMatchType.Exact => escapedText,
            _ => $"%{escapedText}%"
        };
    }
}

/// <summary>
/// LIKE匹配类型
/// </summary>
public enum LikeMatchType
{
    /// <summary>
    /// 包含（%text%）
    /// </summary>
    Contains,

    /// <summary>
    /// 开始于（text%）
    /// </summary>
    StartsWith,

    /// <summary>
    /// 结束于（%text）
    /// </summary>
    EndsWith,

    /// <summary>
    /// 精确匹配（text）
    /// </summary>
    Exact
}
