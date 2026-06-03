using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Cosine similarity operator &lt;=&gt; (Hyperscan/Clickhouse style).
/// </summary>
public class CosineSimilarity : ComparisonOperator
{
    public CosineSimilarity() : base("<=>") { }

    public CosineSimilarity(string op) : base(op) { }

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
