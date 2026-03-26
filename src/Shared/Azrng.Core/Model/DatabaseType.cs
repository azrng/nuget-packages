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
        MySql = 0,

        /// <summary>
        /// SQL Server 数据库
        /// </summary>
        SqlServer = 1,

        /// <summary>
        /// SQLite 数据库
        /// </summary>
        Sqlite = 2,

        /// <summary>
        /// Oracle 数据库
        /// </summary>
        Oracle = 3,

        /// <summary>
        /// Postgresql 数据库
        /// </summary>
        PostgresSql = 4,

        /// <summary>
        /// 达梦数据库（DM）
        /// </summary>
        Dm = 5,

        /// <summary>
        /// 人大金仓数据库（KingbaseES）
        /// </summary>
        Kdbndp = 6,

        /// <summary>
        /// 神通数据库（Oscar）
        /// </summary>
        Oscar = 7,

        /// <summary>
        /// MySQL连接器（MySqlConnector）
        /// </summary>
        MySqlConnector = 8,

        /// <summary>
        /// Microsoft Access 数据库
        /// </summary>
        Access = 9,

        /// <summary>
        /// openGauss 数据库
        /// </summary>
        OpenGauss = 10,

        /// <summary>
        /// QuestDB 时序数据库
        /// </summary>
        QuestDB = 11,

        /// <summary>
        /// 瀚高数据库（HighGo）
        /// </summary>
        HG = 12,

        /// <summary>
        /// ClickHouse 列式数据库
        /// </summary>
        ClickHouse = 13,

        /// <summary>
        /// 南大通用 GBase 数据库
        /// </summary>
        GBase = 14,

        /// <summary>
        /// ODBC 通用连接
        /// </summary>
        Odbc = 15,

        /// <summary>
        /// OceanBase 数据库（Oracle 模式）
        /// </summary>
        OceanBaseForOracle = 16,

        /// <summary>
        /// TDengine 时序数据库
        /// </summary>
        TDengine = 17,

        /// <summary>
        /// 华为 GaussDB 数据库
        /// </summary>
        GaussDB = 18,

        /// <summary>
        /// OceanBase 数据库
        /// </summary>
        OceanBase = 19,

        /// <summary>
        /// TiDB 分布式数据库
        /// </summary>
        Tidb = 20,

        /// <summary>
        /// Vastbase 数据库
        /// </summary>
        Vastbase = 21,

        /// <summary>
        /// 阿里云 PolarDB 数据库
        /// </summary>
        PolarDB = 22,

        /// <summary>
        /// Apache Doris 数据库
        /// </summary>
        Doris = 23,

        /// <summary>
        /// 虚谷数据库
        /// </summary>
        Xugu = 24,

        /// <summary>
        /// 金篆信科 GoldenDB 数据库
        /// </summary>
        GoldenDB = 25,

        /// <summary>
        /// 腾讯云 TDSQL 数据库（PostgreSQL ODBC）
        /// </summary>
        TDSQLForPGODBC = 26,

        /// <summary>
        /// 腾讯云 TDSQL 数据库
        /// </summary>
        TDSQL = 27,

        /// <summary>
        /// SAP HANA 数据库
        /// </summary>
        HANA = 28,

        /// <summary>
        /// IBM DB2 数据库
        /// </summary>
        DB2 = 29,

        /// <summary>
        /// 华为 GaussDB 原生数据库
        /// </summary>
        GaussDBNative = 30,

        /// <summary>
        /// DuckDB 分析型数据库
        /// </summary>
        DuckDB = 31,

        /// <summary>
        /// MongoDB 文档数据库
        /// </summary>
        MongoDb = 32,

        /// <summary>
        /// 内存数据库（用于测试）
        /// </summary>
        InMemory = 800,

        /// <summary>
        /// 自定义数据库类型
        /// </summary>
        Custom = 900
    }
}