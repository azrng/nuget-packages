using Xunit.Abstractions;

namespace Common.Core.Test.Helper
{
    public class CollectionHelperTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CollectionHelperTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task BatchForAsync_Ok()
        {
            var num = 1998;
            var total = await CollectionHelper.ExecuteInBatchesAsync(num, currentBatchSize =>
            {
                _testOutputHelper.WriteLine($"当前批次总数 ：{currentBatchSize}");

                var result = 10;
                return Task.FromResult(20);
            });
            _testOutputHelper.WriteLine($"结果：{total}");
        }

        [Fact]
        public void BatchFor_Ok()
        {
            var num = 1998;
            var total = CollectionHelper.ExecuteInBatches(num, currentBatchSize =>
            {
                _testOutputHelper.WriteLine($"当前批次总数 ：{currentBatchSize}");

                var result = 10;
                return result;
            });
            _testOutputHelper.WriteLine($"结果：{total}");
        }

        /// <summary>
        /// 随机从集合中移除指定元素
        /// </summary>
        [Fact]
        public void RemoveRandomItem_Ok()
        {
            var numbers = new List<int>
                          {
                              1,
                              2,
                              3,
                              4,
                              5,
                              6,
                              7,
                              8,
                              9,
                              10
                          };
            _testOutputHelper.WriteLine("原始集合: " + string.Join(", ", numbers));

            var remaining = CollectionHelper.RemoveRandomItem(numbers, 3);
            _testOutputHelper.WriteLine("剩余的元素: " + string.Join(", ", remaining));
            _testOutputHelper.WriteLine("原集合未被修改: " + string.Join(", ", numbers));
        }

        /// <summary>
        /// 执行集合分页处理 - 空集合
        /// </summary>
        [Fact]
        public async Task ProcessCollectionInPagesAsync_EmptyCollection_ReturnsZero()
        {
            // Arrange
            var emptyCollection = new List<int>();

            // Act
            var result = await CollectionHelper.ExecuteCollectionInPagesAsync(emptyCollection,
                (items, page) => Task.FromResult(items.Count));

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// 集合分页处理 - 无效的每页大小（0或负数），应处理为一次性处理所有数据
        /// </summary>
        [Fact]
        public async Task ProcessCollectionInPagesAsync_InvalidPageSize_ProcessesAllAtOnce()
        {
            // Arrange
            var collection = new List<int>
                             {
                                 1,
                                 2,
                                 3,
                                 4,
                                 5
                             };
            var processedItems = new List<int>();

            // Act
            var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection,
                (items, page) =>
                {
                    processedItems.AddRange(items);
                    _testOutputHelper.WriteLine($"处理第{page}页，数据量：{items.Count}");
                    return Task.FromResult(items.Count);
                },
                pageSize: 0);

            // Assert
            Assert.Equal(collection.Count, result);
            Assert.Equal(collection.Count, processedItems.Count);
            Assert.Equal(collection, processedItems);
        }

        [Theory]
        [InlineData(2)] // 每页2条，应该分3页
        [InlineData(3)] // 每页3条，应该分2页
        public async Task ProcessCollectionInPagesAsync_ValidPageSize_ProcessesCorrectly(int pageSize)
        {
            // Arrange
            var collection = new List<int>
                             {
                                 1,
                                 2,
                                 3,
                                 4,
                                 5
                             };
            var processedPages = new List<int>();
            var processedItems = new List<int>();

            // Act
            var result = await CollectionHelper.ExecuteCollectionInPagesAsync(collection,
                (items, page) =>
                {
                    processedPages.Add(page);
                    processedItems.AddRange(items);
                    _testOutputHelper.WriteLine($"处理第{page}页，数据量：{items.Count}");
                    return Task.FromResult(items.Count);
                },
                pageSize);

            // Assert
            Assert.Equal(collection.Count, result);
            Assert.Equal(collection.Count, processedItems.Count);
            Assert.Equal(collection, processedItems);
            Assert.Equal((collection.Count + pageSize - 1) / pageSize, processedPages.Count);
            Assert.Equal(1, processedPages[0]); // 确保页码从1开始
        }
    }
}