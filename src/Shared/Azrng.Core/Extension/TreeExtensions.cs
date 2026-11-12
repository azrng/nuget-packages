using Azrng.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// tree扩展
    /// </summary>
    public static class TreeExtensions
    {
        /// <summary>
        /// 从项目列表中生成项目树
        /// </summary>
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <typeparam name="K">Type of parent_id</typeparam>
        /// <param name="collection">Collection of items</param>
        /// <param name="idSelector">Function extracting item's id</param>
        /// <param name="parentIdSelector">Function extracting item's parent_id</param>
        /// <param name="rootId">Root element id</param>
        /// <remarks>内部先物化集合并按 parentId 分组索引，O(n) 构建；传入 IQueryable 等延迟源不会反复执行查询</remarks>
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(this IEnumerable<T> collection, Func<T, K> idSelector,
            Func<T, K> parentIdSelector, K? rootId = default)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (idSelector == null)
                throw new ArgumentNullException(nameof(idSelector));
            if (parentIdSelector == null)
                throw new ArgumentNullException(nameof(parentIdSelector));

            // 原实现每层递归都对原始集合重新 Where（O(n²)），延迟源会被反复执行且结果随枚举漂移
            // Dictionary 不允许 null 键，parentIdSelector 返回 null 的根节点单独归桶
            List<T>? rootItems = null;
            var byParent = new Dictionary<K, List<T>>();
            foreach (var item in collection)
            {
                var parentKey = parentIdSelector(item);
                if (parentKey == null)
                {
                    (rootItems ??= new List<T>()).Add(item);
                }
                else if (!byParent.TryGetValue(parentKey, out var bucket))
                {
                    byParent[parentKey] = bucket = new List<T>();
                    bucket.Add(item);
                }
                else
                {
                    bucket.Add(item);
                }
            }

            IEnumerable<TreeItem<T>> Build(K? parentKey)
            {
                List<T>? children;
                if (parentKey != null)
                {
                    children = byParent.TryGetValue(parentKey, out var found) ? found : null;
                }
                else
                {
                    // 与原实现一致：rootId 为 null 时仅取 parentId 同为 null 的项作为根
                    children = rootItems;
                }

                return children?.Select(CreateNode) ?? Enumerable.Empty<TreeItem<T>>();

                TreeItem<T> CreateNode(T item)
                {
                    return new TreeItem<T>
                    {
                        Item = item,
                        Children = Build(idSelector(item))
                    };
                }
            }

            return Build(rootId);
        }


        /// <summary>
        /// 获取树性结构中的所有数据
        /// </summary>
        /// <param name="items">树形结构</param>
        /// <param name="childSelector">根据当前节点返回子节点</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector)
            where T : class
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (childSelector == null)
                throw new ArgumentNullException(nameof(childSelector));
            var stack = new Stack<T>(items);
            var visited = new HashSet<T>(); // 用于防止循环依赖
            while (stack.Any())
            {
                var next = stack.Pop();
                if (next == null)
                    continue;

                // 检查是否已访问过，以避免循环依赖
                if (!visited.Add(next))
                    continue;

                yield return next;
                foreach (var child in childSelector(next))
                {
                    if (child is not null && !visited.Contains(child))
                        stack.Push(child);
                }
            }
        }
    }
}