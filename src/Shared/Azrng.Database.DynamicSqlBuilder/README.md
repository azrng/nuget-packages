# Azrng.DynamicSqlBuilder

一个强大、灵活的动态 SQL 构建器，支持参数化查询和多种 SQL 操作符。

## 功能特性

- **参数化查询** - 使用 Dapper 的 DynamicParameters 防止 SQL 注入
- **多种操作符支持** - =, <>, >, >=, <=, LIKE, NOT LIKE, BETWEEN, IN, NOT IN
- **嵌套条件** - 支持复杂的嵌套 WHERE 条件
- **排序支持** - 支持多字段排序（ASC/DESC）
- **类型转换** - 自动处理各种数据类型的转换
- **分页支持** - 内置 LIMIT 和 OFFSET 分页
- **工厂模式** - 使用工厂模式创建不同类型的 SQL 操作
- **类型安全** - 使用泛型确保类型安全

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.DynamicSqlBuilder
```

或通过 .NET CLI:

```
dotnet add package Azrng.DynamicSqlBuilder
```

### 基本使用

#### 1. 构建简单条件

```csharp
// 等于条件
var whereClause = new SqlWhereClauseInfoDto("Name", MatchOperator.Equal, "John");

// 大于条件
var ageClause = new SqlWhereClauseInfoDto("Age", MatchOperator.GreaterThan, 25);

// LIKE 条件
var nameClause = new SqlWhereClauseInfoDto("Name", MatchOperator.Like, "%John%");
```

#### 2. 构建复杂查询

```csharp
// 创建多个条件
var sqlWhereClauses = new List<SqlWhereClauseInfoDto>
{
    new SqlWhereClauseInfoDto("Status", MatchOperator.Equal, 1),
    new SqlWhereClauseInfoDto("DepartmentId", new List<int>{1, 2, 3}),
    new SqlWhereClauseInfoDto("CreateTime", MatchOperator.GreaterThan, DateTime.Now.AddDays(-30))
};

// 创建排序
var sortFields = new List<SortFieldDto>
{
    new SortFieldDto("CreateTime", "DESC"),
    new SortFieldDto("Name", "ASC")
};

// 生成完整的 SQL 查询
var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
    "Users",
    new List<string> { "Id", "Name", "Email", "Status", "CreateTime" },
    sqlWhereClauses,
    sortFields
);

// 执行查询
var users = await connection.QueryAsync<User>(sql, parameters);
```

#### 3. 分页查询

```csharp
var page = 1;
var pageSize = 20;

var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementWithPaging(
    "SELECT * FROM Users WHERE Status = @Status",
    page,
    pageSize,
    "CreateTime",
    "DESC",
    new { Status = 1 }
);
```

#### 4. IN 操作

```csharp
// 创建 IN 条件
var inFields = new List<InOperatorFieldDto>
{
    InOperatorFieldDto.Create<int>("DepartmentId", new[] { 1, 2, 3, 5 })
};

var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
    "Products",
    new List<string> { "Id", "Name", "DepartmentId" },
    inFields,
    inFields
);
```

## 操作符详细说明

### 比较操作符 (=, <>, >, >=, <=)

| 操作符 | 说明 | 示例 | SQL |
|---------|------|------|-----|
| Equal | 等于 | `WHERE Age = @Age` |
| NotEqual | 不等于 | `WHERE Name <> @Name` |
| GreaterThan | 大于 | `WHERE Score > @Score` |
| LessThan | 小于 | `WHERE Price < @Price` |
| GreaterOrEqual | 大于等于 | `WHERE Age >= @Age` |
| LessOrEqual | 小于等于 | `WHERE Level <= @Level` |

### 字符串操作符 (LIKE, NOT LIKE)

| 操作符 | 说明 | 示例 | SQL |
|---------|------|------|-----|
| Like | 模糊匹配 | `WHERE Name LIKE @Name` |
| NotLike | 不匹配 | `WHERE Name NOT LIKE @Name` |

### 范围操作符 (BETWEEN)

| 操作符 | 说明 | 示例 | SQL |
|---------|------|------|-----|
| Between | 在范围内 | `WHERE Age BETWEEN @MinAge AND @MaxAge` |

### 集合操作符 (IN, NOT IN)

| 操作符 | 说明 | 示例 | SQL |
|---------|------|------|-----|
| In | 包含于 | `WHERE DepartmentId IN @DeptIds` |
| NotIn | 不包含于 | `WHERE Status NOT IN @Statuses` |

## API 参考

### DynamicSqlBuilderHelper 类

主要方法：

```csharp
// 构建带参数的SQL查询
public static (string, Dapper.DynamicParameters) BuilderSqlQueryStatementGeneric<T>(
    string tableName,
    List<string> selectFields,
    List<SqlWhereClauseInfoDto> whereClauses,
    List<SortFieldDto> sortFields = null,
    List<InOperatorFieldDto> inFields = null,
    List<InOperatorFieldDto> inFields = null
)

// 构建分页SQL查询
public static string BuilderSqlQueryStatementWithPaging<T>(
    string sourceSql,
    int pageIndex,
    int pageSize,
    string orderByField = null,
    string orderDirection = null)
```

### SqlWhereClauseHelper 类

WHERE 条件构建助手：

```csharp
// 创建字段条件
public static SqlWhereClauseInfoDto CreateFieldCondition<T>(
    string fieldName,
    MatchOperator matchOperator,
    T value)

// 创建 IN 条件
public static SqlWhereClauseInfoDto CreateInCondition<T>(
    string fieldName,
    List<T> values)

// 创建 BETWEEN 条件
public static SqlWhereClauseInfoDto CreateBetweenCondition<T>(
    string fieldName,
    T minValue,
    T maxValue)

// 创建 LIKE 条件
public static SqlWhereClauseInfoDto CreateLikeCondition(
    string fieldName,
    string likeValue,
    MatchOperator matchOperator = MatchOperator.Like)
```

## 数据模型

### FieldValueInfoDto

字段值信息：

```csharp
public class FieldValueInfoDto
{
    public string FieldName { get; set; }  // 字段名
    public object Value { get; set; }      // 字段值
}
```

### InOperatorFieldDto

IN 操作字段：

```csharp
public class InOperatorFieldDto
{
    public string FieldName { get; set; }     // 字段名
    public List<object> Values { get; set; }  // 值集合
}
```

### SortFieldDto

排序字段：

```csharp
public class SortFieldDto
{
    public string FieldName { get; set; }  // 字段名
    public string Direction { get; set; }   // 排序方向（ASC/DESC）
}
```

## SQL 操作工厂

项目使用工厂模式创建不同类型的 SQL 操作：

- **SqlEqualOperation** - 生成等于条件
- **SqlInOperation** - 生成 IN 条件
- **SqlLikeOperation** - 生成 LIKE 条件
- **SqlBetweenOperation** - 生成 BETWEEN 条件
- **其他操作类型**...

## 类型转换

`TypeConvertHelper` 提供类型转换功能：

```csharp
// 转换为目标类型
public static object ConvertToTargetType(object value, Type targetType)

// 类型检查
public static Type GetUnderlyingType(Type type)

// 可空类型处理
public static object GetDefaultVaule(Type type)
```

## 安全特性

### SQL 注入防护

项目使用 **Dapper.DynamicParameters** 确保所有查询都通过参数化方式执行：

✅ **安全做法：**
```csharp
// 使用 DynamicParameters - Dapper 会自动处理参数化
var parameters = new DynamicParameters();
parameters.Add("Name", userName);

var result = await connection.QueryAsync<User>(sql, parameters);
```

❌ **不安全做法（避免）：**
```csharp
// 禁止字符串拼接
string sql = $"SELECT * FROM Users WHERE Name = '{userName}'";  // ❌ 危险
```

### 字段名验证

✅ **新功能：** 从 v1.1.0 开始，项目内置字段名验证功能

**自动验证：**
所有字段名在构建SQL时都会自动验证，防止SQL注入：

```csharp
// 以下字段名会被拒绝：
// - SQL关键字：SELECT, DROP, DELETE等
// - 包含特殊字符：; -- ' " /* */
// - 空字段名或纯空格
// - 超长字段名（>128字符）

var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
    "Users",                          // ✅ 表名会被验证
    new List<string> { "Id", "Name" }, // ✅ 查询字段会被验证
    new List<SqlWhereClauseInfoDto>
    {
        new SqlWhereClauseInfoDto("Email", MatchOperator.Equal, "test@test.com") // ✅ WHERE字段会被验证
    },
    sortFields: new List<SortFieldDto>
    {
        new SortFieldDto("CreateTime", false) // ✅ 排序字段会被验证
    }
);
```

**自定义验证：**
您也可以使用 `FieldNameValidator` 类进行自定义验证：

```csharp
using Azrng.DynamicSqlBuilder.Validation;

// 验证单个字段名
FieldNameValidator.ValidateFieldName("UserName"); // ✅ 有效
FieldNameValidator.ValidateFieldName("DROP TABLE"); // ❌ 抛出 ArgumentException

// 验证多个字段名
var fields = new[] { "Id", "Name", "Email" };
if (!FieldNameValidator.AreValidFieldNames(fields, out var invalidFields))
{
    Console.WriteLine($"无效字段: {string.Join(", ", invalidFields)}");
}

// 使用白名单验证
var allowedFields = new[] { "Id", "Name", "Email" };
FieldNameValidator.ValidateFieldNameWhitelist("Name", allowedFields); // ✅
FieldNameValidator.ValidateFieldNameWhitelist("Password", allowedFields); // ❌
```

**验证规则：**
1. 字段名不能为空或纯空格
2. 字段名长度不能超过128字符
3. 字段名必须以字母或下划线开头
4. 字段名只能包含字母、数字和下划线
5. 字段名不能是SQL关键字（SELECT, DROP, DELETE等）
6. 字段名不能包含SQL注入模式（--、;、'等）

## 最佳实践

### 1. 使用参数化查询

始终使用 DynamicParameters 防止 SQL 注入：

```csharp
var parameters = new Dapper.DynamicParameters();
parameters.Add("Status", status);
parameters.Add("DepartmentId", deptId);

var (sql, pars) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
    "Users",
    fields,
    whereClauses,
    sortFields,
    inFields,
    inFields,
    parameters);
```

### 2. 验证输入

在使用动态 SQL 构建器之前验证输入：

```csharp
// 验证字段名
if (!IsValidFieldName(fieldName))
{
    throw new ArgumentException($"无效的字段名: {fieldName}");
}

// 验证排序方向
if (orderDirection != "ASC" && orderDirection != "DESC")
{
    throw new ArgumentException("排序方向必须是 ASC 或 DESC");
}
```

### 3. 处理空值

```csharp
// 安全处理可能为 null 的值
public static object GetSafeValue(object value)
{
    return value ?? DBNull.Value;
}

// 在条件构建时使用
var safeValue = GetSafeValue(userInput);
```

### 4. 使用事务

对于多个数据库操作，使用事务确保数据一致性：

```csharp
using var transaction = await connection.BeginTransactionAsync();
try
{
    // 执行多个操作
    await connection.ExecuteAsync(sql1, parameters, transaction);
    await connection.ExecuteAsync(sql2, parameters, transaction);

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```


## 已知限制

1. **分页语法** - 当前使用 PostgreSQL 语法的 `LIMIT {0} OFFSET {1}`
   - 不同数据库的分页语法可能不同
   - SQL Server 使用 `OFFSET/FETCH`
   - Oracle 使用 `ROWNUM` 或 `OFFSET/FETCH`

2. **字段名大小写** - 某些数据库对字段名大小写敏感

3. **类型转换** - 复杂类型转换可能失败，需要妥善处理

## 适用场景

- 动态查询构建器 - 需要根据用户输入动态构建查询
- 报表系统 - 需要支持多种过滤和排序条件
- 后台管理系统 - 需要灵活的查询条件组合
- API 系统 - 需要安全地构建动态 SQL


## 相关链接

- GitHub 仓库：[https://github.com/azrng/nuget-packages](https://github.com/azrng/nuget-packages)

## 版本历史

### 1.1.0 (最新)
- ✨ 新增字段名自动验证功能
  - 自动验证表名、查询字段、WHERE字段、排序字段
  - 防止SQL注入和无效字段名
  - 新增 FieldNameValidator 验证器类
- ✨ 新增全面的测试覆盖
  - 边界条件测试（BoundaryTests）
  - 安全测试（SecurityTest）
  - 错误处理测试（ErrorHandlingTests）
- 🎨 重构现有测试代码
  - 引入 TestDataFactory 统一测试数据管理
  - 引入 TestHelper 统一测试辅助方法
  - 消除硬编码和重复代码
- 📝 完善项目文档
  - 更新安全特性说明
  - 新增测试指南

### 1.0.0
- 初始版本
- 支持基本的 SQL 操作符
- 提供参数化查询功能
- 支持分页查询