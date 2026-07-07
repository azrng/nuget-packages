using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Show;

/// <summary>
/// SHOW INDEX FROM table 语句，对齐上游 ShowIndexStatement。
/// </summary>
public class ShowIndexStatement : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }

    /// <summary>数据库名（SHOW INDEX FROM db.table 的 db 部分），可选。</summary>
    public string? DbName { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("SHOW INDEX FROM ");
        if (!string.IsNullOrEmpty(DbName)) sb.Append(DbName).Append('.');
        if (Table != null) sb.Append(Table.Name);
        return sb.ToString();
    }
}
