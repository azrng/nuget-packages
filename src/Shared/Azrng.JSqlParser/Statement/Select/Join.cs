using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a JOIN clause in a SQL statement.
/// </summary>
public class Join : ASTNodeAccessImpl
{
    public bool Outer { get; set; }
    public bool Right { get; set; }
    public bool Left { get; set; }
    public bool Natural { get; set; }
    public bool Full { get; set; }
    public bool Inner { get; set; }
    public bool Simple { get; set; }
    public bool Cross { get; set; }
    public bool Semi { get; set; }

    /// <summary>是否为 JPQL/HQL 的 JOIN FETCH（预加载关联）。</summary>
    public bool Fetch { get; set; }

    public FromItem RightItem { get; set; } = null!;
    public Expression.Expression? OnExpression { get; set; }
    public List<Column> UsingColumns { get; set; } = new();

    public bool IsInnerJoin()
    {
        return Inner || !(Left || Right || Full || Outer || Cross || Natural);
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        if (Simple)
        {
            if (RightItem == null)
                throw new InvalidOperationException("Simple join requires a right item.");

            sb.Append(", ").Append(RightItem);
            return sb.ToString();
        }

        if (Inner) sb.Append("INNER ");
        else if (Left) sb.Append("LEFT ");
        else if (Right) sb.Append("RIGHT ");
        else if (Full) sb.Append("FULL ");
        if (Outer) sb.Append("OUTER ");
        if (Natural) sb.Append("NATURAL ");
        if (Cross) sb.Append("CROSS ");
        if (Semi) sb.Append("SEMI ");

        if (RightItem == null)
            throw new InvalidOperationException("Join requires a right item.");

        sb.Append("JOIN ");
        if (Fetch) sb.Append("FETCH ");
        sb.Append(RightItem);

        if (OnExpression != null)
            sb.Append(" ON ").Append(OnExpression);
        else if (UsingColumns.Count > 0)
            sb.Append(" USING (").Append(string.Join(", ", UsingColumns)).Append(")");

        return sb.ToString();
    }
}
