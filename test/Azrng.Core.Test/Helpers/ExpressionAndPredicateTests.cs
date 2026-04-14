using System.Linq.Expressions;
using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class ExpressionAndPredicateTests
{
    [Fact]
    public void TrueAndFalseHelpers_ShouldReturnExpectedResults()
    {
        PredicateExtensions.True<int>().Compile()(123).Should().BeTrue();
        PredicateExtensions.False<int>().Compile()(123).Should().BeFalse();
    }

    [Fact]
    public void PredicateAndOr_ShouldCombineExpressions()
    {
        Expression<Func<SampleItem, bool>> left = x => x.Age >= 18;
        Expression<Func<SampleItem, bool>> right = x => x.Name.StartsWith("A");

        var andFunc = left.And(right).Compile();
        var orFunc = left.Or(right).Compile();

        andFunc(new SampleItem { Age = 20, Name = "Alice" }).Should().BeTrue();
        andFunc(new SampleItem { Age = 20, Name = "Bob" }).Should().BeFalse();
        orFunc(new SampleItem { Age = 10, Name = "Alice" }).Should().BeTrue();
        orFunc(new SampleItem { Age = 10, Name = "Bob" }).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_ShouldCreateWorkingBinaryExpression()
    {
        var body = Expression.Property(Expression.Parameter(typeof(SampleItem), "x"), nameof(SampleItem.Age))
            .GreaterThan(Expression.Constant(18));

        body.NodeType.Should().Be(ExpressionType.GreaterThan);
    }

    [Fact]
    public void Expressionable_ShouldCombineAndOrBranches()
    {
        var expression = Expressionable.Create<SampleItem>()
            .And(x => x.Age >= 18)
            .AndIF(true, x => x.Name.StartsWith("A"))
            .OrIF(false, x => x.Name == "Ignored")
            .ToExpression()
            .Compile();

        expression(new SampleItem { Age = 20, Name = "Alice" }).Should().BeTrue();
        expression(new SampleItem { Age = 20, Name = "Bob" }).Should().BeFalse();
    }

    [Fact]
    public void Expressionable_WithoutConditions_ShouldDefaultToTrue()
    {
        var expression = Expressionable.Create<SampleItem>().ToExpression().Compile();

        expression(new SampleItem()).Should().BeTrue();
    }

    private sealed class SampleItem
    {
        public int Age { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
