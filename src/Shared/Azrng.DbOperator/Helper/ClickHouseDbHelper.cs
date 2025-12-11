using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using ClickHouse.Client.Numerics;
using Dapper;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class ClickHouseDbHelper : DbHelperBase
    {
        private static string ConnectionStringFormat =>
            "Host={0};Port={1};Database={2};User={3};Password={4};Compress=True;CheckCompressedHash=False;Compressor=lz4;";

        public ClickHouseDbHelper(string connectionString) : base(connectionString) { }

        public ClickHouseDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = string.Format(ConnectionStringFormat, dataSourceConfig.Host, dataSourceConfig.Port,
                dataSourceConfig.DbName, dataSourceConfig.User, dataSourceConfig.Password);
        }

        protected override DbConnection GetConnection()
        {
            return new ClickHouseConnection(ConnectionString);
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
                    row[i] = ConvertReaderValueObject(reader[i], reader.GetFieldType(i));
                }

                rows.Add(row);
            }

            return rows.ToArray();
        }

        public override DbParameter SetParameter(string key, object value)
        {
            return new ClickHouseDbParameter { ParameterName = key, Value = value };
        }

        /// <summary>
        /// 转换读取的对象
        /// </summary>
        /// <param name="readerValue"></param>
        /// <param name="readerValueType"></param>
        /// <returns></returns>
        public override object? ConvertReaderValueObject(object? readerValue, Type readerValueType)
        {
            if (readerValue == null)
                return null;

            if (readerValueType == typeof(DateTime))
            {
                if (DataSourceConfig.TimeIsUtc)
                {
                    return TimeZoneInfo
                           .ConvertTimeFromUtc(Convert.ToDateTime(readerValue), TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"))
                           .ToStandardString();
                }
                else
                {
                    return readerValue.ToString().ToDateTime().ToStandardString();
                }
            }
            else if (DataSourceConfig.DecimalIsTwo &&
                     (readerValueType == typeof(decimal) ||
                      readerValueType == typeof(double) ||
                      readerValueType == typeof(float) ||
                      readerValueType == typeof(ClickHouseDecimal)))
            {
                return Math.Round(Convert.ToDouble(readerValue), 2);
            }

            return readerValue;
        }
    }
}