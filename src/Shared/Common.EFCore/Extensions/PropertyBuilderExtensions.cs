using Azrng.Core.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azrng.EFCore.Extensions
{
    /// <summary>
    /// PropertyBuilder扩展
    /// </summary>
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// 无时区时间（pgsql）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyBuilder"></param>
        /// <param name="isPgsql"></param>
        /// <returns></returns>
        public static PropertyBuilder<T> IsUnTimeZoneDateTime<T>(this PropertyBuilder<T> propertyBuilder, bool isPgsql = true)
        {
            return isPgsql ? propertyBuilder.HasColumnType("timestamp without time zone") : propertyBuilder;
        }

        /// <summary>
        /// 有时区的时间(pgsql)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyBuilder"></param>
        /// <param name="isPgsql"></param>
        /// <returns></returns>
        public static PropertyBuilder<T> IsTimeZoneDateTime<T>(this PropertyBuilder<T> propertyBuilder, bool isPgsql = true)
        {
            return isPgsql ? propertyBuilder.HasColumnType("timestamp with time zone") : propertyBuilder;
        }

        /// <summary>
        /// 设置列名
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <param name="columnType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PropertyBuilder<T> HasCustomerColumnName<T>(this PropertyBuilder<T> propertyBuilder, string columnType)
        {
            return columnType.IsNotNullOrWhiteSpace() ? propertyBuilder.HasColumnName(columnType) : propertyBuilder;
        }
    }
}