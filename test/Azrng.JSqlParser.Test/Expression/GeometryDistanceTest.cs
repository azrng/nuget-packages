using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// PostGIS 几何距离算子 &lt;-&gt; / &lt;#&gt; 测试（移植自上游 GeometryDistance）。
/// </summary>
public class GeometryDistanceTest
{
    [Fact]
    public void GeometryDistance_KnnOperator_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM roads ORDER BY geom <-> 'POINT(0 0)' LIMIT 10";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var orderBy = select.OrderByElements![0];
        Assert.IsType<GeometryDistance>(orderBy.Expression);
        var geo = (GeometryDistance)orderBy.Expression;
        Assert.Equal("<->", geo.Operator);

        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void GeometryDistance_HashOperator_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM roads ORDER BY geom <#> 'POINT(0 0)' LIMIT 10";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var orderBy = select.OrderByElements![0];
        var geo = Assert.IsType<GeometryDistance>(orderBy.Expression);
        Assert.Equal("<#>", geo.Operator);

        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void GeometryDistance_InSelect_ShouldRoundTrip()
    {
        var sql = "SELECT a <-> b FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var geo = Assert.IsType<GeometryDistance>(select.SelectItems![0].Expression);
        Assert.Equal("<->", geo.Operator);

        Assert.Equal(sql, select.ToString());
    }
}
