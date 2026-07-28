using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Database;

/// <summary>
/// CREATE DATABASE 语句。
/// <para>语法：CREATE DATABASE [IF NOT EXISTS] databaseName</para>
/// 对齐上游 issue #2070 常见 DDL 需求。
/// </summary>
public class CreateDatabase : ASTNodeAccessImpl, IStatement
{
    /// <summary>IF NOT EXISTS 修饰符。</summary>
    public bool IfNotExists { get; set; }

    /// <summary>数据库名。</summary>
    public string? DatabaseName { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE DATABASE");
        if (IfNotExists) sb.Append(" IF NOT EXISTS");
        if (DatabaseName != null) sb.Append(' ').Append(DatabaseName);
        return sb.ToString();
    }
}
