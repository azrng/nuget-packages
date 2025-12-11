using System;

namespace Azrng.EFCore
{
    /// <summary>
    /// EFCore连接配置
    /// </summary>
    public class EfCoreConnectOption
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 机器ID(生成标识ID使用)
        /// </summary>
        public int WorkId { get; set; } = new Random().Next(1, 1024);

        /// <summary>
        /// 是否蛇形命名
        /// </summary>
        public bool IsSnakeCaseNaming { get; set; } = true;

        /// <summary>
        /// 使用老的更新列
        /// </summary>
        public bool UseOldUpdateColumn { get; set; }

        /// <summary>
        /// 数据库schema
        /// </summary>
        public string Schema { get; set; }

        public void ParamVerify()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException(nameof(ConnectionString));
        }
    }
}