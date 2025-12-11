namespace Azrng.SqlMigration
{
    /// <summary>
    ///  迁移接口
    /// </summary>
    internal interface ISqlMigrationService
    {
        /// <summary>
        ///  迁移应用
        /// </summary>
        /// <param name="migrationName"></param>
        /// <returns></returns>
        Task<bool> MigrateAsync(string migrationName);
    }
}