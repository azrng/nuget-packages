using Dapper;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class SqliteDbHelper : DbHelperBase
    {
        private static string ConnectionStringFormat = "Data Source={0};";

        public SqliteDbHelper(string connectionString) : base(connectionString) { }

        public SqliteDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = string.Format(ConnectionStringFormat, dataSourceConfig.DbName);
        }

        protected override DbConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
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
            return new SqliteParameter($"@{key}", value);
        }
    }
}