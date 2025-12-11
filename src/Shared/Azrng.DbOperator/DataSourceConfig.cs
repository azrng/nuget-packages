namespace Azrng.DbOperator
{
    public class DataSourceConfig
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType Type { get; set; }

        /// <summary>
        /// 数据库主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 数据库端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 数据库用户名
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 数据库密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否是否是utc时间
        /// </summary>
        public bool TimeIsUtc { get; set; }

        /// <summary>
        /// 数值类是否保存两位小数
        /// </summary>
        public bool DecimalIsTwo { get; set; }
    }
}