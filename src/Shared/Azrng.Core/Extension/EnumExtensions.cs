using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 枚举扩展
    /// </summary>
    /// <remarks>获取枚举特性的缓存方案：https://notes.bassemweb.com/software/dotnet/get-enum-value-display-name.html</remarks>
    public static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<Enum, string> _descriptionCache = new();

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="enumItem"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumItem)
        {
            if (enumItem == null)
                throw new ArgumentNullException(nameof(enumItem));

            return _descriptionCache.GetOrAdd(enumItem, key =>
            {
                var fieldInfo = enumItem.GetType().GetField(enumItem.ToString());
                if (fieldInfo == null)
                    return string.Empty;

                var infos = GetDescriptionAttr(fieldInfo);
                if (infos is null || infos.Length == 0)
                {
                    return enumItem.ToString();
                }

                var des = infos[0];
                return des.Description;
            });
        }

        /// <summary>
        /// 获取枚举对应的自定义特性
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetCustomerObj<T>(this Enum value) where T : Attribute
        {
            return value.GetType().CustomAttributeCommon<T>(value.ToString());
        }

        #region 私有方法

        /// <summary>
        /// 获取字段Description
        /// </summary>
        /// <param name="fieldInfo">FieldInfo</param>
        /// <returns>DescriptionAttribute[] </returns>
        private static DescriptionAttribute[] GetDescriptionAttr(FieldInfo fieldInfo)
        {
            if (fieldInfo != null)
            {
                return (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            }

            return null;
        }

        #endregion 私有方法
    }
}