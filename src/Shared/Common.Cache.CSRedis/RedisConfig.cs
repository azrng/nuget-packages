namespace Common.Cache.CSRedis
{
    public class RedisConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /////// <summary>
        /////// 数据库实例
        /////// </summary>
        //public int DefaultDataBase { get; set; }

        /// <summary>
        /// 实例名
        /// </summary>
        public string InstanceName { get; set; }
    }
}
