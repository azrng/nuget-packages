using System.Linq.Expressions;
using Azrng.Core.CommonDto;
using FluentAssertions;
using Xunit;

namespace Common.Db.Core.Test.CommonDto;

public class PredicateExpressionVisitorTests
{
    [Fact]
    public void Visit_ShouldReplacePredicateParameter()
    {
        Expression<Func<SampleEntity, bool>> expression = source => source.Age >= 18 && source.Enabled;
        var replacement = Expression.Parameter(typeof(SampleEntity), "target");
        var visitor = new PredicateExpressionVisitor(replacement);

        var visitedBody = visitor.Visit(expression.Body);
        var lambda = Expression.Lambda<Func<SampleEntity, bool>>(visitedBody!, replacement).Compile();

        lambda(new SampleEntity { Age = 20, Enabled = true }).Should().BeTrue();
        lambda(new SampleEntity { Age = 17, Enabled = true }).Should().BeFalse();
        visitedBody!.ToString().Should().Contain("target");
    }

    private sealed class SampleEntity
    {
        public int Age { get; set; }

        public bool Enabled { get; set; }
    }
}
