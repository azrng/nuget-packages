using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.CreateView;

/// <summary>
/// Represents a CREATE VIEW statement in SQL.
/// </summary>
public class CreateView : ASTNodeAccessImpl, Statement
{
    public Table? View { get; set; }
    public Select.Select? Select { get; set; }
    public bool OrReplace { get; set; }
    public bool IfNotExists { get; set; }

    /// <summary>CREATE TEMPORARY/TEMP VIEW 标记：null=未指定，"TEMPORARY"/"TEMP"=对应关键字。对齐上游 temp(TemporaryOption)。</summary>
    public string? Temporary { get; set; }

    /// <summary>CREATE RECURSIVE VIEW（PostgreSQL）。</summary>
    public bool Recursive { get; set; }

    /// <summary>WITH CHECK OPTION：null=无，"CASCADED"/"LOCAL"=对应修饰，""=无修饰 CHECK OPTION。</summary>
    public string? WithCheckOption { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE ");
        if (OrReplace) sb.Append("OR REPLACE ");
        if (Temporary != null) sb.Append(Temporary).Append(' ');
        if (Recursive) sb.Append("RECURSIVE ");
        sb.Append("VIEW ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(View);
        if (Select != null) sb.Append(" AS ").Append(Select);
        if (WithCheckOption != null)
        {
            sb.Append(" WITH ");
            if (!string.IsNullOrEmpty(WithCheckOption)) sb.Append(WithCheckOption).Append(' ');
            sb.Append("CHECK OPTION");
        }
        return sb.ToString();
    }
}
