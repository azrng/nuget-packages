using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// DataTable扩展
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// DataTable转Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static T ToEntity<T>(this DataTable table) where T : new()
        {
            var val = new T();
            foreach (DataRow row in table.Rows)
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

                    propertyInfo.SetValue(val, Convert.ChangeType(row[propertyInfo.Name], type), null);
                }
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
                foreach (var propertyInfo in val.GetType().GetProperties())
                {
                    if (!table.Columns.Contains(propertyInfo.Name) || DBNull.Value == row[propertyInfo.Name])
                    {
                        continue;
                    }

                    var type = propertyInfo.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = new NullableConverter(type).UnderlyingType;
                    }

                    propertyInfo.SetValue(val, Convert.ChangeType(row[propertyInfo.Name], type), null);
                }

                list.Add(val);
            }

            return list;
        }
    }
}