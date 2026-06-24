using Azrng.Core.Model;

namespace Azrng.Database.DynamicSqlBuilder.Services;

/// <summary>
/// SQL数据库方言服务 - 处理不同数据库的语法差异
/// </summary>
public static class SqlDialectService
{
    public static string GetInOperatorSql(string fieldName, string parameterName, DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => $"{fieldName} = ANY({parameterName})",
            _ => ThrowUnsupportedDialect(dialect)
        };
    }

    public static string GetNotInOperatorSql(string fieldName, string parameterName, DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => $"{fieldName} != ALL({parameterName})",
            _ => ThrowUnsupportedDialect(dialect)
        };
    }

    public static string GetPagingSql(string sql, int pageIndex, int pageSize, string orderBy, DatabaseType dialect)
    {
        var offset = (pageIndex - 1) * pageSize;
        var hasOrderBy = !string.IsNullOrWhiteSpace(orderBy);
        var orderByClause = hasOrderBy ? $" ORDER BY {orderBy}" : string.Empty;

        return dialect switch
        {
            DatabaseType.PostgresSql => $"{sql}{orderByClause} LIMIT {pageSize} OFFSET {offset}",
            _ => ThrowUnsupportedDialect(dialect)
        };
    }

    public static string GetLikeEscapeCharacter(DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => "\\",
            _ => ThrowUnsupportedDialect(dialect)
        };
    }

    public static string GetParameterPrefix(DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => "@",
            _ => ThrowUnsupportedDialect(dialect)
        };
    }

    private static string ThrowUnsupportedDialect(DatabaseType dialect)
    {
        throw new NotSupportedException($"DynamicSqlBuilder 当前仅支持 PostgreSQL 方言，暂不支持: {dialect}");
    }
}
