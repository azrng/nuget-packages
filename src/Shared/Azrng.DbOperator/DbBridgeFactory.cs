using Azrng.DbOperator.DbBridge;

namespace Azrng.DbOperator
{
    public class DbBridgeFactory
    {
        public static IBasicDbBridge CreateDbBridge(DatabaseType dbType, string connectionString)
        {
            return dbType switch
            {
                DatabaseType.MySql => new MySqlBasicDbBridge(connectionString),
                DatabaseType.SqlServer => new SqlServerBasicDbBridge(connectionString),
                DatabaseType.Oracle => new OracleBasicDbBridge(connectionString),
                DatabaseType.PostgresSql => new PostgreBasicDbBridge(connectionString),
                DatabaseType.SqLite => throw new NotImplementedException("暂不支持"),
                DatabaseType.ClickHouse => new ClickHouseBasicDbBridge(connectionString),
                _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null)
            };
        }
    }
}