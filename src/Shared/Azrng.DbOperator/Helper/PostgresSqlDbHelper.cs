using Dapper;
using Npgsql;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class PostgresSqlDbHelper : DbHelperBase
    {
        /// <summary>
        /// 连接字符串格式
        /// </summary>
        private static string ConnectionStringFormat =>
            "host={0};port={1};database={2};username={3};password={4};PersistSecurityInfo=true;Maximum Pool Size=100;Connection Idle Lifetime=5;Connection Pruning Interval=5;";

        public PostgresSqlDbHelper(string connectionString) : base(connectionString) { }

        public PostgresSqlDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = string.Format(ConnectionStringFormat, dataSourceConfig.Host, dataSourceConfig.Port,
                dataSourceConfig.DbName, dataSourceConfig.User, dataSourceConfig.Password);
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