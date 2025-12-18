using System;
using System.Collections.Generic;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 集合扩展
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 如果集合中不存在该项，则将其添加到集合中。
        /// </summary>
        /// <param name="source">集合</param>
        /// <param name="item">要检查并添加的 ItemDto</param>
        /// <typeparam name="T">集合中项的类型</typeparam>
        /// <returns>如果已添加，返回 True；否则返回 False。</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
#else
            if (source == null) throw new ArgumentNullException();
#endif

            if (source.Contains(item)) return false;

            source.Add(item);
            return true;
        }
    }
}