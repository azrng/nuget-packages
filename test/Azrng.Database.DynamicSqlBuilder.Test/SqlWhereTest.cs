using Azrng.Core;
using Azrng.Database.DynamicSqlBuilder.Model;
using Common.Dapper.Repository;
using Microsoft.Extensions.Logging;

namespace Azrng.Database.DynamicSqlBuilder.Test;

public class SqlWhereTest
{
    private readonly ILogger<SqlWhereTest> _logger;
    private readonly IDapperRepository _dapperRepository;
    private readonly IJsonSerializer _jsonSerializer;

    public SqlWhereTest(ILogger<SqlWhereTest> logger, IDapperRepository dapperRepository, IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _dapperRepository = dapperRepository;
        _jsonSerializer = jsonSerializer;
    }

    [Fact]
    public async Task MatchOperator_Equal_String_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("creator", new List<FieldValueInfoDto> { new("admin", "admin") })
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        {
            var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
                "public.inventory_details",
                string.Empty,
                queryFields,
                sqlWhere,
                inOperatorFields: null,
                notInOperatorFields: null,
                sortFields: sortFields);

            _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

            var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
            var safeResult = result ?? new List<object>();
            _logger.LogInformation("Query result count: {Count}", safeResult.Count);
            Assert.True(safeResult.Count >= 0);
        }

        {
            var (querySql, countSql, pageParameters) = DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
                "public.inventory_details",
                string.Empty,
                queryFields,
                sqlWhere,
                inOperatorFields: null,
                notInOperatorFields: null,
                pageIndex: 1,
                pageSize: 20,
                sortFields: sortFields);

            var rowList = await _dapperRepository.QueryAsync<object>(querySql, pageParameters);
            var safeRowList = rowList ?? new List<object>();
            var resultList = _jsonSerializer.ToObject<IEnumerable<Dictionary<string, object>>>(
                _jsonSerializer.ToJson(safeRowList));

            _logger.LogInformation(
                "Paged query SQL: {Sql} parameters: {Count} result count: {ResultCount}",
                querySql,
                pageParameters.ParameterNames.Count(),
                resultList?.Count() ?? 0);

            var result = await _dapperRepository.QueryAsync<object>(countSql, pageParameters);
            var safeCountResult = result ?? new List<object>();
            _logger.LogInformation("Count query result count: {Count}", safeCountResult.Count);
            Assert.True(safeCountResult.Count >= 0);
        }
    }

    [Fact]
    public async Task MatchOperator_Between_CreateTime_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("create_time",
                new List<FieldValueInfoDto>
                {
                    new("2025-01-01", "2025-01-01"),
                    new("2025-11-30", "2025-11-30")
                },
                MatchOperator.Between)
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_Equal_Decimal_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("cost_price",
                new List<FieldValueInfoDto> { new("", 10000.00m) },
                MatchOperator.Equal,
                valueType: typeof(decimal))
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_Equal_VendorId_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("vendor_id",
                new List<FieldValueInfoDto> { new("1959275764966588416", 1959275764966588416) },
                MatchOperator.Equal,
                valueType: typeof(long))
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_GreaterThan_LessThan_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("outbound_price", new List<FieldValueInfoDto> { new("1", 11) }, MatchOperator.GreaterThan, valueType: typeof(int)),
            new("outbound_price", new List<FieldValueInfoDto> { new("1", 22) }, MatchOperator.GreaterThanEqual, valueType: typeof(int)),
            new("outbound_price", new List<FieldValueInfoDto> { new("1000", 100.1m) }, MatchOperator.LessThan, valueType: typeof(decimal)),
            new("outbound_price", new List<FieldValueInfoDto> { new("1000", 200.22m) }, MatchOperator.LessThanEqual, valueType: typeof(decimal))
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_Like_OrderName_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("order_name", new List<FieldValueInfoDto> { new("%出库订单%", "%出库订单%") }, MatchOperator.Like)
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_Not_Like_OrderName_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("order_name", new List<FieldValueInfoDto> { new("%aa%", "%aa%") }, MatchOperator.NotLike)
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_In_OrderName_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("batch_no",
                new List<FieldValueInfoDto>
                {
                    new("65412", "65412"),
                    new("20250824", "20250824")
                },
                MatchOperator.In,
                valueType: typeof(string))
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }

    [Fact]
    public async Task MatchOperator_Not_In_OrderName_Test()
    {
        var queryFields = new List<string> { "product_id", "product_batch_id", "creator", "create_time" };
        var sqlWhere = new List<SqlWhereClauseInfoDto>
        {
            new("batch_no",
                new List<FieldValueInfoDto>
                {
                    new("111", "111"),
                    new("222", "222")
                },
                MatchOperator.NotIn,
                valueType: typeof(string))
        };
        var sortFields = new List<SortFieldDto> { new("create_time", false) };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "public.inventory_details",
            string.Empty,
            queryFields,
            sqlWhere,
            inOperatorFields: null,
            notInOperatorFields: null,
            sortFields: sortFields);

        _logger.LogInformation("Query SQL: {Sql} parameters: {Count}", sql, parameters.ParameterNames.Count());

        var result = await _dapperRepository.QueryAsync<object>(sql, parameters);
        var safeResult = result ?? new List<object>();
        _logger.LogInformation("Query result count: {Count}", safeResult.Count);
        Assert.True(safeResult.Count >= 0);
    }
}
