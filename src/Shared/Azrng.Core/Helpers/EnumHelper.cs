using Azrng.Core.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public class EnumHelper
    {
        /// <summary>
        /// 根据Description获取枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumValue<T>(string description)
        {
            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var curDesc = GetDescriptionAttr(field);
                if (curDesc?.Length > 0)
                {
                    if (curDesc[0].Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException($"{description} 未能找到对应的枚举.");
        }

        /// <summary>
        /// 枚举转字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary<T>() where T : Enum
        {
            var dic = new Dictionary<int, string>();
            if (!typeof(T).IsEnum)
            {
                return dic;
            }

            var values = Enum.GetValues(typeof(T));
            foreach (Enum item in values)
            {
                dic.Add(Convert.ToInt32(item), item.GetDescription());
            }

            return dic;
        }

        /// <summary>
        /// 获取枚举中所有的key
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static List<string> GetKeys<TEnum>()
        {
            var values = Enum.GetValues(typeof(TEnum));
            var list = new List<string>();
            foreach (Enum item in values)
            {
                list.Add(item.ToString());
            }

            return list;
        }

        /// <summary>
        /// 获取枚举所有的值
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static List<int> GetValues<TEnum>()
        {
            var values = Enum.GetValues(typeof(TEnum));
            var list = new List<int>();
            foreach (Enum item in values)
            {
                list.Add(Convert.ToInt32(item));
            }

            return list;
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

        /// <summary>
        /// 获取成员信息的 Description
        /// </summary>
        /// <param name="fieldInfo">成员信息</param>
        /// <returns>Description</returns>
        private static string GetDescription(MemberInfo fieldInfo)
        {
            var customAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), true);
            return customAttribute == null ? string.Empty : ((DescriptionAttribute)customAttribute).Description;
        }

        #endregion 私有方法
    }
}