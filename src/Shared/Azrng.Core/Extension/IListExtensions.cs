using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Azrng.Core.Extension
{
    public static class IListExtensions
    {
        /// <summary>
        /// IList To DataTable
        /// </summary>
        /// <param name="list"></param>
        /// <param name="hasColumns"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IList list, bool hasColumns = true)
        {
            var dataTable = new DataTable();
            if (list.Count <= 0)
                return dataTable;
            var properties = list[0]!.GetType().GetProperties();
            if (hasColumns)
            {
                foreach (var propertyInfo in properties)
                {
                    var type = propertyInfo.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = type.GetGenericArguments()[0];
                    }

                    dataTable.Columns.Add(new DataColumn(propertyInfo.Name, type));
                }
            }

            foreach (var t in list)
            {
                var arrayList = new ArrayList();
                foreach (var item in properties)
                {
                    var value = item.GetValue(t, null);
                    arrayList.Add(value);
                }

                var values = arrayList.ToArray();
                dataTable.LoadDataRow(values, fAcceptChanges: true);
            }

            return dataTable;
        }

        /// <summary>
        /// List To DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hasColumns"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this List<T> list, bool hasColumns = true)
        {
            var dataTable = new DataTable();
            var properties = typeof(T).GetProperties();
            if (hasColumns)
            {
                foreach (var propertyInfo in properties)
                {
                    var type = propertyInfo.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = type.GetGenericArguments()[0];
                    }

                    dataTable.Columns.Add(new DataColumn(propertyInfo.Name, type));
                }
            }

            if (list.Count == 0) return dataTable;

            foreach (var item in list)
            {
                var values = new object[properties.Length];

                for (var i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        /// <summary>
        ///将IList追加到其他DataTable
        /// </summary>
        /// <param name="list"></param>
        /// <param name="table"></param>
        /// <param name="hasColumns"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IList list, DataTable table, bool hasColumns = true)
        {
            table ??= new DataTable();

            if (list.Count <= 0) return table;
            var properties = list[0]!.GetType().GetProperties();
            if (hasColumns)
            {
                foreach (var propertyInfo in properties)
                {
                    var type = propertyInfo.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = type.GetGenericArguments()[0];
                    }

                    table.Columns.Add(new DataColumn(propertyInfo.Name, type));
                }
            }

            foreach (var t in list)
            {
                var arrayList = new ArrayList();
                foreach (var item in properties)
                {
                    var value = item.GetValue(t, null);
                    arrayList.Add(value);
                }

                table.LoadDataRow(arrayList.ToArray(), fAcceptChanges: true);
            }

            return table;
        }

        /// <summary>
        /// 将List追加到其他DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="table"></param>
        /// <param name="hasColumns"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this List<T> list, DataTable table, bool hasColumns = true)
        {
            table ??= new DataTable();

            var properties = typeof(T).GetProperties();
            if (hasColumns)
            {
                foreach (var propertyInfo in properties)
                {
                    var type = propertyInfo.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = type.GetGenericArguments()[0];
                    }

                    table.Columns.Add(new DataColumn(propertyInfo.Name, type));
                }
            }

            if (list.Count <= 0)
            {
                return table;
            }

            foreach (var t in list)
            {
                var arrayList = new ArrayList();
                foreach (var item in properties)
                {
                    var value = item.GetValue(t, null);
                    arrayList.Add(value);
                }

                var values = arrayList.ToArray();
                table.LoadDataRow(values, fAcceptChanges: true);
            }

            return table;
        }
    }
}