namespace Azrng.SqlMigration
{
    /// <summary>
    /// 执行数据库操作
    /// </summary>
    public interface IDbVersionService
    {
        /// <summary>
        /// 获取当前的版本号
        /// </summary>
        /// <param name="migrationName">迁移数据库名字</param>
        /// <returns></returns>
        Task<string> GetCurrentVersionAsync(string migrationName);

        /// <summary>
        /// 记录版本升级历史
        /// </summary>
        /// <param name="migrationName">迁移数据库名字</param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task WriteVersionLogAsync(string migrationName, string version);
    }
}