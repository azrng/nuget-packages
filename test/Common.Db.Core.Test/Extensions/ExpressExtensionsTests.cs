using System.Linq.Expressions;
using Azrng.Core.Extensions;
using FluentAssertions;
using Xunit;

namespace Common.Db.Core.Test.Extensions;

public class ExpressExtensionsTests
{
    [Fact]
    public void MarkEqual_ShouldCompareEachPropertyOnValueObject()
    {
        Expression<Func<CustomerRecord, CustomerAddress>> accessor = item => item.Address;

        var predicate = accessor.MarkEqual(new CustomerAddress
        {
            City = "Shanghai",
            ZipCode = 200000
        }).Compile();

        predicate(new CustomerRecord
        {
            Address = new CustomerAddress { City = "Shanghai", ZipCode = 200000 }
        }).Should().BeTrue();
        predicate(new CustomerRecord
        {
            Address = new CustomerAddress { City = "Beijing", ZipCode = 200000 }
        }).Should().BeFalse();
    }

    [Fact]
    public void MergeAnd_ShouldCombineTwoPredicates()
    {
        Expression<Func<CustomerRecord, bool>> left = item => item.Age >= 18;
        Expression<Func<CustomerRecord, bool>> right = entity => entity.Enabled;

        var predicate = left.MergeAnd(right).Compile();

        predicate(new CustomerRecord { Age = 20, Enabled = true }).Should().BeTrue();
        predicate(new CustomerRecord { Age = 20, Enabled = false }).Should().BeFalse();
    }

    [Fact]
    public void MergeAnd_WithMultiplePredicates_ShouldKeepExistingPredicateAndAllAdditionalConditions()
    {
        Expression<Func<CustomerRecord, bool>> initial = item => item.Age >= 18;
        Expression<Func<CustomerRecord, bool>> named = entity => entity.Name.StartsWith("A");
        Expression<Func<CustomerRecord, bool>> enabled = row => row.Enabled;

        var predicate = initial.MergeAnd(named, enabled)!.Compile();

        predicate(new CustomerRecord { Age = 21, Name = "Alice", Enabled = true }).Should().BeTrue();
        predicate(new CustomerRecord { Age = 21, Name = "Alice", Enabled = false }).Should().BeFalse();
        predicate(new CustomerRecord { Age = 16, Name = "Alice", Enabled = true }).Should().BeFalse();
    }

    [Fact]
    public void MergeOr_WithMultiplePredicates_ShouldKeepExistingPredicateAndMatchAnyCondition()
    {
        Expression<Func<CustomerRecord, bool>> initial = item => item.Age >= 18;
        Expression<Func<CustomerRecord, bool>> named = entity => entity.Name.StartsWith("A");
        Expression<Func<CustomerRecord, bool>> enabled = row => row.Enabled;

        var predicate = initial.MergeOr(named, enabled)!.Compile();

        predicate(new CustomerRecord { Age = 21, Name = "Bob", Enabled = false }).Should().BeTrue();
        predicate(new CustomerRecord { Age = 12, Name = "Alice", Enabled = false }).Should().BeTrue();
        predicate(new CustomerRecord { Age = 12, Name = "Bob", Enabled = false }).Should().BeFalse();
    }

    private sealed class CustomerRecord
    {
        public int Age { get; set; }

        public bool Enabled { get; set; }

        public string Name { get; set; } = string.Empty;

        public CustomerAddress Address { get; set; } = new();
    }

    private sealed class CustomerAddress
    {
        public string City { get; set; } = string.Empty;

        public int ZipCode { get; set; }
    }
}
