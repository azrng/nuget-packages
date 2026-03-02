namespace Azrng.Dapper
{
    /// <summary>
    /// Dapper 仓储配置项
    /// </summary>
    public sealed class DapperRepositoryOptions
    {
        /// <summary>
        /// 默认命令超时时间（秒）
        /// </summary>
        public int? DefaultCommandTimeout { get; set; }
    }
}
