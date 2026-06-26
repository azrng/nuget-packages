using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class CollectionHelperTests
{
    #region ExecuteCollectionInPagesAsync

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_EmptyCollection_ReturnsZero()
    {
        var collection = new List<int>();

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) => Task.FromResult(page.Count));

        result.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_PageSizeZero_ProcessesAllAsOnePage()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };
        var processedPages = new List<int>();

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) =>
        {
            processedPages.Add(pageNum);
            return Task.FromResult(page.Count);
        }, pageSize: 0);

        result.Should().Be(5);
        processedPages.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_PageSizeNegative_ProcessesAllAsOnePage()
    {
        var collection = new List<int> { 1, 2, 3 };

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) =>
        {
            return Task.FromResult(page.Count);
        }, pageSize: -1);

        result.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_ExactPageDivision_ProcessesCorrectly()
    {
        var collection = Enumerable.Range(1, 10).ToList();
        var pages = new List<(List<int> items, int pageNum)>();

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) =>
        {
            pages.Add((page.ToList(), pageNum));
            return Task.FromResult(page.Count);
        }, pageSize: 5);

        result.Should().Be(10);
        pages.Should().HaveCount(2);
        pages[0].pageNum.Should().Be(1);
        pages[0].items.Should().Equal(1, 2, 3, 4, 5);
        pages[1].pageNum.Should().Be(2);
        pages[1].items.Should().Equal(6, 7, 8, 9, 10);
    }

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_UnevenPages_LastPageHasRemainder()
    {
        var collection = Enumerable.Range(1, 10).ToList();
        var pages = new List<(List<int> items, int pageNum)>();

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) =>
        {
            pages.Add((page.ToList(), pageNum));
            return Task.FromResult(page.Count);
        }, pageSize: 3);

        result.Should().Be(10);
        pages.Should().HaveCount(4);
        pages[0].items.Should().HaveCount(3);
        pages[1].items.Should().HaveCount(3);
        pages[2].items.Should().HaveCount(3);
        pages[3].items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_SingleElement_ReturnsProcessedCount()
    {
        var collection = new List<string> { "hello" };

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) =>
        {
            return Task.FromResult(page.Count);
        });

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteCollectionInPagesAsync_DefaultPageSize_ProcessesCorrectly()
    {
        var collection = Enumerable.Range(1, 100).ToList();
        var callCount = 0;

        var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection, (page, pageNum) =>
        {
            callCount++;
            return Task.FromResult(page.Count);
        });

        result.Should().Be(100);
        callCount.Should().Be(1);
    }

    #endregion

    #region ExecuteInBatches

    [Fact]
    public void ExecuteInBatches_ZeroTotal_ReturnsZero()
    {
        var result = CollectionHelper.ExecuteInBatches(0, batchSize => batchSize);

        result.Should().Be(0);
    }

    [Fact]
    public void ExecuteInBatches_ExactBatchDivision_ProcessesCorrectly()
    {
        var batches = new List<int>();

        var result = CollectionHelper.ExecuteInBatches(100, batchSize =>
        {
            batches.Add(batchSize);
            return batchSize;
        }, batchSize: 50);

        result.Should().Be(100);
        batches.Should().HaveCount(2);
        batches.Should().AllBeEquivalentTo(50);
    }

    [Fact]
    public void ExecuteInBatches_UnevenBatches_LastBatchHasRemainder()
    {
        var batches = new List<int>();

        var result = CollectionHelper.ExecuteInBatches(100, batchSize =>
        {
            batches.Add(batchSize);
            return batchSize;
        }, batchSize: 30);

        result.Should().Be(100);
        batches.Should().HaveCount(4);
        batches[0].Should().Be(30);
        batches[1].Should().Be(30);
        batches[2].Should().Be(30);
        batches[3].Should().Be(10);
    }

    [Fact]
    public void ExecuteInBatches_SingleBatch_ProcessesAll()
    {
        var batches = new List<int>();

        var result = CollectionHelper.ExecuteInBatches(5, batchSize =>
        {
            batches.Add(batchSize);
            return batchSize;
        }, batchSize: 10);

        result.Should().Be(5);
        batches.Should().ContainSingle().Which.Should().Be(5);
    }

    [Fact]
    public void ExecuteInBatches_DefaultBatchSize_UsesDefault()
    {
        var callCount = 0;

        var result = CollectionHelper.ExecuteInBatches(2500, batchSize =>
        {
            callCount++;
            return batchSize;
        });

        result.Should().Be(2500);
        callCount.Should().Be(3);
    }

    #endregion

    #region ExecuteInBatchesAsync

    [Fact]
    public async Task ExecuteInBatchesAsync_ZeroTotal_ReturnsZero()
    {
        var result = await CollectionHelper.ExecuteInBatchesAsync(0, batchSize => Task.FromResult(batchSize));

        result.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteInBatchesAsync_ExactBatchDivision_ProcessesCorrectly()
    {
        var batches = new List<int>();

        var result = await CollectionHelper.ExecuteInBatchesAsync(100, batchSize =>
        {
            batches.Add(batchSize);
            return Task.FromResult(batchSize);
        }, batchSize: 50);

        result.Should().Be(100);
        batches.Should().HaveCount(2);
        batches.Should().AllBeEquivalentTo(50);
    }

    [Fact]
    public async Task ExecuteInBatchesAsync_UnevenBatches_LastBatchHasRemainder()
    {
        var batches = new List<int>();

        var result = await CollectionHelper.ExecuteInBatchesAsync(100, batchSize =>
        {
            batches.Add(batchSize);
            return Task.FromResult(batchSize);
        }, batchSize: 30);

        result.Should().Be(100);
        batches.Should().HaveCount(4);
        batches[3].Should().Be(10);
    }

    [Fact]
    public async Task ExecuteInBatchesAsync_SingleBatch_ProcessesAll()
    {
        var batches = new List<int>();

        var result = await CollectionHelper.ExecuteInBatchesAsync(5, batchSize =>
        {
            batches.Add(batchSize);
            return Task.FromResult(batchSize);
        }, batchSize: 10);

        result.Should().Be(5);
        batches.Should().ContainSingle().Which.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteInBatchesAsync_DefaultBatchSize_UsesDefault()
    {
        var callCount = 0;

        var result = await CollectionHelper.ExecuteInBatchesAsync(2500, batchSize =>
        {
            callCount++;
            return Task.FromResult(batchSize);
        });

        result.Should().Be(2500);
        callCount.Should().Be(3);
    }

    #endregion

    #region RemoveRandomItem

    [Fact]
    public void RemoveRandomItem_NullCollection_ThrowsArgumentNullException()
    {
        Action act = () => CollectionHelper.RemoveRandomItem<int>(null!, 1);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRandomItem_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        var collection = new List<int> { 1, 2, 3 };

        Action act = () => CollectionHelper.RemoveRandomItem(collection, -1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*不能为负数*");
    }

    [Fact]
    public void RemoveRandomItem_CountExceedsCollectionSize_ThrowsArgumentOutOfRangeException()
    {
        var collection = new List<int> { 1, 2, 3 };

        Action act = () => CollectionHelper.RemoveRandomItem(collection, 4);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*不能超过集合元素数量*");
    }

    [Fact]
    public void RemoveRandomItem_RemoveZero_ReturnsCopyOfOriginal()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };

        var result = CollectionHelper.RemoveRandomItem(collection, 0);

        result.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void RemoveRandomItem_RemoveZero_DoesNotModifyOriginal()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };

        CollectionHelper.RemoveRandomItem(collection, 0);

        collection.Should().HaveCount(5);
    }

    [Fact]
    public void RemoveRandomItem_RemoveSome_ReturnsCorrectCount()
    {
        var collection = Enumerable.Range(1, 100).ToList();

        var result = CollectionHelper.RemoveRandomItem(collection, 30);

        result.Should().HaveCount(70);
    }

    [Fact]
    public void RemoveRandomItem_RemoveSome_ContainsOnlyOriginalElements()
    {
        var collection = Enumerable.Range(1, 100).ToList();

        var result = CollectionHelper.RemoveRandomItem(collection, 30);

        result.Should().AllSatisfy(item => collection.Should().Contain(item));
    }

    [Fact]
    public void RemoveRandomItem_RemoveAll_ReturnsEmptyCollection()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };

        var result = CollectionHelper.RemoveRandomItem(collection, 5);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRandomItem_RemoveMoreThanHalf_UsesKeepStrategy()
    {
        var collection = Enumerable.Range(1, 10).ToList();

        var result = CollectionHelper.RemoveRandomItem(collection, 8);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(item => collection.Should().Contain(item));
    }

    [Fact]
    public void RemoveRandomItem_DoesNotModifyOriginalCollection()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };
        var originalCount = collection.Count;

        CollectionHelper.RemoveRandomItem(collection, 2);

        collection.Should().HaveCount(originalCount);
    }

    [Fact]
    public void RemoveRandomItem_RemoveOneFromSingleItem_ReturnsEmpty()
    {
        var collection = new List<int> { 42 };

        var result = CollectionHelper.RemoveRandomItem(collection, 1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRandomItem_RemoveOneFromMultiple_ReturnsCorrectCount()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };

        var result = CollectionHelper.RemoveRandomItem(collection, 1);

        result.Should().HaveCount(4);
        result.Should().AllSatisfy(item => collection.Should().Contain(item));
    }

    #endregion
}
