using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Geometry distance operator &lt;-&gt; / &lt;#&gt; (PostGIS KNN distance).
/// </summary>
public class GeometryDistance : ComparisonOperator
{
    public GeometryDistance() : base("<->") { }

    public GeometryDistance(string op) : base(op) { }

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
