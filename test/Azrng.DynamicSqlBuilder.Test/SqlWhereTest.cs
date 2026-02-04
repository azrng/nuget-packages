using Azrng.Core;
using Azrng.DynamicSqlBuilder.Model;
using Common.Dapper.Repository;
using Microsoft.Extensions.Logging;

namespace Azrng.DynamicSqlBuilder.Test;

public class SqlWhereTest
{
    private readonly ILogger<CollectionTest> _logger;
    private readonly IDapperRepository _dapperRepository;
    private readonly IJsonSerializer _jsonSerializer;

    public SqlWhereTest(ILogger<CollectionTest> logger, IDapperRepository dapperRepository, IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _dapperRepository = dapperRepository;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// sqlWhere、=、字符串
    /// </summary>
    [Fact]
    public async Task MatchOperator_Equal_String_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("creator", new List<FieldValueInfoDto> { new FieldValueInfoDto("admin", "admin") }),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };
        {
            // 调用参数化查询方法,返回SQL和参数对象
            var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
                string.Empty,
                queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);

            _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

            var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
            _logger.LogInformation($"查询结果: {result.Count}条");
            Assert.True(result?.Count >= 0);
        }

        // 示例2: 分页查询 (如果需要可以取消注释)
        {
            var (querySql, countSql, pageParameters) = DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
                "public.inventory_details",
                string.Empty, queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null,
                pageIndex: 1,
                pageSize: 20,
                sortFields: sortFields);

            var rowList = await _dapperRepository.QueryAsync<object>(querySql, pageParameters);
            var resultList = _jsonSerializer.ToObject<IEnumerable<Dictionary<string, object>>>(_jsonSerializer.ToJson(rowList));
            _logger.LogInformation($"查询脚本: {querySql} parameters: {pageParameters.ParameterNames.Count()}  结果：{resultList?.Count()}条");

            var result = await _dapperRepository.QueryAsync<object>(countSql, pageParameters);
            _logger.LogInformation($"查询结果: {result.Count}条");
            Assert.True(result?.Count >= 0);
        }
    }

    /// <summary>
    /// sqlWhere、between、时间
    /// </summary>
    [Fact]
    public async Task MatchOperator_Between_CreateTime_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("create_time",
                               new List<FieldValueInfoDto>
                               {
                                   new FieldValueInfoDto("2025-01-01", "2025-01-01"), new FieldValueInfoDto("2025-11-30", "2025-11-30")
                               }, MatchOperator.Between),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、=、decimal
    /// </summary>
    [Fact]
    public async Task MatchOperator_Equal_Decimal_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("cost_price",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("", 10000.00m) }, MatchOperator.Equal,
                               valueType: typeof(decimal)),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、=、long
    /// </summary>
    [Fact]
    public async Task MatchOperator_Equal_VendorId_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("vendor_id",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("1959275764966588416", 1959275764966588416) },
                               MatchOperator.Equal, valueType: typeof(long)),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、>、>=、<、<=、int
    /// </summary>
    [Fact]
    public async Task MatchOperator_GreaterThan_LessThan_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("outbound_price",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("1", 11) }, MatchOperator.GreaterThan,
                               valueType: typeof(int)),
                           new SqlWhereClauseInfoDto("outbound_price",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("1", 22) }, MatchOperator.GreaterThanEqual,
                               valueType: typeof(int)),
                           new SqlWhereClauseInfoDto("outbound_price",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("1000", 100.1m) },
                               MatchOperator.LessThan, valueType: typeof(decimal)),
                           new SqlWhereClauseInfoDto("outbound_price",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("1000", 200.22m) },
                               MatchOperator.LessThanEqual, valueType: typeof(decimal)),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、like
    /// </summary>
    [Fact]
    public async Task MatchOperator_Like_OrderName_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("order_name",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("%出库订单%", "%出库订单%") }, MatchOperator.Like),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、not like
    /// </summary>
    [Fact]
    public async Task MatchOperator_Not_Like_OrderName_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("order_name",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("%aa%", "%aa%") }, MatchOperator.NotLike),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、in
    /// </summary>
    [Fact]
    public async Task MatchOperator_In_OrderName_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("batch_no",
                               new List<FieldValueInfoDto>
                               {
                                   new FieldValueInfoDto("65412", "65412"), new FieldValueInfoDto("20250824", "20250824")
                               },
                               MatchOperator.In, valueType: typeof(string)),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);
        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result.Count >= 0);
    }

    /// <summary>
    /// sqlWhere、not in
    /// </summary>
    [Fact]
    public async Task MatchOperator_Not_In_OrderName_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
                       {
                           new SqlWhereClauseInfoDto("batch_no",
                               new List<FieldValueInfoDto> { new FieldValueInfoDto("111", "111"), new FieldValueInfoDto("222", "222") },
                               MatchOperator.NotIn, valueType: typeof(string)),
                       };
        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            string.Empty,
            queryFields, sqlWhere, inOperatorFields: null, notInOperatorFields: null, sortFields: sortFields);

        _logger.LogInformation($"查询脚本: {sql} parameters: {parameters.ParameterNames.Count()}");

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        _logger.LogInformation($"查询结果: {result.Count}条");
        Assert.True(result?.Count >= 0);
    }
}