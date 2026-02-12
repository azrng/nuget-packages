using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class OracleDbHelper : DbHelperBase
    {
        private const string ConnectionStringFormat = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));User Id={3};Password={4};";

        public OracleDbHelper(string connectionString) : base(connectionString) { }

        public OracleDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = string.Format(ConnectionStringFormat,
                dataSourceConfig.Host,
                dataSourceConfig.Port,
                dataSourceConfig.DbName,
                dataSourceConfig.UserId.IsNotNullOrWhiteSpace() ? dataSourceConfig.UserId : dataSourceConfig.User,
                dataSourceConfig.Password);
        }

        protected override DbConnection GetConnection()
        {
            return new OracleConnection(ConnectionString);
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
            return new OracleParameter($":{key}", value);
        }

        public override string BuildSplitPageSql(string sourceSql, int pageIndex, int pageSize,
                                                 string? orderColumn = null,
                                                 string? orderDirection = null)
        {
            var orderSql = " ORDER BY 1 ";
            if (orderColumn.IsNotNullOrWhiteSpace() && orderDirection.IsNotNullOrWhiteSpace())
                orderSql = $" ORDER BY {orderColumn} {orderDirection}";
            var sql = $"SELECT * FROM (SELECT A.*, ROW_NUMBER() OVER ({orderSql}) Reserved_Field_RowNumber FROM ({sourceSql}) A) WHERE Reserved_Field_RowNumber > {(pageIndex - 1) * pageSize} AND Reserved_Field_RowNumber <= {pageIndex * pageSize}";
            return sql;
        }
    }
}