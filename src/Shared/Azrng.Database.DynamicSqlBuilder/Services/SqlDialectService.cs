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
            DatabaseType.MySql => $"{fieldName} IN {parameterName}",
            DatabaseType.SqlServer => $"{fieldName} IN {parameterName}",
            DatabaseType.Oracle => $"{fieldName} IN {parameterName}",
            DatabaseType.Sqlite => $"{fieldName} IN {parameterName}",
            _ => throw new NotSupportedException($"不支持的数据库方言: {dialect}")
        };
    }

    public static string GetNotInOperatorSql(string fieldName, string parameterName, DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => $"{fieldName} != ALL({parameterName})",
            DatabaseType.MySql => $"{fieldName} NOT IN {parameterName}",
            DatabaseType.SqlServer => $"{fieldName} NOT IN {parameterName}",
            DatabaseType.Oracle => $"{fieldName} NOT IN {parameterName}",
            DatabaseType.Sqlite => $"{fieldName} NOT IN {parameterName}",
            _ => throw new NotSupportedException($"不支持的数据库方言: {dialect}")
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
            DatabaseType.MySql => $"{sql}{orderByClause} LIMIT {pageSize} OFFSET {offset}",
            DatabaseType.SqlServer => GetSqlServerPagingSql(sql, pageSize, offset, orderBy),
            DatabaseType.Oracle => GetOraclePagingSql(sql, pageSize, offset, orderBy),
            DatabaseType.Sqlite => $"{sql}{orderByClause} LIMIT {pageSize} OFFSET {offset}",
            _ => throw new NotSupportedException($"不支持的数据库方言: {dialect}")
        };
    }

    private static string GetSqlServerPagingSql(string sql, int pageSize, int offset, string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            throw new InvalidOperationException("SQL Server 分页必须指定排序字段");
        }

        return $"{sql} ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }

    private static string GetOraclePagingSql(string sql, int pageSize, int offset, string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            throw new InvalidOperationException("Oracle 分页必须指定排序字段");
        }

        return $"{sql} ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }

    public static string GetLikeEscapeCharacter(DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => "\\",
            DatabaseType.MySql => "\\",
            DatabaseType.SqlServer => "\\",
            DatabaseType.Oracle => "\\",
            DatabaseType.Sqlite => "\\",
            _ => "\\"
        };
    }

    public static string GetParameterPrefix(DatabaseType dialect)
    {
        return dialect switch
        {
            DatabaseType.PostgresSql => "@",
            DatabaseType.MySql => "?",
            DatabaseType.SqlServer => "@",
            DatabaseType.Oracle => ":",
            DatabaseType.Sqlite => "@",
            _ => "@"
        };
    }
}
