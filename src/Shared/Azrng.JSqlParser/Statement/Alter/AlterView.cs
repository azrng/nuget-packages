using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// ALTER VIEW / REPLACE VIEW 语句，对齐上游 AlterView。
/// 形式：<c>ALTER VIEW v [(cols)] AS SELECT ...</c> / <c>REPLACE VIEW v AS SELECT ...</c>。
/// </summary>
public class AlterView : ASTNodeAccessImpl, Statement
{
    public Schema.Table View { get; set; } = null!;

    public Select.Select Select { get; set; } = null!;

    public bool UseReplace { get; set; }

    public List<string> ColumnNames { get; } = new();

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder(UseReplace ? "REPLACE " : "ALTER ");
        sb.Append("VIEW ").Append(View);
        if (ColumnNames.Count > 0)
            sb.Append($" ({string.Join(", ", ColumnNames)})");
        sb.Append(" AS ").Append(Select);
        return sb.ToString();
    }
}
