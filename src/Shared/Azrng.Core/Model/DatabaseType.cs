namespace Azrng.Core.Model
{
    /// <summary>
    /// 数据库类型（用于SQL方言选择和连接字符串构建）
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// MySQL 数据库
        /// </summary>
        MySql,

        /// <summary>
        /// SQL Server 数据库
        /// </summary>
        SqlServer,

        /// <summary>
        /// SQLite 数据库
        /// </summary>
        Sqlite,

        /// <summary>
        /// Oracle 数据库
        /// </summary>
        Oracle,

        /// <summary>
        /// PostgreSQL 数据库
        /// </summary>
        PostgresSql,

        /// <summary>
        /// 内存数据库（用于测试）
        /// </summary>
        InMemory,

        /// <summary>
        /// ClickHouse 数据库（列式OLAP数据库）
        /// </summary>
        ClickHouse
    }
}