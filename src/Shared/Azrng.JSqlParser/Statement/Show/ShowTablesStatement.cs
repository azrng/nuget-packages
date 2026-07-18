using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Show;

/// <summary>
/// SHOW TABLES 语句，对齐上游 ShowTablesStatement。
/// 形式：<c>SHOW TABLES [FROM db] [LIKE pattern | WHERE expr]</c>。
/// </summary>
public class ShowTablesStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>数据库名（FROM 子句），可选。</summary>
    public string? DbName { get; set; }

    /// <summary>LIKE 模式表达式，可选。</summary>
    public Expression.IExpression? LikeExpression { get; set; }

    /// <summary>WHERE 条件，可选。</summary>
    public Expression.IExpression? WhereCondition { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("SHOW TABLES");
        if (!string.IsNullOrEmpty(DbName)) sb.Append(" FROM ").Append(DbName);
        if (LikeExpression != null) sb.Append(" LIKE ").Append(LikeExpression);
        if (WhereCondition != null) sb.Append(" WHERE ").Append(WhereCondition);
        return sb.ToString();
    }
}
