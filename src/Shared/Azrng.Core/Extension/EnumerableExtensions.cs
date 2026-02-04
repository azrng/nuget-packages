using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// Enumerable扩展
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 检查集合是null或者空
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source?.Any() != true;
        }

        /// <summary>
        /// 检查集合不是null或者空
        /// </summary>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source?.Any() == true;
        }

        /// <summary>
        /// 带条件的where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> QueryableWhereIf<T>(this IQueryable<T> source, bool condition,
                                                        Expression<Func<T, bool>> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// 带条件的where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereIF<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ToPage<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            return source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 分页转为List
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ToPageList<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            return source.ToPage(pageIndex, pageSize).ToList();
        }

        /// <summary>
        /// 分页转为Array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ToPageArray<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            return source.ToPage(pageIndex, pageSize).ToArray();
        }

        /// <summary>
        /// 字典进行 ASCII排序
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static Dictionary<string, string> AsciiDictionary(this Dictionary<string, string> dic)
        {
            var array = dic.Keys.ToArray();
            Array.Sort(array, string.CompareOrdinal);

            return array.ToDictionary(key => key, key => dic[key]);
        }

        /// <summary>
        /// 循环并获取索引
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            var index = 0;
            foreach (var item in source) { yield return (item, index++); }
        }
    }
}