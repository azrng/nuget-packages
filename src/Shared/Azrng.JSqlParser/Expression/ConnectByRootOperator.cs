using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Oracle CONNECT_BY_ROOT expression for hierarchical queries.
/// <para>对齐上游 ConnectByRootOperator，操作数为 Expression（commit 624a768b）。</para>
/// </summary>
public class ConnectByRootOperator : ASTNodeAccessImpl, IExpression
{
    public IExpression Expression { get; set; } = null!;

    public ConnectByRootOperator() { }

    public ConnectByRootOperator(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("CONNECT_BY_ROOT ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
