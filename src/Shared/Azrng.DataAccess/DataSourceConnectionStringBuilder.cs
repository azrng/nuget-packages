using Azrng.Core.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.Common;

namespace Azrng.DataAccess
{
    /// <summary>
    /// 数据源连接字符串构建与脱敏工具。
    /// </summary>
    public static class DataSourceConnectionStringBuilder
    {
        private const string MaskedValue = "***";

        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "Pwd",
            "User Id",
            "UserID",
            "Username",
            "User",
            "UID"
        };

        /// <summary>
        /// 根据数据源配置构建连接字符串。
        /// </summary>
        /// <param name="dataSourceConfig">数据源配置。</param>
        /// <returns>连接字符串。</returns>
        public static string Build(DataSourceConfig dataSourceConfig)
        {
            ArgumentNullException.ThrowIfNull(dataSourceConfig);

            return Build(dataSourceConfig.Type, dataSourceConfig);
        }

        /// <summary>
        /// 根据数据库类型和数据源配置构建连接字符串。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="dataSourceConfig">数据源配置。</param>
        /// <returns>连接字符串。</returns>
        public static string Build(DatabaseType databaseType, DataSourceConfig dataSourceConfig)
        {
            ArgumentNullException.ThrowIfNull(dataSourceConfig);

            return databaseType switch
            {
                DatabaseType.MySql => BuildMySql(dataSourceConfig),
                DatabaseType.SqlServer => BuildSqlServer(dataSourceConfig),
                DatabaseType.Oracle => BuildOracle(dataSourceConfig),
                DatabaseType.PostgresSql => BuildPostgresSql(dataSourceConfig),
                DatabaseType.Sqlite => BuildSqlite(dataSourceConfig),
                DatabaseType.ClickHouse => BuildClickHouse(dataSourceConfig),
                _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
            };
        }

        /// <summary>
        /// 脱敏连接字符串中的账号与密码字段。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <returns>脱敏后的连接字符串。</returns>
        public static string MaskConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return string.Empty;

            try
            {
                var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
                foreach (var key in builder.Keys.Cast<string>().Where(SensitiveKeys.Contains).ToList())
                {
                    builder[key] = MaskedValue;
                }

                return builder.ConnectionString;
            }
            catch (ArgumentException)
            {
                return MaskConnectionStringSegments(connectionString);
            }
        }

        private static string BuildMySql(DataSourceConfig dataSourceConfig)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = dataSourceConfig.Host,
                Port = (uint)Math.Max(0, dataSourceConfig.Port),
                Database = dataSourceConfig.DbName,
                UserID = dataSourceConfig.User,
                Password = dataSourceConfig.Password,
                Pooling = dataSourceConfig.Pooling,
                MinimumPoolSize = (uint)Math.Max(0, dataSourceConfig.MinPoolSize),
                MaximumPoolSize = (uint)Math.Max(dataSourceConfig.MinPoolSize, dataSourceConfig.MaxPoolSize),
                CharacterSet = "utf8"
            };

            return builder.ConnectionString;
        }

        private static string BuildSqlServer(DataSourceConfig dataSourceConfig)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSourceConfig.Port > 0 ? $"{dataSourceConfig.Host},{dataSourceConfig.Port}" : dataSourceConfig.Host,
                InitialCatalog = dataSourceConfig.DbName,
                UserID = dataSourceConfig.User,
                Password = dataSourceConfig.Password,
                Encrypt = false,
                Pooling = dataSourceConfig.Pooling,
                MinPoolSize = Math.Max(0, dataSourceConfig.MinPoolSize),
                MaxPoolSize = Math.Max(dataSourceConfig.MinPoolSize, dataSourceConfig.MaxPoolSize)
            };

            return builder.ConnectionString;
        }

        private static string BuildOracle(DataSourceConfig dataSourceConfig)
        {
            var builder = new OracleConnectionStringBuilder
            {
                DataSource =
                    $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={dataSourceConfig.Host})(PORT={dataSourceConfig.Port}))(CONNECT_DATA=(SERVICE_NAME={dataSourceConfig.DbName})))",
                UserID = dataSourceConfig.UserId.IsNotNullOrWhiteSpace() ? dataSourceConfig.UserId : dataSourceConfig.User,
                Password = dataSourceConfig.Password
            };

            return builder.ConnectionString;
        }

        private static string BuildPostgresSql(DataSourceConfig dataSourceConfig)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = dataSourceConfig.Host,
                Port = dataSourceConfig.Port,
                Database = dataSourceConfig.DbName,
                Username = dataSourceConfig.User,
                Password = dataSourceConfig.Password,
                PersistSecurityInfo = false,
                Pooling = dataSourceConfig.Pooling,
                MinPoolSize = Math.Max(0, dataSourceConfig.MinPoolSize),
                MaxPoolSize = Math.Max(dataSourceConfig.MinPoolSize, dataSourceConfig.MaxPoolSize),
                ConnectionIdleLifetime = 5,
                ConnectionPruningInterval = 5
            };

            return builder.ConnectionString;
        }

        private static string BuildSqlite(DataSourceConfig dataSourceConfig)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dataSourceConfig.DbName
            };

            return builder.ConnectionString;
        }

        private static string BuildClickHouse(DataSourceConfig dataSourceConfig)
        {
            var builder = new DbConnectionStringBuilder
            {
                ["Host"] = dataSourceConfig.Host,
                ["Port"] = dataSourceConfig.Port,
                ["Database"] = dataSourceConfig.DbName,
                ["User"] = dataSourceConfig.User,
                ["Password"] = dataSourceConfig.Password,
                ["Compress"] = true,
                ["CheckCompressedHash"] = false,
                ["Compressor"] = "lz4"
            };

            return builder.ConnectionString;
        }

        private static string MaskConnectionStringSegments(string connectionString)
        {
            var segments = connectionString.Split(';', StringSplitOptions.None);
            for (var i = 0; i < segments.Length; i++)
            {
                var separatorIndex = segments[i].IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = segments[i][..separatorIndex].Trim();
                if (SensitiveKeys.Contains(key))
                {
                    segments[i] = $"{segments[i][..(separatorIndex + 1)]}{MaskedValue}";
                }
            }

            return string.Join(';', segments);
        }
    }
}