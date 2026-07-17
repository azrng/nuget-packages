using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Show;

/// <summary>
/// SHOW COLUMNS FROM table 语句，对齐上游 ShowColumnsStatement。
/// </summary>
public class ShowColumnsStatement : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }

    /// <summary>是否带 FULL 关键字（SHOW FULL COLUMNS）。</summary>
    public bool Full { get; set; }

    /// <summary>数据库名（SHOW COLUMNS FROM db.table 的 db 部分），可选。</summary>
    public string? DbName { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("SHOW ");
        if (Full) sb.Append("FULL ");
        sb.Append("COLUMNS FROM ");
        if (!string.IsNullOrEmpty(DbName)) sb.Append(DbName).Append('.');
        if (Table != null) sb.Append(Table.Name);
        return sb.ToString();
    }
}
