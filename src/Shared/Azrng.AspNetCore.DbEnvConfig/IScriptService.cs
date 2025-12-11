namespace Azrng.AspNetCore.DbEnvConfig
{
    /// <summary>
    /// 脚本服务(不支持依赖注入)
    /// </summary>
    public interface IScriptService
    {
        /// <summary>
        /// 获取初始化表脚本
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="field">字段</param>
        /// <param name="value">内容值</param>
        /// <param name="schema">模式</param>
        /// <returns></returns>
        string GetInitTableScript(string tableName, string field, string value, string? schema = null);

        /// <summary>
        /// 获取初始化表数据脚本(表没有数据的时候才会初始化)
        /// </summary>
        /// <returns></returns>
        string GetInitTableDataScript();
    }
}