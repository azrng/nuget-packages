using Azrng.Core.Model;
using Azrng.DataAccess.DbBridge;
using System;

namespace Azrng.DataAccess
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
                DatabaseType.Sqlite => new SqliteBasicDbBridge(connectionString),
                DatabaseType.ClickHouse => new ClickHouseBasicDbBridge(connectionString),
                _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null)
            };
        }

        public static IBasicDbBridge CreateDbBridge(DataSourceConfig dataSourceConfig)
        {
            ArgumentNullException.ThrowIfNull(dataSourceConfig);

            return dataSourceConfig.Type switch
            {
                DatabaseType.MySql => new MySqlBasicDbBridge(dataSourceConfig),
                DatabaseType.SqlServer => new SqlServerBasicDbBridge(dataSourceConfig),
                DatabaseType.Oracle => new OracleBasicDbBridge(dataSourceConfig),
                DatabaseType.PostgresSql => new PostgreBasicDbBridge(dataSourceConfig),
                DatabaseType.Sqlite => new SqliteBasicDbBridge(dataSourceConfig),
                DatabaseType.ClickHouse => new ClickHouseBasicDbBridge(dataSourceConfig),
                _ => throw new ArgumentOutOfRangeException(nameof(dataSourceConfig.Type), dataSourceConfig.Type, null)
            };
        }
    }
}
