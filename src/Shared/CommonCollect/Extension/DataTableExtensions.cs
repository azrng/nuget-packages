using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace CommonCollect.Extension
{
    /// <summary>
    /// DataTable扩展
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// 将List追加到其他DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="table"></param>
        /// <param name="hasColumns"></param>
        /// <returns></returns>
        [Obsolete]
        public static DataTable ToDataTable<T>(this List<T> list, DataTable table, bool hasColumns = true)
        {
            if (table == null)
            {
                table = new DataTable();
            }
            PropertyInfo[] properties = typeof(T).GetProperties();
            if (hasColumns)
            {
                foreach (PropertyInfo propertyInfo in properties)
                {
                    Type type = propertyInfo.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = type.GetGenericArguments()[0];
                    }
                    table.Columns.Add(new DataColumn(propertyInfo.Name, type));
                }
            }
            if (list.Count > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    ArrayList arrayList = new ArrayList();
                    PropertyInfo[] array = properties;
                    for (int i = 0; i < array.Length; i++)
                    {
                        object value = array[i].GetValue(list[j], null);
                        arrayList.Add(value);
                    }
                    object[] values = arrayList.ToArray();
                    table.LoadDataRow(values, fAcceptChanges: true);
                }
            }
            return table;
        }
    }
}