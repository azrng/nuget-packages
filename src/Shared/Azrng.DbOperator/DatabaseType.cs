namespace Azrng.DbOperator
{
    /// <summary>
    /// 数据库类型枚举，定义了项目支持的所有数据库类型
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
        /// Oracle 数据库
        /// </summary>
        Oracle,

        /// <summary>
        /// PostgreSQL 数据库
        /// </summary>
        PostgresSql,

        /// <summary>
        /// SQLite 数据库
        /// </summary>
        SqLite,

        /// <summary>
        /// ClickHouse 数据库（列式数据库）
        /// </summary>
        ClickHouse
    }
}