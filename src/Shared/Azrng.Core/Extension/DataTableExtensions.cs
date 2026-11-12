using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// DataTable扩展
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// DataTable转Entity（多行时取第一行）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static T ToEntity<T>(this DataTable table) where T : new()
        {
            var val = new T();
            foreach (DataRow row in table.Rows)
            {
                FillEntity(row, val);
                //方法名为单数语义即取首行；原实现无 break 时多行会静默覆盖为最后一行
                break;
            }

            return val;
        }

        /// <summary>
        /// DataTable转List Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T>? ToEntities<T>(this DataTable? table) where T : new()
        {
            if (table == null)
            {
                return null;
            }

            var list = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                var val = new T();
                FillEntity(row, val);
                list.Add(val);
            }

            return list;
        }

        private static void FillEntity(DataRow row, object val)
        {
            foreach (var propertyInfo in val.GetType().GetProperties())
            {
                if (!row.Table.Columns.Contains(propertyInfo.Name) || DBNull.Value == row[propertyInfo.Name])
                {
                    continue;
                }

                var type = propertyInfo.PropertyType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = new NullableConverter(type).UnderlyingType;
                }

                object converted;
                if (type == typeof(Guid))
                {
                    // Guid 未实现 IConvertible，ChangeType 必抛
                    converted = Guid.Parse(row[propertyInfo.Name].ToString()!);
                }
                else if (type.IsEnum)
                {
                    converted = Enum.Parse(type, row[propertyInfo.Name].ToString()!, true);
                }
                else
                {
                    // 数字/日期解析随 CurrentCulture 漂移，统一 InvariantCulture
                    converted = Convert.ChangeType(row[propertyInfo.Name], type, CultureInfo.InvariantCulture);
                }

                propertyInfo.SetValue(val, converted, null);
            }
        }
    }
}