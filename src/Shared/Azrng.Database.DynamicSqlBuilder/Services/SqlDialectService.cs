using Azrng.Core.Model;

namespace Azrng.Database.DynamicSqlBuilder.Services;

/// <summary>
/// SQL数据库方言服务 - 处理不同数据库的语法差异
/// </summary>
public static class SqlDialectService
{
    /// <summary>
    /// 获取IN操作符的SQL语法
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="parameterName">参数名</param>
    /// <param name="dialect">数据库方言</param>
    /// <returns>SQL片段</returns>
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

    /// <summary>
    /// 获取NOT IN操作符的SQL语法
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="parameterName">参数名</param>
    /// <param name="dialect">数据库方言</param>
    /// <returns>SQL片段</returns>
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

    /// <summary>
    /// 获取分页SQL语法
    /// </summary>
    /// <param name="sql">原始SQL</param>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="dialect">数据库方言</param>
    /// <returns>分页SQL</returns>
    public static string GetPagingSql(string sql, int pageIndex, int pageSize, string orderBy, DatabaseType dialect)
    {
        var offset = (pageIndex - 1) * pageSize;

        return dialect switch
        {
            DatabaseType.PostgresSql => $"{sql} ORDER BY {orderBy} LIMIT {pageSize} OFFSET {offset}",
            DatabaseType.MySql => $"{sql} ORDER BY {orderBy} LIMIT {pageSize} OFFSET {offset}",
            DatabaseType.SqlServer => GetSqlServerPagingSql(sql, pageSize, offset, orderBy),
            DatabaseType.Oracle => GetOraclePagingSql(sql, pageSize, offset, orderBy),
            DatabaseType.Sqlite => $"{sql} ORDER BY {orderBy} LIMIT {pageSize} OFFSET {offset}",
            _ => throw new NotSupportedException($"不支持的数据库方言: {dialect}")
        };
    }

    /// <summary>
    /// 获取SQL Server的分页SQL（使用OFFSET FETCH）
    /// </summary>
    private static string GetSqlServerPagingSql(string sql, int pageSize, int offset, string orderBy)
    {
        return $"{sql} ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }

    /// <summary>
    /// 获取Oracle的分页SQL（使用ROWNUM）
    /// </summary>
    private static string GetOraclePagingSql(string sql, int pageSize, int offset, string orderBy)
    {
        // Oracle 12c+ 使用 OFFSET FETCH
        return $"{sql} ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }

    /// <summary>
    /// 获取LIKE操作符的转义字符
    /// </summary>
    /// <param name="dialect">数据库方言</param>
    /// <returns>转义字符</returns>
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

    /// <summary>
    /// 获取参数占位符前缀
    /// </summary>
    /// <param name="dialect">数据库方言</param>
    /// <returns>参数前缀</returns>
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
