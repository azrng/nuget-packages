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
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static int ExecuteSqlCommand(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, out var openedByUs, parameters);
            using (command)
            {
                try
                {
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    if (openedByUs)
                        conn.Close();
                }
            }
        }

        /// <summary>
        /// 返回受影响行数
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static async Task<int> ExecuteSqlCommandAsync(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, out var openedByUs, parameters);
            using (command)
            {
                try
                {
                    return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                finally
                {
                    if (openedByUs)
                        await conn.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static object ExecuteScalar(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, out var openedByUs, parameters);
            using (command)
            {
                try
                {
                    return command.ExecuteScalar();
                }
                finally
                {
                    if (openedByUs)
                        conn.Close();
                }
            }
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static async Task<object> ExecuteScalarAsync(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, out var openedByUs, parameters);
            using (command)
            {
                try
                {
                    return await command.ExecuteScalarAsync().ConfigureAwait(false);
                }
                finally
                {
                    if (openedByUs)
                        await conn.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 查询并返回DataTable
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static DataTable SqlQueryDataTable(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, out var openedByUs, parameters);
            using (command)
            {
                try
                {
                    using var reader = command.ExecuteReader();
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
                finally
                {
                    if (openedByUs)
                        conn.Close();
                }
            }
        }

        /// <summary>
        /// 查询并返回DataTable
        /// </summary>
        /// <param name="facade">db.Database</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static async Task<DataTable> SqlQueryDataTableAsync(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out var conn, out var openedByUs, parameters);
            using (command)
            {
                try
                {
                    await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
                finally
                {
                    if (openedByUs)
                        await conn.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 查询并返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="facade"></param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
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
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数，必须为 DbParameter 类型</param>
        /// <returns></returns>
        public static async Task<List<T>> SqlQueryListAsync<T>(this DatabaseFacade facade, string sql, params object[] parameters)
            where T : class, new()
        {
            var dt = await facade.SqlQueryDataTableAsync(sql, parameters).ConfigureAwait(false);
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
        /// <param name="openedByUs">连接是否由本方法打开（仅此时调用方负责关闭）</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection connection,
                                               out bool openedByUs, params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            connection = conn;

            // 连接可能已由 EF 打开（如事务中），重复 Open 会抛异常；谁打开谁负责关闭
            openedByUs = conn.State != ConnectionState.Open;
            if (openedByUs)
                conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = sql;
            foreach (var parameter in parameters)
            {
                // 原始值无法绑定到 DbCommand，提前抛出清晰异常，避免使用方退化为拼接 SQL
                if (parameter is not DbParameter dbParameter)
                    throw new ArgumentException(
                        $"SQL 参数必须是 DbParameter 类型，实际传入 {parameter.GetType().Name}，请使用参数化查询", nameof(parameters));

                cmd.Parameters.Add(dbParameter);
            }

            return cmd;
        }

        #endregion 私有方法
    }
}
