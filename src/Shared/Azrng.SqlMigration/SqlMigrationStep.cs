namespace Azrng.SqlMigration
{
    /// <summary>
    /// 脚本迁移状态
    /// </summary>
    public enum SqlMigrationStep
    {
        /// <summary>
        /// 准备迁移
        /// </summary>
        Prepare,

        /// <summary>
        /// 准备迁移
        /// </summary>
        VersionUpdatePrepare,

        /// <summary>
        /// 迁移成功
        /// </summary>
        VersionUpdateSuccess,

        /// <summary>
        /// 迁移失败
        /// </summary>
        VersionUpdateFailed,

        /// <summary>
        /// 迁移成功
        /// </summary>
        Success,

        /// <summary>
        /// 迁移失败
        /// </summary>
        Failed
    }
}