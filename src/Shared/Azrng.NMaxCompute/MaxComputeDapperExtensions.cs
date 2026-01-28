// using System.Data;
// using Microsoft.Extensions.Logging;
// using NMaxCompute.Adapter;
// using NMaxCompute.Adapter.Ho;
//
// namespace NMaxCompute;
//
// /// <summary>
// /// MaxCompute Dapper 扩展方法
// /// </summary>
// public static class MaxComputeDapperExtensions
// {
//     /// <summary>
//     /// 查询并返回强类型列表
//     /// </summary>
//     /// <typeparam name="T">返回类型</typeparam>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>查询结果列表</returns>
//     public static IEnumerable<T> Query<T>(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         using var command = connection.CreateCommand();
//         command.CommandText = sql;
//         command.CommandTimeout = commandTimeout;
//
//         // 添加参数
//         AddParameters(command, param);
//
//         using var reader = command.ExecuteReader();
//         return MapToList<T>(reader);
//     }
//
//     /// <summary>
//     /// 查询并返回强类型列表（异步）
//     /// </summary>
//     /// <typeparam name="T">返回类型</typeparam>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>查询结果列表</returns>
//     public static async Task<IEnumerable<T>> QueryAsync<T>(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         return await Task.Run(() => connection.Query<T>(sql, param, commandTimeout));
//     }
//
//     /// <summary>
//     /// 查询并返回单个结果
//     /// </summary>
//     /// <typeparam name="T">返回类型</typeparam>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>查询结果</returns>
//     public static T? QueryFirstOrDefault<T>(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         return connection.Query<T>(sql, param, commandTimeout).FirstOrDefault();
//     }
//
//     /// <summary>
//     /// 查询并返回单个结果（异步）
//     /// </summary>
//     /// <typeparam name="T">返回类型</typeparam>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>查询结果</returns>
//     public static async Task<T?> QueryFirstOrDefaultAsync<T>(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         var results = await connection.QueryAsync<T>(sql, param, commandTimeout);
//         return results.FirstOrDefault();
//     }
//
//     /// <summary>
//     /// 执行 SQL 并返回受影响的行数
//     /// </summary>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>受影响的行数</returns>
//     public static int Execute(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         using var command = connection.CreateCommand();
//         command.CommandText = sql;
//         command.CommandTimeout = commandTimeout;
//
//         // 添加参数
//         AddParameters(command, param);
//
//         return command.ExecuteNonQuery();
//     }
//
//     /// <summary>
//     /// 执行 SQL 并返回受影响的行数（异步）
//     /// </summary>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>受影响的行数</returns>
//     public static async Task<int> ExecuteAsync(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         return await Task.Run(() => connection.Execute(sql, param, commandTimeout));
//     }
//
//     /// <summary>
//     /// 查询并返回标量值
//     /// </summary>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>标量值</returns>
//     public static object? ExecuteScalar(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         using var command = connection.CreateCommand();
//         command.CommandText = sql;
//         command.CommandTimeout = commandTimeout;
//
//         // 添加参数
//         AddParameters(command, param);
//
//         return command.ExecuteScalar();
//     }
//
//     /// <summary>
//     /// 查询并返回标量值（异步）
//     /// </summary>
//     /// <param name="connection">连接</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <param name="param">参数</param>
//     /// <param name="commandTimeout">命令超时时间（秒）</param>
//     /// <returns>标量值</returns>
//     public static async Task<object?> ExecuteScalarAsync(
//         this MaxComputeConnection connection,
//         string sql,
//         object? param = null,
//         int commandTimeout = 30)
//     {
//         return await Task.Run(() => connection.ExecuteScalar(sql, param, commandTimeout));
//     }
//
//     /// <summary>
//     /// 添加参数到命令
//     /// </summary>
//     private static void AddParameters(IDbCommand command, object? param)
//     {
//         if (param == null)
//             return;
//
//         if (command is not MaxComputeCommand maxComputeCommand)
//             return;
//
//         var properties = param.GetType().GetProperties();
//         foreach (var prop in properties)
//         {
//             var value = prop.GetValue(param);
//             maxComputeCommand.Parameters.Add(prop.Name, value);
//         }
//     }
//
//     /// <summary>
//     /// 将 DataReader 映射到列表
//     /// </summary>
//     private static List<T> MapToList<T>(IDataReader reader)
//     {
//         var results = new List<T>();
//         var properties = typeof(T).GetProperties();
//
//         while (reader.Read())
//         {
//             var item = Activator.CreateInstance<T>();
//             var fieldCount = reader.FieldCount;
//
//             for (int i = 0; i < fieldCount; i++)
//             {
//                 var fieldName = reader.GetName(i);
//                 var property = properties.FirstOrDefault(p =>
//                     string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));
//
//                 if (property != null && property.CanWrite && !reader.IsDBNull(i))
//                 {
//                     var value = reader.GetValue(i);
//                     try
//                     {
//                         // 处理可空类型
//                         var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
//                         var convertedValue = Convert.ChangeType(value, targetType);
//                         property.SetValue(item, convertedValue);
//                     }
//                     catch
//                     {
//                         // 如果转换失败，尝试直接设置
//                         if (value?.GetType() == property.PropertyType ||
//                             value?.GetType() == typeof(Nullable<>).MakeGenericType(property.PropertyType))
//                         {
//                             property.SetValue(item, value);
//                         }
//                     }
//                 }
//             }
//
//             results.Add(item);
//         }
//
//         return results;
//     }
// }
//
// /// <summary>
// /// MaxCompute 适配器 Dapper 扩展方法
// /// </summary>
// public static class MaxComputeAdapterExtensions
// {
//     /// <summary>
//     /// 查询并返回强类型列表
//     /// </summary>
//     /// <typeparam name="T">返回类型</typeparam>
//     /// <param name="adapter">适配器</param>
//     /// <param name="querySqlBase">查询配置</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <returns>查询结果列表</returns>
//     public static async Task<IEnumerable<T>> QueryAsync<T>(
//         this IMaxComputeAdapter adapter,
//         QuerySqlBase querySqlBase,
//         string sql)
//     {
//         if (adapter == null)
//             throw new ArgumentNullException(nameof(adapter));
//
//         if (querySqlBase == null)
//             throw new ArgumentNullException(nameof(querySqlBase));
//
//         if (string.IsNullOrWhiteSpace(sql))
//             throw new ArgumentNullException(nameof(sql));
//
//         var result = await adapter.QuerySingleSqlAsync(new QuerySingleSqlRequestHo(querySqlBase)
//         {
//             Sql = sql
//         });
//
//         return MapToList<T>(result);
//     }
//
//     /// <summary>
//     /// 查询并返回字典列表
//     /// </summary>
//     /// <param name="adapter">适配器</param>
//     /// <param name="querySqlBase">查询配置</param>
//     /// <param name="sql">SQL 语句</param>
//     /// <returns>查询结果字典列表</returns>
//     public static async Task<List<Dictionary<string, object?>>> QueryAsync(
//         this IMaxComputeAdapter adapter,
//         QuerySqlBase querySqlBase,
//         string sql)
//     {
//         if (adapter == null)
//             throw new ArgumentNullException(nameof(adapter));
//
//         if (querySqlBase == null)
//             throw new ArgumentNullException(nameof(querySqlBase));
//
//         if (string.IsNullOrWhiteSpace(sql))
//             throw new ArgumentNullException(nameof(sql));
//
//         var result = await adapter.QuerySingleSqlAsync(new QuerySingleSqlRequestHo(querySqlBase)
//         {
//             Sql = sql
//         });
//
//         var list = new List<Dictionary<string, object?>>();
//
//         if (result.Rows == null)
//             return list;
//
//         for (int row = 0; row < result.Rows.Length; row++)
//         {
//             var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
//             for (int col = 0; col < result.Columns.Length; col++)
//             {
//                 var columnName = result.Columns[col];
//                 var value = result.Rows[row][col];
//                 dict[columnName] = value;
//             }
//             list.Add(dict);
//         }
//
//         return list;
//     }
//
//     /// <summary>
//     /// 将 MaxCompute 响应映射到列表
//     /// </summary>
//     private static List<T> MapToList<T>(Adapter.Ho.QuerySingleSqlResponseHo result)
//     {
//         var results = new List<T>();
//         var properties = typeof(T).GetProperties();
//
//         if (result.Rows == null || result.Columns == null)
//             return results;
//
//         foreach (var row in result.Rows)
//         {
//             var item = Activator.CreateInstance<T>();
//
//             for (int i = 0; i < result.Columns.Length && i < row.Length; i++)
//             {
//                 var fieldName = result.Columns[i];
//                 var property = properties.FirstOrDefault(p =>
//                     string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));
//
//                 if (property != null && property.CanWrite && row[i] != null)
//                 {
//                     try
//                     {
//                         var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
//                         var convertedValue = Convert.ChangeType(row[i], targetType);
//                         property.SetValue(item, convertedValue);
//                     }
//                     catch
//                     {
//                         if (row[i]?.GetType() == property.PropertyType)
//                         {
//                             property.SetValue(item, row[i]);
//                         }
//                     }
//                 }
//             }
//
//             results.Add(item);
//         }
//
//         return results;
//     }
// }
