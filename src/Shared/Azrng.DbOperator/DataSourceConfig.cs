namespace Azrng.DbOperator
{
    public class DataSourceConfig
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DbName { get; set; } = string.Empty;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType Type { get; set; }

        /// <summary>
        /// 数据库主机
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 数据库端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 数据库用户名
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// 数据库密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 是否是UTC时间
        /// </summary>
        public bool TimeIsUtc { get; set; }

        /// <summary>
        /// 数值类是否保存两位小数
        /// </summary>
        public bool DecimalIsTwo { get; set; }

        /// <summary>
        /// 时区ID（用于 DateTime 转换）
        /// </summary>
        public string TimeZoneId { get; set; } = "Asia/Shanghai";

        /// <summary>
        /// 连接池启用
        /// </summary>
        public bool Pooling { get; set; }

        /// <summary>
        /// 最小连接池大小
        /// </summary>
        public int MinPoolSize { get; set; } = 5;

        /// <summary>
        /// 最大连接池大小
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Oracle 数据库用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;
    }
}