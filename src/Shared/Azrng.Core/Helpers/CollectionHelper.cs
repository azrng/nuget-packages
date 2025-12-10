using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 集合帮助类
    /// </summary>
    public class CollectionHelper
    {
        /// <summary>
        /// 对集合数据进行分页处理并执行操作
        /// </summary>
        /// <param name="collection">要处理的集合数据</param>
        /// <param name="processFunc">处理每页数据的函数，参数1：当前页数据，参数2：当前页码，返回值：处理的数据量</param>
        /// <param name="pageSize">每页数据量，默认5000条</param>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <returns>返回总处理数据量</returns>
        /// <remarks>
        /// 适用场景：当您有一个已有的集合数据需要分页处理时使用此方法，比如：
        /// - 批量处理大量实体数据并需要知道处理的页码
        /// - 需要对已有集合按固定数量分组处理
        /// </remarks>
        public static async Task<int> ExecuteCollectionInPagesAsync<T>(ICollection<T> collection,
                                                                       Func<ICollection<T>, int, Task<int>> processFunc,
                                                                       int pageSize = 5000)
        {
            if (collection.Count == 0)
            {
                return 0;
            }

            if (pageSize <= 0)
            {
                return await processFunc(collection, 1);
            }

            var size = collection.Count;

            var total = 0;

            // 求总页数
            var pageCount = (size + (pageSize - 1)) / pageSize;
            ConsoleHelper.WriteInfoLine($"当前数据 每页：{pageSize}条  共：{pageCount}页");

            for (var i = 1; i <= pageCount; i++)
            {
                var currSize = collection.Skip(pageSize * (i - 1)).Take(pageSize).ToList();
                ConsoleHelper.WriteInfoLine($"当前是第{i}页 本页面数据条数：{currSize.Count}");
                total += await processFunc(currSize, i);
            }

            return total;
        }

        /// <summary>
        /// 按批次大小循环执行指定次数的操作
        /// </summary>
        /// <param name="totalNumber">需要处理的总数量</param>
        /// <param name="processFunc">处理每批数据的函数，参数：当前批次的大小，返回值：当前批次处理的数据量</param>
        /// <param name="batchSize">每批处理的最大数量，默认1000条</param>
        /// <returns>返回总处理数据量</returns>
        /// <remarks>
        /// 适用场景：当您需要循环执行指定次数的操作时使用此方法，比如：
        /// - 批量插入指定数量的数据
        /// - 需要分批处理总数量已知的任务
        /// </remarks>
        public static int ExecuteInBatches(int totalNumber, Func<int, int> processFunc, int batchSize = 1000)
        {
            var totalBatches = (totalNumber + batchSize - 1) / batchSize;

            var total = 0;
            for (var batch = 0; batch < totalBatches; batch++)
            {
                var currentBatchSize = Math.Min(batchSize, totalNumber - batch * batchSize);

                total += processFunc(currentBatchSize);

                ConsoleHelper.WriteInfoLine($"当前批次：{batch + 1}/{totalBatches}，插入数量：{currentBatchSize}");
            }

            return total;
        }

        /// <summary>
        /// 按批次大小循环执行指定次数的操作
        /// </summary>
        /// <param name="totalNumber">需要处理的总数量</param>
        /// <param name="processFunc">处理每批数据的函数，参数：当前批次的大小，返回值：当前批次处理的数据量</param>
        /// <param name="batchSize">每批处理的最大数量，默认1000条</param>
        /// <returns>返回总处理数据量</returns>
        /// <remarks>
        /// 适用场景：当您需要循环执行指定次数的操作时使用此方法，比如：
        /// - 批量插入指定数量的数据
        /// - 需要分批处理总数量已知的任务
        /// </remarks>
        public static async Task<int> ExecuteInBatchesAsync(int totalNumber, Func<int, Task<int>> processFunc, int batchSize = 1000)
        {
            var totalBatches = (totalNumber + batchSize - 1) / batchSize;

            var total = 0;
            for (var batch = 0; batch < totalBatches; batch++)
            {
                var currentBatchSize = Math.Min(batchSize, totalNumber - batch * batchSize);

                total += await processFunc(currentBatchSize);

                ConsoleHelper.WriteInfoLine($"当前批次：{batch + 1}/{totalBatches}，插入数量：{currentBatchSize}");
            }

            return total;
        }

        /// <summary>
        /// 从集合中随机删除指定数量的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">源集合</param>
        /// <param name="countToRemove">要删除的元素数量</param>
        /// <returns>剩余的元素集合</returns>
        public static ICollection<T> RemoveRandomItem<T>(ICollection<T> collection, int countToRemove)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (countToRemove < 0)
                throw new ArgumentOutOfRangeException(nameof(countToRemove), "删除数量不能为负数");

            if (countToRemove == 0)
                return collection.ToList(); // 返回副本而不是原集合

            var originalCount = collection.Count;

            if (countToRemove > originalCount)
                throw new ArgumentOutOfRangeException(nameof(countToRemove), "删除数量不能超过集合元素数量");

            // 创建集合的副本进行处理
            var workingList = collection.ToList();

            // 优化策略：根据要删除的元素数量选择不同的算法
            if (countToRemove <= originalCount / 2)
            {
                // 使用部分随机选择算法（适合删除少量元素）
                RemoveUsingPartialRandomSelection(workingList, countToRemove);
            }
            else
            {
                // 使用保留策略（适合删除大量元素）
                workingList = RemoveUsingKeepStrategy(workingList, countToRemove);
            }

            return workingList;
        }

        #region 私有方法

        /// <summary>
        /// 使用部分随机选择算法删除元素（适合删除少量元素）
        /// </summary>
        private static void RemoveUsingPartialRandomSelection<T>(List<T> list, int countToRemove)
        {
            // 使用 Fisher-Yates 洗牌算法的部分实现
            for (var i = 0; i < countToRemove; i++)
            {
                // 在剩余元素中随机选择一个索引
                var randomIndex = RandomGenerator.Random.Value.Next(list.Count - i);

                // 将选中的元素与当前未处理区域的末尾元素交换
                (list[randomIndex], list[list.Count - 1 - i]) = (list[list.Count - 1 - i], list[randomIndex]);
            }

            // 从列表末尾删除指定数量的元素
            list.RemoveRange(list.Count - countToRemove, countToRemove);
        }

        /// <summary>
        /// 使用保留策略删除元素（适合删除大量元素）
        /// </summary>
        private static List<T> RemoveUsingKeepStrategy<T>(List<T> list, int countToRemove)
        {
            var countToKeep = list.Count - countToRemove;
            var keptItems = new List<T>(countToKeep);

            // 随机选择要保留的元素
            for (var i = 0; i < countToKeep; i++)
            {
                var randomIndex = RandomGenerator.Random.Value.Next(list.Count - i);

                // 将选中的元素交换到前面
                var selectedItem = list[randomIndex];
                list[randomIndex] = list[list.Count - 1 - i];
                list[list.Count - 1 - i] = selectedItem;

                keptItems.Add(selectedItem);
            }

            return keptItems;
        }

        #endregion
    }
}