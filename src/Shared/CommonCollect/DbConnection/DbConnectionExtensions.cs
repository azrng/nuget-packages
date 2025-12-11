using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace CommonCollect.DbConnection
{
    internal static class DbConnectionExtensions
    {
        public static int ExecuteNonQuery(this IDbConnection connection, string sql, IDbTransaction transaction = null,
            params object[] sqlParams)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using var dbCommand = connection.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = sql;
            foreach (object value in sqlParams)
            {
                dbCommand.Parameters.Add(value);
            }

            if (transaction != null)
            {
                dbCommand.Transaction = transaction;
            }

            return dbCommand.ExecuteNonQuery();
        }

        public static T ExecuteReader<T>(this IDbConnection connection, string sql, Func<IDataReader, T> readerFunc,
            params object[] sqlParams)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using var dbCommand = connection.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = sql;
            foreach (object value in sqlParams)
            {
                dbCommand.Parameters.Add(value);
            }

            var arg = dbCommand.ExecuteReader();
            T result = default;
            if (readerFunc != null)
            {
                return readerFunc(arg);
            }

            return result;
        }

        public static List<T> ExecuteReader<T>(this IDbConnection connection, string sql, params object[] sqlParams)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = sql;
            foreach (object value in sqlParams)
            {
                dbCommand.Parameters.Add(value);
            }

            return dbCommand.ExecuteReader().ConvertReaderToList<T>();
        }

        public static T ExecuteScalar<T>(this IDbConnection connection, string sql, params object[] sqlParams)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = sql;
            foreach (object value in sqlParams)
            {
                dbCommand.Parameters.Add(value);
            }

            object obj = dbCommand.ExecuteScalar();
            T result = default;
            if (obj != null)
            {
                Type typeFromHandle = typeof(T);
                TypeConverter converter = TypeDescriptor.GetConverter(typeFromHandle);
                if (converter.CanConvertFrom(obj.GetType()))
                {
                    return (T)converter.ConvertFrom(obj);
                }

                return (T)Convert.ChangeType(obj, typeFromHandle);
            }

            return result;
        }

        private static List<T> ConvertReaderToList<T>(this IDataReader objReader)
        {
            using (objReader)
            {
                List<T> list = new List<T>();
                Type typeFromHandle = typeof(T);
                while (objReader.Read())
                {
                    T val = Activator.CreateInstance<T>();
                    for (int i = 0; i < objReader.FieldCount; i++)
                    {
                        if (objReader[i] != null && !(objReader[i] is DBNull))
                        {
                            PropertyInfo property = typeFromHandle.GetProperty(objReader.GetName(i),
                                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public |
                                BindingFlags.GetProperty);
                            if (property != null)
                            {
                                property.SetValue(val, objReader[i].ConvertToSpecifyType(property.PropertyType), null);
                            }
                        }
                    }

                    list.Add(val);
                }

                return list;
            }
        }

        private static object ConvertToSpecifyType(this object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                {
                    return null;
                }

                conversionType = new NullableConverter(conversionType).UnderlyingType;
            }

            if (typeof(Enum).IsAssignableFrom(conversionType))
            {
                return Enum.Parse(conversionType, value.ToString());
            }

            return Convert.ChangeType(value, conversionType);
        }
    }
}