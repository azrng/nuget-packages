using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class ExpressionableTests
{
    [Fact]
    public void Create_ShouldReturnExpressionableInstance()
    {
        var result = Expressionable.Create<TestEntity>();

        result.Should().NotBeNull();
        result.Should().BeOfType<Expressionable<TestEntity>>();
    }

    [Fact]
    public void And_WhenFirstCondition_ShouldSetExpression()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20 }).Should().BeTrue();
        expression(new TestEntity { Age = 10 }).Should().BeFalse();
    }

    [Fact]
    public void And_WhenMultipleConditions_ShouldCombineWithAndAlso()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .And(x => x.Name.StartsWith("A"))
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20, Name = "Alice" }).Should().BeTrue();
        expression(new TestEntity { Age = 20, Name = "Bob" }).Should().BeFalse();
        expression(new TestEntity { Age = 10, Name = "Alice" }).Should().BeFalse();
    }

    [Fact]
    public void AndIF_WhenConditionIsTrue_ShouldApplyAnd()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .AndIF(true, x => x.Name == "Alice")
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20, Name = "Alice" }).Should().BeTrue();
        expression(new TestEntity { Age = 20, Name = "Bob" }).Should().BeFalse();
    }

    [Fact]
    public void AndIF_WhenConditionIsFalse_ShouldSkipAnd()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .AndIF(false, x => x.Name == "Alice")
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20, Name = "Bob" }).Should().BeTrue();
    }

    [Fact]
    public void AndIF_WhenConditionIsFalseAndFirstExpression_ShouldReturnDefaultTrue()
    {
        var expression = Expressionable.Create<TestEntity>()
            .AndIF(false, x => x.Age > 100)
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 10 }).Should().BeTrue();
    }

    [Fact]
    public void Or_WhenFirstCondition_ShouldSetExpression()
    {
        var expression = Expressionable.Create<TestEntity>()
            .Or(x => x.Age > 18)
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20 }).Should().BeTrue();
        expression(new TestEntity { Age = 10 }).Should().BeFalse();
    }

    [Fact]
    public void Or_WhenMultipleConditions_ShouldCombineWithOrElse()
    {
        var expression = Expressionable.Create<TestEntity>()
            .Or(x => x.Age > 18)
            .Or(x => x.Name.StartsWith("A"))
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20, Name = "Bob" }).Should().BeTrue();
        expression(new TestEntity { Age = 10, Name = "Alice" }).Should().BeTrue();
        expression(new TestEntity { Age = 10, Name = "Bob" }).Should().BeFalse();
    }

    [Fact]
    public void OrIF_WhenConditionIsTrue_ShouldApplyOr()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .OrIF(true, x => x.Name == "Alice")
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 10, Name = "Alice" }).Should().BeTrue();
        expression(new TestEntity { Age = 20, Name = "Bob" }).Should().BeTrue();
        expression(new TestEntity { Age = 10, Name = "Bob" }).Should().BeFalse();
    }

    [Fact]
    public void OrIF_WhenConditionIsFalse_ShouldSkipOr()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .OrIF(false, x => x.Name == "Alice")
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20, Name = "Bob" }).Should().BeTrue();
        expression(new TestEntity { Age = 10, Name = "Alice" }).Should().BeFalse();
    }

    [Fact]
    public void OrIF_WhenConditionIsFalseAndFirstExpression_ShouldReturnDefaultTrue()
    {
        var expression = Expressionable.Create<TestEntity>()
            .OrIF(false, x => x.Age > 100)
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 10 }).Should().BeTrue();
    }

    [Fact]
    public void ToExpression_WhenNoConditionsAdded_ShouldReturnDefaultTrue()
    {
        var expression = Expressionable.Create<TestEntity>()
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 0, Name = "" }).Should().BeTrue();
    }

    [Fact]
    public void FluentChain_ShouldSupportMixedAndOrOperations()
    {
        var expression = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 10)
            .Or(x => x.Name == "VIP")
            .AndIF(true, x => x.IsActive)
            .OrIF(false, x => x.Age < 5)
            .ToExpression()
            .Compile();

        expression(new TestEntity { Age = 20, Name = "Normal", IsActive = true }).Should().BeTrue();
        expression(new TestEntity { Age = 5, Name = "VIP", IsActive = true }).Should().BeTrue();
        expression(new TestEntity { Age = 5, Name = "Normal", IsActive = true }).Should().BeFalse();
    }

    [Fact]
    public void Create_MultipleInstances_ShouldBeIndependent()
    {
        var expr1 = Expressionable.Create<TestEntity>()
            .And(x => x.Age > 18)
            .ToExpression()
            .Compile();

        var expr2 = Expressionable.Create<TestEntity>()
            .And(x => x.Age < 18)
            .ToExpression()
            .Compile();

        var entity = new TestEntity { Age = 20 };

        expr1(entity).Should().BeTrue();
        expr2(entity).Should().BeFalse();
    }

    [Fact]
    public void And_ShouldReturnSameInstance_ForFluentChaining()
    {
        var expressionable = Expressionable.Create<TestEntity>();

        var result = expressionable.And(x => x.Age > 18);

        result.Should().BeSameAs(expressionable);
    }

    [Fact]
    public void Or_ShouldReturnSameInstance_ForFluentChaining()
    {
        var expressionable = Expressionable.Create<TestEntity>();

        var result = expressionable.Or(x => x.Age > 18);

        result.Should().BeSameAs(expressionable);
    }

    [Fact]
    public void AndIF_ShouldReturnSameInstance_ForFluentChaining()
    {
        var expressionable = Expressionable.Create<TestEntity>();

        var result = expressionable.AndIF(true, x => x.Age > 18);

        result.Should().BeSameAs(expressionable);
    }

    [Fact]
    public void OrIF_ShouldReturnSameInstance_ForFluentChaining()
    {
        var expressionable = Expressionable.Create<TestEntity>();

        var result = expressionable.OrIF(true, x => x.Age > 18);

        result.Should().BeSameAs(expressionable);
    }

    [Fact]
    public void And_WithNestedLambda_ShouldPreserveInnerParameter()
    {
        var expression = Expressionable.Create<NestedEntity>()
            .And(x => x.Children.Any(c => c.Age > 10))
            .And(x => x.Age >= 18)
            .ToExpression()
            .Compile();

        expression(new NestedEntity { Age = 20, Children = [new NestedEntity.ChildEntity { Age = 11 }] }).Should().BeTrue();
        expression(new NestedEntity { Age = 10, Children = [new NestedEntity.ChildEntity { Age = 11 }] }).Should().BeFalse();
        expression(new NestedEntity { Age = 20, Children = [new NestedEntity.ChildEntity { Age = 5 }] }).Should().BeFalse();
    }

    [Fact]
    public void And_WithNestedLambdaClosureOverOuterParameter_ShouldPreserveBothParameters()
    {
        var expression = Expressionable.Create<NestedEntity>()
            .And(x => x.Children.Any(c => c.Age > x.MinChildAge))
            .ToExpression()
            .Compile();

        expression(new NestedEntity { MinChildAge = 10, Children = [new NestedEntity.ChildEntity { Age = 11 }] }).Should().BeTrue();
        expression(new NestedEntity { MinChildAge = 20, Children = [new NestedEntity.ChildEntity { Age = 11 }] }).Should().BeFalse();
    }

    private sealed class TestEntity
    {
        public int Age { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class NestedEntity
    {
        public int Age { get; set; }

        public int MinChildAge { get; set; }

        public List<ChildEntity> Children { get; set; } = [];

        public sealed class ChildEntity
        {
            public int Age { get; set; }
        }
    }
}
