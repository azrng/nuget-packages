using System.Text;
using JSqlParser.Net.Expression;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Abstract base class for SELECT statements.
/// Implements both Statement and Expression interfaces.
/// </summary>
public abstract class Select : ASTNodeAccessImpl, Statement, Expression.Expression
{
    public List<WithItem>? WithItemsList { get; set; }
    public Limit? Limit { get; set; }
    public Limit? LimitBy { get; set; }
    public Offset? Offset { get; set; }
    public Fetch? Fetch { get; set; }
    public bool OracleSiblings { get; set; }
    public List<OrderByElement>? OrderByElements { get; set; }

    public abstract T Accept<T, S>(SelectVisitor<T> selectVisitor, S context);

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public abstract StringBuilder AppendSelectBodyTo(StringBuilder builder);

    public new StringBuilder AppendTo(StringBuilder builder)
    {
        if (WithItemsList != null && WithItemsList.Count > 0)
        {
            builder.Append("WITH ");
            for (int i = 0; i < WithItemsList.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(WithItemsList[i]);
                builder.Append(' ');
            }
        }

        AppendSelectBodyTo(builder);

        if (OrderByElements != null && OrderByElements.Count > 0)
        {
            builder.Append(OracleSiblings ? " ORDER SIBLINGS BY " : " ORDER BY ");
            builder.Append(string.Join(", ", OrderByElements));
        }

        if (LimitBy != null) builder.Append(LimitBy);
        if (Limit != null) builder.Append(Limit);
        if (Offset != null) builder.Append(Offset);
        if (Fetch != null) builder.Append(Fetch);

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();

    [Obsolete("Use the specific select body type directly")]
    public Select GetSelectBody() => this;
}
