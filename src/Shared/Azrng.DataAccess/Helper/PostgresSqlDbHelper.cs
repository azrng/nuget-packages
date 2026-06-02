using Azrng.Core.Model;
using Dapper;
using Npgsql;
using System.Data.Common;

namespace Azrng.DataAccess.Helper
{
    public class PostgresSqlDbHelper : DbHelperBase
    {
        public PostgresSqlDbHelper(string connectionString) : base(connectionString) { }

        public PostgresSqlDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = DataSourceConnectionStringBuilder.Build(DatabaseType.PostgresSql, dataSourceConfig);
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