using System;

namespace Azrng.DataAccess
{
    /// <summary>
    /// 数据库帮助类扩展方法。
    /// </summary>
    public static class DbHelperExtensions
    {
        /// <summary>
        /// 获取脱敏后的连接字符串。
        /// </summary>
        /// <param name="dbHelper">数据库帮助类。</param>
        /// <returns>脱敏后的连接字符串。</returns>
        public static string GetMaskedConnectionString(this IDbHelper dbHelper)
        {
            ArgumentNullException.ThrowIfNull(dbHelper);

            return DataSourceConnectionStringBuilder.MaskConnectionString(dbHelper.ConnectionString);
        }
    }
}