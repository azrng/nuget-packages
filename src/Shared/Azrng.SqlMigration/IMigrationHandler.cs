namespace Azrng.SqlMigration
{
    /// <summary>
    /// 升级回调
    /// </summary>
    public interface IMigrationHandler
    {
        /// <summary>
        /// 准备迁移
        /// 函数异常会阻断升级
        /// </summary>
        /// <param name="oloVersion">版本号</param>
        /// <returns>返回是否执行该版本迁移，false则跳过此版本迁移</returns>
        Task<bool> BeforeMigrateAsync(string oloVersion);

        /// <summary>
        /// 当前版本准备迁移
        /// 函数异常会阻断升级
        /// </summary>
        /// <param name="version">版本号</param>
        /// <returns>返回是否执行该版本迁移，false则跳过此版本迁移</returns>
        Task<bool> VersionUpdateBeforeMigrateAsync(string version);

        /// <summary>
        /// 当前版本已迁移
        /// </summary>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        Task VersionUpdateMigratedAsync(string version);

        /// <summary>
        /// 当前版本迁移失败
        /// </summary>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        Task VersionUpdateMigrateFailedAsync(string version);

        /// <summary>
        /// 当前已迁移
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        Task MigratedAsync(string oldVersion, string version);

        /// <summary>
        /// 迁移失败
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        Task MigrateFailedAsync(string oldVersion, string version);
    }
}
