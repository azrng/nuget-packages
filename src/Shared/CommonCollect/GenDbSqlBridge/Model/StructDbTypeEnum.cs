using System.ComponentModel;

namespace CommonCollect.GenDbSqlBridge.Model
{
    public enum StructDbTypeEnum
    {
        /// <summary>
        /// pg数据库
        /// </summary>
        [Description("PostgreSQL")] PG = 1,

        /// <summary>
        /// oracle数据库
        /// </summary>
        [Description("Oracle")] ORACLE = 3,

        /// <summary>
        /// sqlserver数据库
        /// </summary>
        [Description("SQL Server")] SQLSERVER = 4,

        /// <summary>
        /// mysql数据库
        /// </summary>
        [Description("Mysql")] MYSQL = 5,

        /// <summary>
        /// ClickHouse数据库
        /// </summary>
        [Description("ClickHouse")] CLICKHOUSE = 6
    }
}