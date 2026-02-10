using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 对象扩展
    /// </summary>
    public static class ObjectExtensions
    {
        // todo 待商定
        // /// <summary>
        // /// 深拷贝
        // /// </summary>
        // /// <typeparam name="T">原始类型</typeparam>
        // /// <param name="obj">原对象</param>
        // /// <returns></returns>
        // public static T Clone<T>(this T obj)
        //     where T : class
        // {
        //     return obj == null
        //         ? null
        //         : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        // }

        /// <summary>
        /// 参数拼接Url
        /// </summary>
        /// <param name="source">要拼接的实体</param>
        /// <param name="paramLower">参数是否要小写</param>
        /// <returns>Url,</returns>
        public static string ToUrlParameter<T>(this T? source, bool paramLower = false)
            where T : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var buff = new StringBuilder(string.Empty);
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                var value = property.GetValue(source);
                if (value == null)
                    continue;
                if (paramLower)
                {
                    buff.Append(property.Name.ToLowerInvariant() + "=" + value + "&");
                }
                else
                {
                    buff.Append(property.Name + "=" + value + "&");
                }
            }

            return buff.ToString().Trim('&');
        }

        /// <summary>
        /// 实体转字典
        /// </summary>
        /// <param name="source"></param>
        /// <param name="paramLower"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary<T>(this T source, bool paramLower = false)
            where T : class
        {
            var ret = new Dictionary<string, string>();

            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(source);
                if (propertyValue == null) continue;

                if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                {
                    var value = propertyValue.ToString();
                    var key = paramLower ? property.Name.ToLowerInvariant() : property.Name;
                    ret.Add(key, value);
                }
            }

            return ret;
        }

        #region To转换

        /// <summary>
        /// 检查输入对象值是否为空，为空返回默认值，否则将对象转换为输入类型
        /// </summary>
        /// <typeparam name="TFrom">源类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="from">待转换值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>返回值</returns>
        public static TTo To<TFrom, TTo>(this TFrom? from, TTo defaultValue)
        {
            if (from is null)
                return defaultValue;
            try
            {
                var type = typeof(TTo);

                //如目标类型为可空类型，则获取其基础类型作为转换目标类型
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                //如果目标类型为枚举，则使用枚举格式化方法
                if (type!.IsEnum)
                    return (TTo)Enum.Parse(type, from.ToString());
                return (TTo)Convert.ChangeType(from, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 检查输入对象值是否为空，为空返回default(TTo)，否则将对象转换为输入类型
        /// </summary>
        /// <typeparam name="TFrom">源类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="from">待转换值</param>
        /// <returns>返回值</returns>
        public static TTo? To<TFrom, TTo>(this TFrom from)
        {
            return from.To(default(TTo));
        }

        /// <summary>
        /// 检查输入对象值是否为空，为空返回默认值，否则将对象转换为输入类型
        /// </summary>
        /// <typeparam name="TTo">类型</typeparam>
        /// <param name="from">待转换值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>返回值</returns>
        public static TTo To<TTo>(this string? from, TTo defaultValue)
        {
            return from.To<string, TTo>(defaultValue);
        }

        /// <summary>
        /// 检查输入字符串是否为空，为空返回default(TTo)，否则将字符串转换为输入类型
        /// </summary>
        /// <typeparam name="TTo">类型</typeparam>
        /// <param name="from">待转换字符串</param>
        /// <returns>返回值</returns>
        public static TTo? To<TTo>(this string? from)
        {
            return from.To(default(TTo));
        }

        /// <summary>
        /// 获取对象的属性值，并转换成目标类型，获取或者转换失败，返回默认值
        /// </summary>
        /// <typeparam name="TFrom">对象的类型</typeparam>
        /// <typeparam name="TProperty">对象属性类型</typeparam>
        /// <typeparam name="TTo">转换类型</typeparam>
        /// <param name="from">对象</param>
        /// <param name="getProperty">属性获取委托</param>
        /// <param name="defaultValue"></param>
        /// <returns>转换结果</returns>
        public static TTo To<TFrom, TProperty, TTo>(
            this TFrom? from,
            Func<TFrom, TProperty> getProperty,
            TTo defaultValue
        )
        {
            if (from is null)
                return defaultValue;
            var v = getProperty(from);
            return v.To(defaultValue);
        }

        /// <summary>
        /// 获取对象的属性值，并转换成目标类型，获取或者转换失败，返回类型默认值
        /// </summary>
        /// <typeparam name="TFrom">对象的类型</typeparam>
        /// <typeparam name="TProperty">对象属性类型</typeparam>
        /// <typeparam name="TTo">转换类型</typeparam>
        /// <param name="from">对象</param>
        /// <param name="getProperty">属性获取委托</param>
        /// <returns>转换结果</returns>
        public static TTo? To<TFrom, TProperty, TTo>(
            this TFrom? from,
            Func<TFrom, TProperty> getProperty
        )
        {
            if (from is null)
                return default;
            var v = getProperty(from);
            return v.To(default(TTo));
        }

        /// <summary>
        /// 获取对象的属性值，并转换成目标类型，获取或者转换失败，返回类型默认值
        /// </summary>
        /// <typeparam name="TFrom">对象的类型</typeparam>
        /// <typeparam name="TTo">转换类型</typeparam>
        /// <param name="from">对象</param>
        /// <param name="getProperty">属性获取委托</param>
        /// <returns>转换结果</returns>
        public static TTo? To<TFrom, TTo>(this TFrom? from, Func<TFrom, object> getProperty)
        {
            return from.To<TFrom, object, TTo>(getProperty);
        }

        #endregion

        /// <summary>
        /// 对象转ExpandoObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        /// dynamic expando = new ExpandoObject();
        /// var name= expando.Name;
        /// </remarks>
        public static ExpandoObject ToExpandoObject<T>(this T obj) where T : class, new()
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                expando.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }
    }
}