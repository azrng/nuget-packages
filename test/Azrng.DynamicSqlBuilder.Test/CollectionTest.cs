using Azrng.Core;
using Azrng.DynamicSqlBuilder.Model;
using Common.Dapper.Repository;
using ConsoleApp.Models.DynamicSql;
using Microsoft.Extensions.Logging;

namespace Azrng.DynamicSqlBuilder.Test
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
            var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
            var sqlWhere = new List<SqlWhereClauseInfoDto>
                           {
                               new SqlWhereClauseInfoDto("creator",
                                   new List<FieldValueInfoDto> { new FieldValueInfoDto("admin", "admin") }),
                               new SqlWhereClauseInfoDto("create_time",
                                   new List<FieldValueInfoDto>
                                   {
                                       new FieldValueInfoDto("2025-01-01", "2025-01-01"), new FieldValueInfoDto("2025-11-31", "2025-11-31")
                                   },
                                   MatchOperator.Between),
                               new SqlWhereClauseInfoDto("cost_price",
                                   new List<FieldValueInfoDto> { new FieldValueInfoDto(100000.00, 100000.00) }, MatchOperator.LessThan,
                                   valueType: typeof(decimal)),
                               new SqlWhereClauseInfoDto("vendor_id",
                                   new List<FieldValueInfoDto> { new FieldValueInfoDto(1959275764966588416, 1959275764966588416) },
                                   MatchOperator.In,
                                   valueType: typeof(long)),
                               new SqlWhereClauseInfoDto("outbound_price",
                                   new List<FieldValueInfoDto> { new FieldValueInfoDto(1, 1) }, MatchOperator.GreaterThan,
                                   valueType: typeof(decimal)),
                               new SqlWhereClauseInfoDto("total_amount",
                                   new List<FieldValueInfoDto> { new FieldValueInfoDto(10000m, 10000m) }, MatchOperator.LessThanEqual,
                                   valueType: typeof(decimal)),
                               new SqlWhereClauseInfoDto("order_name",
                                   new List<FieldValueInfoDto> { new FieldValueInfoDto("出库", "出库") }, MatchOperator.Like),
                           };

            var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) }; // false = DESC

            var inOperatorFields = new List<InOperatorFieldDto>
                                   {
                                       new InOperatorFieldDto("product_batch_id",
                                           new List<object>()
                                           {
                                               1959435186149298176,
                                               1962857423063494656,
                                               1962856541366267904,
                                               1962857292440285184,
                                               1962857495780143104
                                           }, typeof(long)),
                                       new InOperatorFieldDto("batch_no",
                                           new List<object> { "65423", "65412", "20250824", "20250824" }, typeof(string))
                                   };

            var notOperatorFields = new List<NotInOperatorFieldDto>
                                    {
                                        new NotInOperatorFieldDto("product_id",
                                            new List<object> { 2959275794024726528, 2959275794024726529 }, typeof(long)),
                                        new NotInOperatorFieldDto("batch_no",
                                            new List<object> { "44", "66" }, typeof(string))
                                    };
            var neccessaryCondition = " where deleted=false";

            {
                // 调用参数化查询方法,返回SQL和参数对象
                var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
                    neccessaryCondition,
                    queryFields, sqlWhere, inOperatorFields: inOperatorFields, notInOperatorFields: notOperatorFields,
                    sortFields: sortFields);

                _logger.LogInformation("=== 基础查询 (PostgreSQL 参数化) ===");
                _logger.LogInformation("SQL: {Sql}", sql);
                _logger.LogInformation("参数数量: {Count}", parameters.ParameterNames.Count());

                var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
                var resultList = _jsonSerializer.ToObject<IEnumerable<Dictionary<string, object>>>(_jsonSerializer.ToJson(result));
                _logger.LogInformation($"查询到数据的总数：{resultList?.Count() ?? 0}");
            }

            // 示例2: 分页查询 (如果需要可以取消注释)
            {
                var (querySql, countSql, pageParameters) = DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
                    "public.inventory_details",
                    neccessaryCondition, queryFields, sqlWhere, inOperatorFields: inOperatorFields, notInOperatorFields: notOperatorFields,
                    pageIndex: 1,
                    pageSize: 20,
                    sortFields: sortFields);

                _logger.LogInformation("\n=== 分页查询 (PostgreSQL 参数化) ===");
                _logger.LogInformation("查询SQL: {QuerySql}", querySql);
                _logger.LogInformation("计数SQL: {CountSql}", countSql);
                _logger.LogInformation("参数数量: {Count}", pageParameters.ParameterNames.Count());

                var rowList = await _dapperRepository.QueryAsync<object>(querySql, pageParameters);
                var resultList = _jsonSerializer.ToObject<IEnumerable<Dictionary<string, object>>>(_jsonSerializer.ToJson(rowList));
                _logger.LogInformation($"查询到数据的总数：{resultList?.Count() ?? 0}");

                var result = await _dapperRepository.ExecuteScalarAsync<int>(countSql, pageParameters);
                _logger.LogInformation($"查询到数据的总数：{result}");
            }
        }
    }
}