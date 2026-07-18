namespace Azrng.JSqlParser.Parser;

/// <summary>
/// SQL 解析入口（已弃用，转发到 <see cref="SqlParser"/>）。
/// </summary>
/// <remarks>
/// 历史命名（CC = Composite Components，上游 JSqlParser 作者早期命名）。
/// 新代码请改用 <see cref="SqlParser"/>；本类保留仅为向后兼容，所有方法直接转发。
/// </remarks>
[Obsolete("改用 " + nameof(SqlParser))]
public static class CCJSqlParserUtil
{
    /// <inheritdoc cref="SqlParser.Parse(string?)"/>
    public static Statement.IStatement? Parse(string? sql) => SqlParser.Parse(sql);

    /// <inheritdoc cref="SqlParser.ParseStatements(string?)"/>
    public static Statement.Statements? ParseStatements(string? sql) => SqlParser.ParseStatements(sql);

    /// <inheritdoc cref="SqlParser.ParseExpression(string?)"/>
    public static Expression.IExpression? ParseExpression(string? sql) => SqlParser.ParseExpression(sql);

    /// <inheritdoc cref="SqlParser.ParseCondExpression(string?)"/>
    public static Expression.IExpression? ParseCondExpression(string? sql) => SqlParser.ParseCondExpression(sql);

    /// <inheritdoc cref="SqlParser.ParseNullable(string)"/>
    public static Statement.IStatement? ParseNullable(string sql) => SqlParser.ParseNullable(sql);
}
