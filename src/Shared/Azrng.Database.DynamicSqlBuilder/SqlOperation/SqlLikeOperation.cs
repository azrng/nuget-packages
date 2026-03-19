using Azrng.Database.DynamicSqlBuilder.Services;
using Dapper;

namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// sql like 运算符（支持转义和多种数据库方言）
    /// </summary>
    public class SqlLikeOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;

            // 使用LikeEscapeHelper创建安全的LIKE表达式
            // 默认使用Contains模式（包含）
            var likeValue = LikeEscapeHelper.CreateSearchPattern(
                fieldValue,
                LikeMatchType.Contains,
                dialect
            );

            parameters.Add(paramName, likeValue);

            var escapeChar = SqlDialectService.GetLikeEscapeCharacter(dialect);
            return $"{fieldName} LIKE {paramName} ESCAPE '{escapeChar}'";
        }
    }

    /// <summary>
    /// sql not like 运算符（支持转义和多种数据库方言）
    /// </summary>
    public class SqlNotLikeOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;

            // 使用LikeEscapeHelper创建安全的NOT LIKE表达式
            var likeValue = LikeEscapeHelper.CreateSearchPattern(
                fieldValue,
                LikeMatchType.Contains,
                dialect
            );

            parameters.Add(paramName, likeValue);

            var escapeChar = SqlDialectService.GetLikeEscapeCharacter(dialect);
            return $"{fieldName} NOT LIKE {paramName} ESCAPE '{escapeChar}'";
        }
    }
}
