using Azrng.Core;
using Azrng.DynamicSqlBuilder.Model;
using Common.Dapper.Repository;
using Microsoft.Extensions.Logging;

namespace Azrng.DynamicSqlBuilder.Test;

public class InOperatorTest
{
    private readonly ILogger<InOperatorTest> _logger;
    private readonly IDapperRepository _dapperRepository;
    private readonly IJsonSerializer _jsonSerializer;

    public InOperatorTest(ILogger<InOperatorTest> logger, IDapperRepository dapperRepository, IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _dapperRepository = dapperRepository;
        _jsonSerializer = jsonSerializer;
    }

    [Fact]
    public Task InOperatorFieldDto_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };

        var inOperatorFields = new List<InOperatorFieldDto>
                               {
                                   // 测试long类型
                                   new InOperatorFieldDto("product_batch_id",
                                       new List<object> { 1959435186149298176L, 1962857423063494656L, 1962856541366267904L },
                                       typeof(long)),

                                   // 测试string类型
                                   new InOperatorFieldDto("batch_no",
                                       new List<object> { "65423", "65412", "20250824" }, typeof(string)),

                                   // 测试int类型
                                   new InOperatorFieldDto("status",
                                       new List<object> { 1, 2, 3 }, typeof(int)),

                                   // 测试decimal类型
                                   new InOperatorFieldDto("cost_price",
                                       new List<object> { 100.50m, 200.75m, 300.00m }, typeof(decimal)),

                                   // 测试DateTime类型
                                   new InOperatorFieldDto("create_time",
                                       new List<object> { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2) }, typeof(DateTime)),

                                   // 测试bool类型
                                   new InOperatorFieldDto("is_active",
                                       new List<object> { true, false }, typeof(bool))
                               };

        var neccessaryCondition = " where deleted=false";

        // 调用参数化查询方法,返回SQL和参数对象
        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            neccessaryCondition,
            queryFields, sqlWhereClauses: null, inOperatorFields: inOperatorFields, notInOperatorFields: null);

        _logger.LogInformation("=== InOperatorFieldDto 测试 (PostgreSQL 参数化) ===");
        _logger.LogInformation("SQL: {Sql}", sql);
        _logger.LogInformation("参数数量: {Count}", parameters.ParameterNames.Count());

        // 验证SQL不为空
        Assert.NotNull(sql);
        Assert.NotEmpty(sql);

        // 验证参数不为空
        Assert.NotNull(parameters);

        // 验证是否包含IN操作符
        Assert.Contains("ANY", sql.ToUpper());
        return Task.CompletedTask;
    }

    [Fact]
    public Task NotInOperatorFieldDto_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id", "creator", "create_time" };

        var notInOperatorFields = new List<NotInOperatorFieldDto>
                                  {
                                      // 测试long类型
                                      new NotInOperatorFieldDto("product_id",
                                          new List<object> { 2959275794024726528L, 2959275794024726529L }, typeof(long)),

                                      // 测试string类型
                                      new NotInOperatorFieldDto("batch_no",
                                          new List<object> { "44", "66" }, typeof(string)),

                                      // 测试int类型
                                      new NotInOperatorFieldDto("status",
                                          new List<object> { 4, 5 }, typeof(int)),

                                      // 测试decimal类型
                                      new NotInOperatorFieldDto("outbound_price",
                                          new List<object> { 500.00m, 600.00m }, typeof(decimal)),

                                      // 测试DateTime类型
                                      new NotInOperatorFieldDto("modify_time",
                                          new List<object> { DateTime.Now.AddDays(-7), DateTime.Now.AddDays(-14) }, typeof(DateTime)),

                                      // 测试bool类型
                                      new NotInOperatorFieldDto("is_deleted",
                                          new List<object> { true }, typeof(bool))
                                  };

        var neccessaryCondition = " where deleted=false";

        // 调用参数化查询方法,返回SQL和参数对象
        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            neccessaryCondition,
            queryFields, sqlWhereClauses: null, inOperatorFields: null, notInOperatorFields: notInOperatorFields);

        _logger.LogInformation("=== NotInOperatorFieldDto 测试 (PostgreSQL 参数化) ===");
        _logger.LogInformation("SQL: {Sql}", sql);
        _logger.LogInformation("参数数量: {Count}", parameters.ParameterNames.Count());

        // 验证SQL不为空
        Assert.NotNull(sql);
        Assert.NotEmpty(sql);

        // 验证参数不为空
        Assert.NotNull(parameters);

        // 验证是否包含NOT IN操作符
        Assert.Contains("!=", sql.ToUpper());
        Assert.Contains("ANY", sql.ToUpper());
        return Task.CompletedTask;
    }

    [Fact]
    public Task InAndNotInOperatorFieldDto_Complex_Test()
    {
        var queryFields = new List<string>()
                          {
                              "product_id",
                              "product_batch_id",
                              "creator",
                              "create_time",
                              "status"
                          };

        var inOperatorFields = new List<InOperatorFieldDto>
                               {
                                   new InOperatorFieldDto("product_batch_id",
                                       new List<object> { 1959435186149298176L, 1962857423063494656L }, typeof(long)),
                                   new InOperatorFieldDto("batch_no",
                                       new List<object> { "65423", "65412" }, typeof(string))
                               };

        var notInOperatorFields = new List<NotInOperatorFieldDto>
                                  {
                                      new NotInOperatorFieldDto("product_id",
                                          new List<object> { 2959275794024726528L }, typeof(long)),
                                      new NotInOperatorFieldDto("status",
                                          new List<object> { 0 }, typeof(int)) // 0表示已删除
                                  };

        var sortFields = new List<SortFieldDto> { new SortFieldDto("create_time", false) }; // false = DESC
        var neccessaryCondition = " where deleted=false";

        {
            // 调用参数化查询方法,返回SQL和参数对象
            var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
                neccessaryCondition,
                queryFields, sqlWhereClauses: null, inOperatorFields: inOperatorFields,
                notInOperatorFields: notInOperatorFields, sortFields: sortFields);

            _logger.LogInformation("=== In和NotIn组合测试 (PostgreSQL 参数化) ===");
            _logger.LogInformation("SQL: {Sql}", sql);
            _logger.LogInformation("参数数量: {Count}", parameters.ParameterNames.Count());

            // 验证SQL不为空
            Assert.NotNull(sql);
            Assert.NotEmpty(sql);

            // 验证参数不为空
            Assert.NotNull(parameters);

            // 验证是否包含IN和NOT IN操作符
            Assert.Contains("ANY", sql.ToUpper());
            Assert.Contains("!=", sql.ToUpper());
        }

        // 分页查询测试
        {
            var (querySql, countSql, pageParameters) = DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
                "public.inventory_details",
                neccessaryCondition, queryFields, sqlWhereClauses: null, inOperatorFields: inOperatorFields,
                notInOperatorFields: notInOperatorFields, pageIndex: 1, pageSize: 10, sortFields: sortFields);

            _logger.LogInformation("\n=== 分页查询测试 (PostgreSQL 参数化) ===");
            _logger.LogInformation("查询SQL: {QuerySql}", querySql);
            _logger.LogInformation("计数SQL: {CountSql}", countSql);
            _logger.LogInformation("参数数量: {Count}", pageParameters.ParameterNames.Count());

            // 验证SQL不为空
            Assert.NotNull(querySql);
            Assert.NotNull(countSql);
            Assert.NotEmpty(querySql);
            Assert.NotEmpty(countSql);

            // 验证参数不为空
            Assert.NotNull(pageParameters);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public Task InOperatorFieldDto_EmptyList_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id" };

        var inOperatorFields = new List<InOperatorFieldDto>
                               {
                                   // 空列表测试
                                   new InOperatorFieldDto("product_batch_id",
                                       new List<object>(), typeof(long))
                               };

        var neccessaryCondition = " where deleted=false";

        // 调用参数化查询方法,返回SQL和参数对象
        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            neccessaryCondition,
            queryFields, sqlWhereClauses: null, inOperatorFields: inOperatorFields, notInOperatorFields: null);

        _logger.LogInformation("=== InOperatorFieldDto 空列表测试 ===");
        _logger.LogInformation("SQL: {Sql}", sql);
        _logger.LogInformation("参数数量: {Count}", parameters.ParameterNames.Count());

        // 验证SQL不为空
        Assert.NotNull(sql);
        Assert.NotEmpty(sql);

        // 验证应该生成 1=2 条件（因为列表为空）
        Assert.Contains("1=2", sql);
        return Task.CompletedTask;
    }

    [Fact]
    public Task NotInOperatorFieldDto_EmptyList_Test()
    {
        var queryFields = new List<string>() { "product_id", "product_batch_id" };

        var notInOperatorFields = new List<NotInOperatorFieldDto>
                                  {
                                      // 空列表测试
                                      new NotInOperatorFieldDto("product_id",
                                          new List<object>(), typeof(long))
                                  };

        var neccessaryCondition = " where deleted=false";

        // 调用参数化查询方法,返回SQL和参数对象
        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric("public.inventory_details",
            neccessaryCondition,
            queryFields, sqlWhereClauses: null, inOperatorFields: null, notInOperatorFields: notInOperatorFields);

        _logger.LogInformation("=== NotInOperatorFieldDto 空列表测试 ===");
        _logger.LogInformation("SQL: {Sql}", sql);
        _logger.LogInformation("参数数量: {Count}", parameters.ParameterNames.Count());

        // 验证SQL不为空
        Assert.NotNull(sql);
        Assert.NotEmpty(sql);

        // 验证应该生成 1=1 条件（因为列表为空，NOT IN 空列表应为全部）
        Assert.Contains("1=1", sql);
        return Task.CompletedTask;
    }
}