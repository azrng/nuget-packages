using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Azrng.EFCore.Extensions
{
    /// <summary>
    /// EF扩展类执行任何SQL
    /// </summary>
    public static class EfCoreExtension
    {
        /// <summary>
        /// 返回受影响行数
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static int ExecuteSqlCommand(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var i = command.ExecuteNonQuery();
            conn.Close();
            return i;
        }

        /// <summary>
        /// 返回受影响行数
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<int> ExecuteSqlCommandAsync(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var i = await command.ExecuteNonQueryAsync();
            await conn.CloseAsync();
            return i;
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static object ExecuteScalar(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var i = command.ExecuteScalar();
            conn.Close();
            return i;
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> ExecuteScalarAsync(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var i = await command.ExecuteScalarAsync();
            await conn.CloseAsync();
            return i;
        }

        /// <summary>
        /// 查询并返回DataTable
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static DataTable SqlQueryDataTable(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var reader = command.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            conn.Close();
            return dt;
        }

        /// <summary>
        /// 查询并返回DataTable
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<DataTable> SqlQueryDataTableAsync(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, parameters);
            var reader = await command.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            await reader.CloseAsync();
            await conn.CloseAsync();
            return dt;
        }

        /// <summary>
        /// 查询并返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="facade"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static List<T> SqlQueryList<T>(this DatabaseFacade facade, string sql, params object[] parameters)
            where T : class, new()
        {
            var dt = facade.SqlQueryDataTable(sql, parameters);
            return dt.ToList<T>();
        }

        /// <summary>
        /// 查询并返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="facade"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<List<T>> SqlQueryListAsync<T>(this DatabaseFacade facade, string sql, params object[] parameters)
            where T : class, new()
        {
            var dt = await facade.SqlQueryDataTableAsync(sql, parameters);
            return dt.ToList<T>();
        }

        #region 私有方法

        /// <summary>
        /// DataTable类型转为List类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static List<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            var propertyInfos = typeof(T).GetProperties();
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var t = new T();
                foreach (var p in propertyInfos)
                {
                    if (dt.Columns.IndexOf(p.Name) != -1 && row[p.Name] != DBNull.Value)
                        p.SetValue(t, row[p.Name], null);
                }

                list.Add(t);
            }

            return list;
        }

        /// <summary>
        /// 创建命令
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="sql"></param>
        /// <param name="connection"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection connection,
                                               params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            connection = conn;
            conn.Open();
            var cmd = conn.CreateCommand();

            //提供了根据不同类型数据库的判断
            //facade.IsNpgsql()

            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        #endregion 私有方法
    }
}