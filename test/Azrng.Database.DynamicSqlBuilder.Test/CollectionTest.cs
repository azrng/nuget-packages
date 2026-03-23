using Azrng.Core;
using Azrng.Database.DynamicSqlBuilder.Model;
using Common.Dapper.Repository;
using Microsoft.Extensions.Logging;

namespace Azrng.Database.DynamicSqlBuilder.Test
{
    public class CollectionTest
    {
        private readonly ILogger<CollectionTest> _logger;
        private readonly IDapperRepository _dapperRepository;
        private readonly IJsonSerializer _jsonSerializer;

        public CollectionTest(ILogger<CollectionTest> logger, IDapperRepository dapperRepository, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _dapperRepository = dapperRepository;
            _jsonSerializer = jsonSerializer;
        }

        [Fact]
        public async Task Test()
        {
            var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
            var sqlWhere = new List<SqlWhereClauseInfoDto>
            {
                new("creator", new List<FieldValueInfoDto> { new("admin", "admin") }),
                new("create_time",
                    new List<FieldValueInfoDto>
                    {
                        new("2025-01-01", "2025-01-01"),
                        new("2025-11-31", "2025-11-31")
                    },
                    MatchOperator.Between),
                new("cost_price",
                    new List<FieldValueInfoDto> { new(100000.00, 100000.00) },
                    MatchOperator.LessThan,
                    valueType: typeof(decimal)),
                new("vendor_id",
                    new List<FieldValueInfoDto> { new(1959275764966588416, 1959275764966588416) },
                    MatchOperator.In,
                    valueType: typeof(long)),
                new("outbound_price",
                    new List<FieldValueInfoDto> { new(1, 1) },
                    MatchOperator.GreaterThan,
                    valueType: typeof(decimal)),
                new("total_amount",
                    new List<FieldValueInfoDto> { new(10000m, 10000m) },
                    MatchOperator.LessThanEqual,
                    valueType: typeof(decimal)),
                new("order_name",
                    new List<FieldValueInfoDto> { new("出库", "出库") },
                    MatchOperator.Like)
            };

            var sortFields = new List<SortFieldDto> { new("create_time", false) };

            var inOperatorFields = new List<InOperatorFieldDto>
            {
                new("product_batch_id",
                    new List<object>
                    {
                        1959435186149298176,
                        1962857423063494656,
                        1962856541366267904,
                        1962857292440285184,
                        1962857495780143104
                    },
                    typeof(long)),
                new("batch_no",
                    new List<object> { "65423", "65412", "20250824", "20250824" },
                    typeof(string))
            };

            var notOperatorFields = new List<NotInOperatorFieldDto>
            {
                new("product_id", new List<object> { 2959275794024726528, 2959275794024726529 }, typeof(long)),
                new("batch_no", new List<object> { "44", "66" }, typeof(string))
            };

            const string necessaryCondition = " where deleted=false";

            {
                var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
                    "public.inventory_details",
                    necessaryCondition,
                    queryFields,
                    sqlWhere,
                    inOperatorFields: inOperatorFields,
                    notInOperatorFields: notOperatorFields,
                    sortFields: sortFields);

                _logger.LogInformation("=== Base query (PostgreSQL parameterized) ===");
                _logger.LogInformation("SQL: {Sql}", sql);
                _logger.LogInformation("Parameter count: {Count}", parameters.ParameterNames.Count());

                var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
                var safeResult = result ?? new List<object>();
                var resultList = _jsonSerializer.ToObject<IEnumerable<Dictionary<string, object>>>(
                    _jsonSerializer.ToJson(safeResult));

                _logger.LogInformation("Result count: {Count}", resultList?.Count() ?? 0);
            }

            {
                var (querySql, countSql, pageParameters) = DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
                    "public.inventory_details",
                    necessaryCondition,
                    queryFields,
                    sqlWhere,
                    inOperatorFields: inOperatorFields,
                    notInOperatorFields: notOperatorFields,
                    pageIndex: 1,
                    pageSize: 20,
                    sortFields: sortFields);

                _logger.LogInformation("=== Paging query (PostgreSQL parameterized) ===");
                _logger.LogInformation("Query SQL: {QuerySql}", querySql);
                _logger.LogInformation("Count SQL: {CountSql}", countSql);
                _logger.LogInformation("Parameter count: {Count}", pageParameters.ParameterNames.Count());

                var rowList = await _dapperRepository.QueryAsync<object>(querySql, pageParameters);
                var safeRowList = rowList ?? new List<object>();
                var resultList = _jsonSerializer.ToObject<IEnumerable<Dictionary<string, object>>>(
                    _jsonSerializer.ToJson(safeRowList));

                _logger.LogInformation("Paged row count: {Count}", resultList?.Count() ?? 0);

                var count = await _dapperRepository.ExecuteScalarAsync<int>(countSql, pageParameters);
                _logger.LogInformation("Count query result: {Count}", count);
            }
        }
    }
}
