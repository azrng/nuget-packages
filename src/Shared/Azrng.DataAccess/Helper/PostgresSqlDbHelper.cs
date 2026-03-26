using Dapper;
using Npgsql;
using System;
using System.Data.Common;

namespace Azrng.DataAccess.Helper
{
    public class PostgresSqlDbHelper : DbHelperBase
    {
        public PostgresSqlDbHelper(string connectionString) : base(connectionString) { }

        public PostgresSqlDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = dataSourceConfig.Host,
                Port = dataSourceConfig.Port,
                Database = dataSourceConfig.DbName,
                Username = dataSourceConfig.User,
                Password = dataSourceConfig.Password,
                PersistSecurityInfo = true,
                Pooling = dataSourceConfig.Pooling,
                MinPoolSize = Math.Max(0, dataSourceConfig.MinPoolSize),
                MaxPoolSize = Math.Max(dataSourceConfig.MinPoolSize, dataSourceConfig.MaxPoolSize),
                ConnectionIdleLifetime = 5,
                ConnectionPruningInterval = 5
            };

            ConnectionString = builder.ConnectionString;
        }

        protected override DbConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }

        public override async Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true)
        {
            var rows = new List<object[]>();

            await using var conn = GetConnection();
            await using var reader = await conn.ExecuteReaderAsync(sql, parameters).ConfigureAwait(false);

            if (header)
            {
                var columns = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                rows.Add(columns.ToArray());
            }

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var row = new object[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i];
                }

                rows.Add(row);
            }

            return rows.ToArray();
        }

        public override DbParameter SetParameter(string key, object value)
        {
            return new NpgsqlParameter($"@{key}", value);
        }
    }
}
