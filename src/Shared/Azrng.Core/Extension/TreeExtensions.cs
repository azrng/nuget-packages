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
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(this IEnumerable<T> collection, Func<T, K> idSelector,
            Func<T, K> parentIdSelector, K rootId = default)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (idSelector == null)
                throw new ArgumentNullException(nameof(idSelector));
            if (parentIdSelector == null)
                throw new ArgumentNullException(nameof(parentIdSelector));
            var list = collection.Where(u =>
            {
                var selector = parentIdSelector(u);
                return rootId == null && selector == null || rootId != null && rootId.Equals(selector);
            });
            foreach (var c in list)
            {
                yield return new TreeItem<T>
                {
                    Item = c, Children = collection.GenerateTree(idSelector, parentIdSelector, idSelector(c))
                };
            }
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