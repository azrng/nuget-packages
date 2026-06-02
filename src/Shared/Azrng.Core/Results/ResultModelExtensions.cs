using System;
using System.Collections.Generic;

namespace Azrng.Core.Results
{
    /// <summary>
    /// 结果模型扩展方法。
    /// </summary>
    public static class ResultModelExtensions
    {
        /// <summary>
        /// 获取列表数据；数据为空时返回空只读列表。
        /// </summary>
        /// <param name="result">结果模型。</param>
        /// <typeparam name="T">列表元素类型。</typeparam>
        /// <returns>列表数据。</returns>
        public static IReadOnlyList<T> DataOrEmpty<T>(this IResultModel<IReadOnlyList<T>> result)
        {
            return result.Data ?? Array.Empty<T>();
        }

        /// <summary>
        /// 获取列表数据；数据为空时返回空列表。
        /// </summary>
        /// <param name="result">结果模型。</param>
        /// <typeparam name="T">列表元素类型。</typeparam>
        /// <returns>列表数据。</returns>
        public static List<T> DataOrEmpty<T>(this IResultModel<List<T>> result)
        {
            return result.Data ?? new List<T>();
        }

        /// <summary>
        /// 获取数据；数据为空时返回默认值。
        /// </summary>
        /// <param name="result">结果模型。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <returns>数据或默认值。</returns>
        public static T DataOrDefault<T>(this IResultModel<T> result, T defaultValue)
        {
            return result.Data ?? defaultValue;
        }
    }
}
