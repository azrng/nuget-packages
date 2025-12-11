// using Common.Extension;
// using Microsoft.AspNetCore.Http;
//
// namespace Common.Helpers
// {
//     /// <summary>
//     /// Session帮助类
//     /// </summary>
//     public static class SessionHelper
//     {
//         /// <summary>
//         /// 设置Session
//         /// </summary>
//         /// <param name="key">键</param>
//         /// <param name="value">值</param>
//         public static void SetSession(string key, string value)
//         {
//             HttpContextManager.Current?.Session.SetString(key, value);
//         }
//
//         /// <summary>
//         /// 设置Session
//         /// </summary>
//         /// <param name="key">键</param>
//         /// <param name="value">值</param>
//         public static void SetSession<T>(string key, T value)
//         {
//             if (value == null)
//             {
//                 Remove(key);
//             }
//             else
//             {
//                 HttpContextManager.Current?.Session.SetString(key, value.ToJson());
//             }
//         }
//
//         /// <summary>
//         /// 获取Session
//         /// </summary>
//         /// <param name="key">键</param>
//         /// <returns>返回对应的值</returns>
//         public static string GetSession(string key)
//         {
//             var value = HttpContextManager.Current?.Session.GetString(key);
//             if (string.IsNullOrEmpty(value))
//                 value = string.Empty;
//             return value;
//         }
//
//         /// <summary>
//         /// 获取session
//         /// </summary>
//         /// <typeparam name="T">实体</typeparam>
//         /// <param name="key">键</param>
//         /// <returns></returns>
//         public static T? GetSession<T>(string key)
//         {
//             var value = HttpContextManager.Current?.Session.GetString(key);
//             return string.IsNullOrWhiteSpace(value) ? default : value.ToObject<T>();
//         }
//
//         /// <summary>
//         /// 移除session
//         /// </summary>
//         /// <param name="keys"></param>
//         public static void Remove(params string[] keys)
//         {
//             if (keys.Length == 0)
//             {
//                 return;
//             }
//
//             foreach (var key in keys)
//             {
//                 HttpContextManager.Current?.Session.Remove(key);
//             }
//         }
//     }
// }