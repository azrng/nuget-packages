namespace Azrng.Cache.FreeRedis
{
    public class RedisConfig
    {
        /// <summary>
        /// 连接字符串(localhost:6379,password=,defaultDatabase=1)
        /// </summary>
        public string ConnectionString { get; set; }

        ///// <summary>
        ///// 数据库实例
        ///// </summary>
        public int DefaultDataBase { get; set; }

        /// <summary>
        /// 实例名
        /// </summary>
        public string InstanceName { get; set; }
    }
}