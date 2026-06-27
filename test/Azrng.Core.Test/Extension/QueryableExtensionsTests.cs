using Azrng.Core.Extension;
using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class QueryableExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDtoExtra
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extra { get; set; } = string.Empty;
    }

    private static IQueryable<TestEntity> CreateTestData()
    {
        return new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 30, CreatedAt = new DateTime(2023, 1, 1) },
            new() { Id = 2, Name = "Bob", Age = 25, CreatedAt = new DateTime(2023, 2, 1) },
            new() { Id = 3, Name = "Charlie", Age = 35, CreatedAt = new DateTime(2023, 3, 1) },
            new() { Id = 4, Name = "David", Age = 20, CreatedAt = new DateTime(2023, 4, 1) },
            new() { Id = 5, Name = "Eve", Age = 28, CreatedAt = new DateTime(2023, 5, 1) }
        }.AsQueryable();
    }

    #region SelectMapper

    [Fact]
    public void SelectMapper_ShouldMapCommonProperties()
    {
        var data = CreateTestData();

        var result = data.SelectMapper<TestEntity, TestDto>().ToList();

        result.Should().HaveCount(5);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void SelectMapper_ShouldIgnoreMissingProperties()
    {
        var data = CreateTestData();

        var result = data.SelectMapper<TestEntity, TestDtoExtra>().ToList();

        result.Should().HaveCount(5);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Alice");
        result[0].Extra.Should().BeEmpty();
    }

    #endregion

    #region OrderBy (isAsc bool)

    [Fact]
    public void OrderBy_WithBoolAsc_ShouldSortAscending()
    {
        var data = CreateTestData();

        var result = data.OrderBy(x => x.Age, true).ToList();

        result.Select(x => x.Age).Should().BeInAscendingOrder();
    }

    [Fact]
    public void OrderBy_WithBoolDesc_ShouldSortDescending()
    {
        var data = CreateTestData();

        var result = data.OrderBy(x => x.Age, false).ToList();

        result.Select(x => x.Age).Should().BeInDescendingOrder();
    }

    #endregion

    #region OrderBy (SortEnum)

    [Fact]
    public void OrderBy_WithSortEnumAsc_ShouldSortAscending()
    {
        var data = CreateTestData();

        var result = data.OrderBy(x => x.Name, SortEnum.Asc).ToList();

        result.Select(x => x.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public void OrderBy_WithSortEnumDesc_ShouldSortDescending()
    {
        var data = CreateTestData();

        var result = data.OrderBy(x => x.Name, SortEnum.Desc).ToList();

        result.Select(x => x.Name).Should().BeInDescendingOrder();
    }

    [Fact]
    public void OrderBy_WithSortEnum_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.OrderBy(x => x.Id, SortEnum.Asc);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OrderBy_WithSortEnum_ShouldThrow_WhenKeySelectorIsNull()
    {
        var data = CreateTestData();

        var action = () => data.OrderBy((System.Linq.Expressions.Expression<Func<TestEntity, int>>)null!, SortEnum.Asc);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region OrderBy (params SortContent[])

    [Fact]
    public void OrderBy_WithSortContentArray_ShouldApplyMultipleSorts()
    {
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "B", Age = 30 },
            new() { Id = 2, Name = "A", Age = 25 },
            new() { Id = 3, Name = "A", Age = 35 },
            new() { Id = 4, Name = "B", Age = 20 }
        }.AsQueryable();

        var sorts = new[]
        {
            new SortContent("Name", SortEnum.Asc),
            new SortContent("Age", SortEnum.Desc)
        };

        var result = data.OrderBy(sorts).ToList();

        result[0].Name.Should().Be("A");
        result[0].Age.Should().Be(35);
        result[1].Name.Should().Be("B");
        result[1].Age.Should().Be(30);
        result[2].Name.Should().Be("A");
        result[2].Age.Should().Be(25);
        result[3].Name.Should().Be("B");
        result[3].Age.Should().Be(20);
    }

    [Fact]
    public void OrderBy_WithSortContentArray_ShouldReturnSame_WhenEmpty()
    {
        var data = CreateTestData();

        var result = data.OrderBy(Array.Empty<SortContent>()).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void OrderBy_WithSortContentArray_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.OrderBy(new SortContent("Name", SortEnum.Asc));

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region OrderBy (SortContent)

    [Fact]
    public void OrderBy_WithSortContent_ShouldSortAscending()
    {
        var data = CreateTestData();

        var result = data.OrderBy(new SortContent("Age", SortEnum.Asc)).ToList();

        result.Select(x => x.Age).Should().BeInAscendingOrder();
    }

    [Fact]
    public void OrderBy_WithSortContent_ShouldSortDescending()
    {
        var data = CreateTestData();

        var result = data.OrderBy(new SortContent("Age", SortEnum.Desc)).ToList();

        result.Select(x => x.Age).Should().BeInDescendingOrder();
    }

    [Fact]
    public void OrderBy_WithSortContent_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.OrderBy(new SortContent("Name", SortEnum.Asc));

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OrderBy_WithSortContent_ShouldThrow_WhenSortNameIsNullOrWhiteSpace()
    {
        var data = CreateTestData();

        var action1 = () => data.OrderBy(new SortContent(null!, SortEnum.Asc));
        var action2 = () => data.OrderBy(new SortContent("", SortEnum.Asc));
        var action3 = () => data.OrderBy(new SortContent("   ", SortEnum.Asc));

        action1.Should().Throw<ArgumentNullException>();
        action2.Should().Throw<ArgumentNullException>();
        action3.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region OrderBy (string sortField)

    [Fact]
    public void OrderBy_StringField_ShouldSortAscending()
    {
        var data = CreateTestData();

        var result = data.OrderBy("Age", true).ToList();

        result.Select(x => x.Age).Should().BeInAscendingOrder();
    }

    [Fact]
    public void OrderBy_StringField_ShouldSortDescending()
    {
        var data = CreateTestData();

        var result = data.OrderBy("Age", false).ToList();

        result.Select(x => x.Age).Should().BeInDescendingOrder();
    }

    [Fact]
    public void OrderBy_StringField_ShouldDefaultToAscending()
    {
        var data = CreateTestData();

        var result = data.OrderBy("Name").ToList();

        result.Select(x => x.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public void OrderBy_StringField_ShouldThrow_WhenPropertyNotFound()
    {
        var data = CreateTestData();

        var action = () => data.OrderBy("NonExistent");

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region OrderBy (params FiledOrderParam[])

    [Fact]
    public void OrderBy_WithFiledOrderParams_ShouldApplyMultipleSorts()
    {
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "B", Age = 30 },
            new() { Id = 2, Name = "A", Age = 25 },
            new() { Id = 3, Name = "A", Age = 35 },
            new() { Id = 4, Name = "B", Age = 20 }
        }.AsQueryable();

        var orderParams = new[]
        {
            new FiledOrderParam { PropertyName = "Name", IsAsc = true },
            new FiledOrderParam { PropertyName = "Age", IsAsc = false }
        };

        var result = data.OrderBy(orderParams).ToList();

        result[0].Name.Should().Be("A");
        result[0].Age.Should().Be(35);
        result[1].Name.Should().Be("A");
        result[1].Age.Should().Be(25);
        result[2].Name.Should().Be("B");
        result[2].Age.Should().Be(30);
        result[3].Name.Should().Be("B");
        result[3].Age.Should().Be(20);
    }

    [Fact]
    public void OrderBy_WithFiledOrderParams_ShouldReturnSame_WhenEmpty()
    {
        var data = CreateTestData();

        var result = data.OrderBy(Array.Empty<FiledOrderParam>()).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void OrderBy_WithFiledOrderParams_ShouldSkipInvalidProperty()
    {
        var data = CreateTestData();

        var orderParams = new[]
        {
            new FiledOrderParam { PropertyName = "NonExistent", IsAsc = true },
            new FiledOrderParam { PropertyName = "Age", IsAsc = true }
        };

        var result = data.OrderBy(orderParams).ToList();

        result.Select(x => x.Age).Should().BeInAscendingOrder();
    }

    #endregion

    #region PagedBy (GetPageRequest)

    [Fact]
    public void PagedBy_WithGetPageRequest_ShouldReturnCorrectPage()
    {
        var data = CreateTestData();

        var result = data.PagedBy(new GetPageRequest(2, 2)).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(3);
        result[1].Id.Should().Be(4);
    }

    [Fact]
    public void PagedBy_WithGetPageRequest_ShouldReturnFirstPage()
    {
        var data = CreateTestData();

        var result = data.PagedBy(new GetPageRequest(1, 2)).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
    }

    [Fact]
    public void PagedBy_WithGetPageRequest_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.PagedBy(new GetPageRequest(1, 10));

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PagedBy_WithGetPageRequest_ShouldThrow_WhenPageContentIsNull()
    {
        var data = CreateTestData();

        var action = () => data.PagedBy((GetPageRequest)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region PagedBy (GetPageRequest, out int)

    [Fact]
    public void PagedBy_WithGetPageRequestAndOut_ShouldReturnCorrectTotalCount()
    {
        var data = CreateTestData();

        var result = data.PagedBy(new GetPageRequest(1, 2), out var totalCount).ToList();

        totalCount.Should().Be(5);
        result.Should().HaveCount(2);
    }

    [Fact]
    public void PagedBy_WithGetPageRequestAndOut_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.PagedBy(new GetPageRequest(1, 10), out _);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region PagedBy (int pageIndex, int pageSize)

    [Fact]
    public void PagedBy_WithIntParams_ShouldReturnCorrectPage()
    {
        var data = CreateTestData();

        var result = data.PagedBy(2, 2).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(3);
        result[1].Id.Should().Be(4);
    }

    [Fact]
    public void PagedBy_WithIntParams_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.PagedBy(1, 10);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PagedBy_WithIntParams_ShouldThrow_WhenPageIndexIsZero()
    {
        var data = CreateTestData();

        var action = () => data.PagedBy(0, 10);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PagedBy_WithIntParams_ShouldThrow_WhenPageSizeIsZero()
    {
        var data = CreateTestData();

        var action = () => data.PagedBy(1, 0);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PagedBy_WithIntParams_ShouldThrow_WhenPageIndexIsNegative()
    {
        var data = CreateTestData();

        var action = () => data.PagedBy(-1, 10);

        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region PagedBy (int pageIndex, int pageSize, out int)

    [Fact]
    public void PagedBy_WithIntParamsAndOut_ShouldReturnCorrectTotalCount()
    {
        var data = CreateTestData();

        var result = data.PagedBy(1, 2, out var totalCount).ToList();

        totalCount.Should().Be(5);
        result.Should().HaveCount(2);
    }

    [Fact]
    public void PagedBy_WithIntParamsAndOut_ShouldThrow_WhenQueryIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.PagedBy(1, 10, out _);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region QueryableWhereIf

    [Fact]
    public void QueryableWhereIf_ShouldFilter_WhenConditionIsTrue()
    {
        var data = CreateTestData();

        var result = data.QueryableWhereIf(true, x => x.Age > 28).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Age > 28);
    }

    [Fact]
    public void QueryableWhereIf_ShouldNotFilter_WhenConditionIsFalse()
    {
        var data = CreateTestData();

        var result = data.QueryableWhereIf(false, x => x.Age > 28).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void QueryableWhereIf_ShouldThrow_WhenSourceIsNull()
    {
        IQueryable<TestEntity>? source = null;

        var action = () => source!.QueryableWhereIf(true, x => x.Age > 0);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region WhereIfTrue

    [Fact]
    public void WhereIfTrue_ShouldFilter_WhenConditionIsTrue()
    {
        var data = CreateTestData();

        var result = data.WhereIfTrue(true, x => x.Age < 28).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Age < 28);
    }

    [Fact]
    public void WhereIfTrue_ShouldNotFilter_WhenConditionIsFalse()
    {
        var data = CreateTestData();

        var result = data.WhereIfTrue(false, x => x.Age < 28).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void WhereIfTrue_ShouldThrow_WhenSourceIsNull()
    {
        IQueryable<TestEntity>? source = null;

        var action = () => source!.WhereIfTrue(true, x => x.Age > 0);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region WhereIfNotNullOrWhiteSpace

    [Fact]
    public void WhereIfNotNullOrWhiteSpace_ShouldFilter_WhenValueIsNotNullOrWhiteSpace()
    {
        var data = CreateTestData();

        var result = data.WhereIfNotNullOrWhiteSpace("A", x => x.Name.StartsWith("A")).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void WhereIfNotNullOrWhiteSpace_ShouldNotFilter_WhenValueIsNull()
    {
        var data = CreateTestData();

        var result = data.WhereIfNotNullOrWhiteSpace(null, x => x.Name.StartsWith("A")).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void WhereIfNotNullOrWhiteSpace_ShouldNotFilter_WhenValueIsEmpty()
    {
        var data = CreateTestData();

        var result = data.WhereIfNotNullOrWhiteSpace("", x => x.Name.StartsWith("A")).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void WhereIfNotNullOrWhiteSpace_ShouldNotFilter_WhenValueIsWhiteSpace()
    {
        var data = CreateTestData();

        var result = data.WhereIfNotNullOrWhiteSpace("   ", x => x.Name.StartsWith("A")).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void WhereIfNotNullOrWhiteSpace_ShouldThrow_WhenSourceIsNull()
    {
        IQueryable<TestEntity>? source = null;

        var action = () => source!.WhereIfNotNullOrWhiteSpace("A", x => x.Name == "A");

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region WhereIfNotNull

    [Fact]
    public void WhereIfNotNull_ShouldFilter_WhenValueIsNotNull()
    {
        var data = CreateTestData();
        int? ageFilter = 30;

        var result = data.WhereIfNotNull(ageFilter, x => x.Age == ageFilter).ToList();

        result.Should().HaveCount(1);
        result[0].Age.Should().Be(30);
    }

    [Fact]
    public void WhereIfNotNull_ShouldNotFilter_WhenValueIsNull()
    {
        var data = CreateTestData();
        int? ageFilter = null;

        var result = data.WhereIfNotNull(ageFilter, x => x.Age == 0).ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void WhereIfNotNull_ShouldThrow_WhenSourceIsNull()
    {
        IQueryable<TestEntity>? source = null;

        var action = () => source!.WhereIfNotNull(1, x => x.Age == 1);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region EqualWhere

    [Fact]
    public void EqualWhere_ShouldFilterByEqualCondition()
    {
        var data = CreateTestData();

        var result = data.EqualWhere("Age", 30).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Alice");
    }

    #endregion

    #region LessWhere

    [Fact]
    public void LessWhere_ShouldFilterByLessThanCondition()
    {
        var data = CreateTestData();

        var result = data.LessWhere("Age", 28).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Age < 28);
    }

    #endregion

    #region GreaterWhere

    [Fact]
    public void GreaterWhere_ShouldFilterByGreaterThanCondition()
    {
        var data = CreateTestData();

        var result = data.GreaterWhere("Age", 28).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Age > 28);
    }

    #endregion

    #region WhereAny

    [Fact]
    public void WhereAny_ShouldCombinePredicatesWithOr()
    {
        var data = CreateTestData();

        var result = data.WhereAny(x => x.Age < 22, x => x.Age > 33).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "David");
        result.Should().Contain(x => x.Name == "Charlie");
    }

    [Fact]
    public void WhereAny_ShouldApplySinglePredicate()
    {
        var data = CreateTestData();

        var result = data.WhereAny(x => x.Age == 30).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void WhereAny_ShouldReturnEmpty_WhenNoPredicates()
    {
        var data = CreateTestData();

        var result = data.WhereAny().ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void WhereAny_ShouldThrow_WhenQueryableIsNull()
    {
        IQueryable<TestEntity>? query = null;

        var action = () => query!.WhereAny(x => x.Age > 0);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhereAny_ShouldThrow_WhenPredicatesIsNull()
    {
        var data = CreateTestData();

        var action = () => data.WhereAny(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CountBy

    [Fact]
    public void CountBy_ShouldReturnCorrectCount()
    {
        var data = CreateTestData();

        var result = data.CountBy(out var totalCount).ToList();

        totalCount.Should().Be(5);
        result.Should().HaveCount(5);
    }

    [Fact]
    public void CountBy_ShouldReturnZero_WhenEmpty()
    {
        var data = new List<TestEntity>().AsQueryable();

        var result = data.CountBy(out var totalCount).ToList();

        totalCount.Should().Be(0);
        result.Should().BeEmpty();
    }

    #endregion
}
