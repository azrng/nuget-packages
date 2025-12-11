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

        public override Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true)
        {
            throw new NotImplementedException();
        }

        public override DbParameter SetParameter(string key, object value)
        {
            return new SqliteParameter($"@{key}", value);
        }
    }
}