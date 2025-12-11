namespace Azrng.SqlMigration
{
    /// <summary>
    /// 初始版本设置(用于中途集成sql迁移服务的项目)
    /// </summary>
    public interface IInitVersionSetter
    {
        /// <summary>
        /// 获取当前版本
        /// 在目标数据库中没有获取到版本信息时调用此函数获取项目的初始化版本
        /// </summary>
        /// <returns></returns>
        Task<string> GetCurrentVersionAsync();
    }
}
