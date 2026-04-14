using Azrng.Core.Extensions;
using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Common.Db.Core.Test.Extensions;

public class QueryableExtensionsTests
{
    [Fact]
    public void SelectMapper_ShouldCopyMatchingProperties()
    {
        var query = CreatePeople().AsQueryable();

        var result = query.SelectMapper<PersonEntity, PersonListItem>().ToList();

        result.Should().HaveCount(4);
        result[0].Name.Should().Be("Alice");
        result[0].Age.Should().Be(30);
        result[0].Ignored.Should().BeNull();
    }

    [Fact]
    public void OrderBy_WithExpression_ShouldSupportAscAndDesc()
    {
        var query = CreatePeople().AsQueryable();

        var asc = query.OrderBy(x => x.Age, SortEnum.Asc).Select(x => x.Name).ToList();
        var desc = query.OrderBy(x => x.Age, SortEnum.Desc).Select(x => x.Name).ToList();

        asc.Should().Equal("David", "Bob", "Charlie", "Alice");
        desc.Should().Equal("Alice", "Charlie", "Bob", "David");
    }

    [Fact]
    public void OrderBy_WithSortContentAndFieldName_ShouldSortByConfiguredProperty()
    {
        var query = CreatePeople().AsQueryable();

        var bySortContent = query.OrderBy(new SortContent(nameof(PersonEntity.Name), SortEnum.Desc))
            .Select(x => x.Name)
            .ToList();
        var byFieldName = query.OrderBy(nameof(PersonEntity.Age), isAsc: false)
            .Select(x => x.Name)
            .ToList();

        bySortContent.Should().Equal("David", "Charlie", "Bob", "Alice");
        byFieldName.Should().Equal("Alice", "Charlie", "Bob", "David");
    }

    [Fact]
    public void OrderBy_WithMultipleFields_ShouldApplyThenByBehavior()
    {
        var query = CreatePeople().AsQueryable();

        var ordered = query.OrderBy(
                new FiledOrderParam { PropertyName = nameof(PersonEntity.Enabled), IsAsc = false },
                new FiledOrderParam { PropertyName = nameof(PersonEntity.Age), IsAsc = true })
            .Select(x => x.Name)
            .ToList();

        ordered.Should().Equal("Charlie", "Alice", "David", "Bob");
    }

    [Fact]
    public void PagedBy_AndCountBy_ShouldReturnExpectedSliceAndTotals()
    {
        var query = CreatePeople().AsQueryable();
        var request = new GetPageRequest(pageIndex: 2, pageSize: 2);

        var paged = query.OrderBy(x => x.Id, SortEnum.Asc).PagedBy(request, out var totalCount).ToList();
        var sameQuery = query.CountBy(out var countedTotal);

        totalCount.Should().Be(4);
        countedTotal.Should().Be(4);
        sameQuery.Should().BeSameAs(query);
        paged.Select(x => x.Name).Should().Equal("Charlie", "David");
    }

    [Fact]
    public void PagedBy_WithNumericParameters_ShouldWork()
    {
        var query = CreatePeople().AsQueryable();

        var paged = query.OrderBy(x => x.Id, SortEnum.Asc).PagedBy(pageIndex: 1, pageSize: 3, out var totalCount).ToList();

        totalCount.Should().Be(4);
        paged.Select(x => x.Name).Should().Equal("Alice", "Bob", "Charlie");
    }

    [Fact]
    public void WhereAny_ShouldSupportEmptyAndMultiplePredicates()
    {
        var query = CreatePeople().AsQueryable();

        var empty = query.WhereAny().ToList();
        var matches = query.WhereAny(x => x.Age >= 30, x => x.Name == "Bob").Select(x => x.Name).ToList();

        empty.Should().BeEmpty();
        matches.Should().Equal("Alice", "Bob");
    }

    [Fact]
    public void ComparisonWhereHelpers_ShouldUseExpectedOperators()
    {
        var query = CreatePeople().AsQueryable();

        var equals = query.EqualWhere(nameof(PersonEntity.Age), 25).Select(x => x.Name).ToList();
        var less = query.LessWhere(nameof(PersonEntity.Age), 25).Select(x => x.Name).ToList();
        var greater = query.GreaterWhere(nameof(PersonEntity.Age), 25).Select(x => x.Name).ToList();

        equals.Should().Equal("Bob");
        less.Should().Equal("David");
        greater.Should().Equal("Alice", "Charlie");
    }

    private static List<PersonEntity> CreatePeople() =>
    [
        new PersonEntity { Id = 1, Name = "Alice", Age = 30, Enabled = true },
        new PersonEntity { Id = 2, Name = "Bob", Age = 25, Enabled = false },
        new PersonEntity { Id = 3, Name = "Charlie", Age = 28, Enabled = true },
        new PersonEntity { Id = 4, Name = "David", Age = 20, Enabled = false }
    ];

    private sealed class PersonEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        public bool Enabled { get; set; }
    }

    private sealed class PersonListItem
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        public string? Ignored { get; set; }
    }
}
