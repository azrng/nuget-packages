using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class OracleDbHelper : DbHelperBase
    {
        public OracleDbHelper(string connectionString) : base(connectionString) { }

        public OracleDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig) { }

        protected override DbConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        public override Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true)
        {
            throw new NotImplementedException();
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
                orderSql = " ORDER BY " + orderColumn + " " + orderDirection;
            var sql = $@"SELECT * FROM
                        (
                            SELECT A.*, ROW_NUMBER() OVER ({orderSql}) Reserved_Field_RowNumber
                                FROM ({sourceSql}) A
                        )
                        WHERE Reserved_Field_RowNumber > {(pageIndex - 1) * pageSize} and Reserved_Field_RowNumber <= {pageIndex * pageSize}";

            return sql;
        }
    }
}