# Azrng.Database.DynamicSqlBuilder 项目架构文档

## 目录

- [1. 项目概述](#1-项目概述)
- [2. 整体架构](#2-整体架构)
- [3. 核心设计模式](#3-核心设计模式)
- [4. 分层架构详解](#4-分层架构详解)
- [5. 核心工作原理](#5-核心工作原理)
- [6. SQL构建流程](#6-sql构建流程)
- [7. 安全机制](#7-安全机制)
- [8. 扩展点](#8-扩展点)

---

## 1. 项目概述

**Azrng.Database.DynamicSqlBuilder** 是一个基于 .NET 的动态 SQL 参数化查询构建器，旨在提供安全、灵活、类型安全的动态 SQL 构建能力。

### 1.1 核心目标

- **安全性**：通过参数化查询和字段名验证防止 SQL 注入
- **灵活性**：支持多种 SQL 操作符和复杂嵌套条件
- **易用性**：提供简洁的 API 接口
- **类型安全**：支持泛型和强类型参数

### 1.2 技术栈

- **.NET 6.0+**：支持 .NET 6.0 至 .NET 10.0
- **Dapper**：轻量级 ORM，提供参数化查询支持
- **C# 泛型**：实现类型安全的操作符
- **工厂模式**：动态创建 SQL 操作对象
- **Builder 模式**：构建复杂的 SQL 查询

---

## 2. 整体架构

```
┌─────────────────────────────────────────────────────────────────┐
│                     DynamicSqlBuilderHelper                      │
│                    (SQL 构建入口 / Facade)                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│   Model 层     │   │ SqlOperation  │   │  Validation   │
│  (数据模型)     │   │    层        │   │    层        │
├───────────────┤   ├───────────────┤   ├───────────────┤
│• MatchOperator│   │• SqlOperation │   │• FieldName    │
│• SqlWhere     │   │  (抽象基类)    │   │  Validator    │
│  ClauseInfoDto│   │• SqlOperation │   └───────────────┘
│• InOperator   │   │  Factory      │
│  FieldDto     │   │• 具体操作实现: │
│• SortFieldDto │   │  - Equal      │
└───────────────┘   │  - In         │
                    │  - Like       │
                    │  - Between    │
                    │  - GreaterThan│
                    │  ...          │
                    └───────────────┘
                            ▲
                            │
                    ┌───────┴────────┐
                    │   Utils 层     │
                    ├────────────────┤
                    │• TypeConvert   │
                    │  Helper        │
                    └────────────────┘
```

---

## 3. 核心设计模式

### 3.1 工厂模式 (Factory Pattern)

**目的**：根据不同的操作符类型创建对应的 SQL 操作对象

**实现位置**：[SqlOperationFactory.cs](SqlOperation/SqlOperationFactory.cs)

```csharp
public static SqlOperation CreateSqlOperation(MatchOperator operation)
{
    return operation switch
    {
        MatchOperator.Equal => new SqlEqualOperation(),
        MatchOperator.In => new SqlInOperation(),
        MatchOperator.Like => new SqlLikeOperation(),
        // ...
        _ => throw new NotSupportedException("不支持的类型")
    };
}
```

**优势**：
- 解耦操作符枚举与具体实现
- 便于扩展新的操作符类型
- 集中管理对象创建逻辑

### 3.2 Builder 模式 (Builder Pattern)

**目的**：分步骤构建复杂的 SQL 查询语句

**实现位置**：[DynamicSqlBuilderHelper.cs](DynamicSqlBuilderHelper.cs)

**构建流程**：
1. 构建 SELECT 子句
2. 构建 WHERE 子句
3. 构建 ORDER BY 子句
4. 构建 LIMIT/OFFSET 子句

### 3.3 模板方法模式 (Template Method Pattern)

**目的**：定义 SQL 操作的算法骨架，子类实现具体步骤

**实现位置**：[SqlOperation.cs](SqlOperation/SqlOperationFactory.cs)

```csharp
public abstract class SqlOperation
{
    // 生成唯一参数名
    protected string GetParameterName(string fieldName) { ... }

    // 抽象方法 - 子类实现
    public virtual string GetSqlSentenceResult(...) { ... }
}
```

### 3.4 策略模式 (Strategy Pattern)

**目的**：不同的 SQL 操作符采用不同的生成策略

**实现**：每个操作符类都是一个独立的策略
- `SqlEqualOperation`：等于策略
- `SqlInOperation`：IN 策略
- `SqlLikeOperation`：LIKE 策略

---

## 4. 分层架构详解

### 4.1 Model 层

**职责**：定义数据传输对象 (DTO) 和枚举类型

#### 4.1.1 MatchOperator (操作符枚举)

[MatchOperator.cs](Model/MatchOperator.cs)

```csharp
public enum MatchOperator
{
    Equal,           // =
    NotEqual,        // <>
    GreaterThan,     // >
    LessThan,        // <
    GreaterThanEqual,// >=
    LessThanEqual,   // <=
    Like,            // LIKE
    NotLike,         // NOT LIKE
    Between,         // BETWEEN
    In,              // IN
    NotIn,           // NOT IN
    And              // AND
}
```

#### 4.1.2 SqlWhereClauseInfoDto (WHERE 条件 DTO)

[SqlWhereClauseInfoDto.cs](Model/SqlWhereClauseInfoDto.cs)

```csharp
public class SqlWhereClauseInfoDto
{
    public MatchOperator MatchOperator { get; set; }      // 操作符
    public string FieldName { get; set; }                  // 字段名
    public List<FieldValueInfoDto> FieldValueInfos { get; set; } // 字段值
    public string LogicalOperator { get; set; }            // 逻辑运算符
    public IEnumerable<SqlWhereClauseInfoDto> NestedChildrens { get; set; } // 嵌套条件
    public Type ValueType { get; set; }                    // 值类型
}
```

**设计亮点**：
- 支持嵌套条件 ([`NestedChildrens`](Model/SqlWhereClauseInfoDto.cs#L49))
- 支持类型安全 ([`ValueType`](Model/SqlWhereClauseInfoDto.cs#L54))
- 支持逻辑运算符组合

#### 4.1.3 InOperatorFieldDto / NotInOperatorFieldDto

[InOperatorFieldDto.cs](Model/InOperatorFieldDto.cs)

```csharp
public class InOperatorFieldDto
{
    public string Field { get; set; }
    public IEnumerable<object> Ids { get; set; }
    public Type ValueType { get; set; }

    // 泛型工厂方法
    public static InOperatorFieldDto Create<T>(string field, IEnumerable<T> values)
    {
        return new InOperatorFieldDto(field, values?.Cast<object>(), typeof(T));
    }
}
```

### 4.2 SqlOperation 层

**职责**：实现各种 SQL 操作符的生成逻辑

#### 4.2.1 SqlOperation 抽象基类

```csharp
public abstract class SqlOperation
{
    private static int _parameterIndex = 0;

    // 生成唯一参数名 (线程安全)
    protected string GetParameterName(string fieldName)
    {
        return $"@p_{fieldName}_{Interlocked.Increment(ref _parameterIndex)}";
    }

    // 多个重载方法支持不同类型
    public virtual string GetSqlSentenceResult(
        string fieldName, object fieldValue,
        DynamicParameters parameters, Type valueType) { }
}
```

**设计特点**：
- 使用 `Interlocked.Increment` 保证参数名生成的线程安全
- 提供多个重载方法支持不同数据类型
- 参数名格式：`@p_{字段名}_{索引}`

#### 4.2.2 具体操作实现

**SqlEqualOperation** ([SqlEqualOperation.cs](SqlOperation/SqlEqualOperation.cs))
```csharp
public override string GetSqlSentenceResult(
    string fieldName, object fieldValue,
    DynamicParameters parameters, Type valueType)
{
    var paramName = GetParameterName(fieldName);
    parameters.Add(paramName, fieldValue);
    return $" {fieldName} = {paramName} ";
}
```

**SqlInOperation** ([SqlInOperation.cs](SqlOperation/SqlInOperation.cs))
```csharp
public override string GetSqlSentenceResult(
    string fieldName, IEnumerable<object> fieldValues,
    DynamicParameters parameters, Type valueType)
{
    var paramName = GetParameterName(fieldName);
    var convertedValues = TypeConvertHelper.ConvertToTargetType(fieldValues, valueType);
    parameters.Add(paramName, convertedValues);
    return $" {fieldName} = ANY({paramName}) ";  // PostgreSQL 语法
}
```

### 4.3 Validation 层

**职责**：验证字段名安全性，防止 SQL 注入

#### 4.3.1 FieldNameValidator

[FieldNameValidator.cs](Validation/FieldNameValidator.cs)

```csharp
public static class FieldNameValidator
{
    private static readonly Regex FieldNamePattern = new(@"^[a-zA-Z_.][a-zA-Z0-9_.]*$");
    private static readonly HashSet<string> SqlKeywords = new() { /* SQL 关键字 */ };

    private static bool IsValidFieldName(string fieldName)
    {
        // 1. 检查空值
        if (string.IsNullOrWhiteSpace(fieldName)) return false;

        // 2. 检查长度
        if (fieldName.Length > 128) return false;

        // 3. 检查格式
        if (!FieldNamePattern.IsMatch(fieldName)) return false;

        // 4. 检查 SQL 关键字
        if (SqlKeywords.Contains(fieldName)) return false;

        // 5. 检查可疑模式
        if (ContainsSuspiciousPatterns(fieldName)) return false;

        return true;
    }
}
```

**验证规则**：
1. 非空检查
2. 长度限制 (最大 128 字符)
3. 正则表达式验证：`^[a-zA-Z_.][a-zA-Z0-9_.]*$`
4. SQL 关键字黑名单检查
5. 可疑模式检查（如 `--`, `;`, `'` 等）

### 4.4 Utils 层

**职责**：提供通用工具方法

#### 4.4.1 TypeConvertHelper

[TypeConvertHelper.cs](Utils/TypeConvertHelper.cs)

```csharp
public class TypeConvertHelper
{
    // 单值类型转换
    public static object ConvertToTargetType(object value, Type targetType)
    {
        // 处理可空类型
        if (targetType.IsGenericType &&
            targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            targetType = Nullable.GetUnderlyingType(targetType);
        }

        // 根据目标类型转换
        if (targetType == typeof(int)) return Convert.ToInt32(value);
        if (targetType == typeof(long)) return Convert.ToInt64(value);
        if (targetType == typeof(string)) return value?.ToString() ?? string.Empty;
        // ... 更多类型
    }
}
```

---

## 5. 核心工作原理

### 5.1 参数化查询机制

**传统 SQL 注入风险**：
```csharp
// ❌ 危险：直接拼接字符串
string sql = $"SELECT * FROM Users WHERE Name = '{userName}'";
```

**本项目安全方案**：
```csharp
// ✅ 安全：使用 Dapper.DynamicParameters
var parameters = new DynamicParameters();
parameters.Add("@p_Name_1", userName);
string sql = "SELECT * FROM Users WHERE Name = @p_Name_1";
```

**工作原理**：
1. 所有值都通过 `DynamicParameters` 传递
2. SQL 中使用占位符 `@p_XXX` 引用参数
3. Dapper 自动处理参数化，数据库驱动负责转义

### 5.2 唯一参数名生成

```csharp
private static int _parameterIndex = 0;

protected string GetParameterName(string fieldName)
{
    return $"@p_{fieldName}_{Interlocked.Increment(ref _parameterIndex)}";
}
```

**保证唯一性**：
- 使用静态计数器 `_parameterIndex`
- 使用 `Interlocked.Increment` 保证线程安全
- 每次调用自动递增，确保参数名不重复

**示例**：
```
@p_Name_1
@p_Age_2
@p_Name_3  // 同一字段多次使用，索引递增
```

### 5.3 类型转换机制

```csharp
// 用户传入 object 类型
object value = "123";

// 根据 ValueType 转换为目标类型
var convertedValue = TypeConvertHelper.ConvertToTargetType(value, typeof(int));
// 结果：123 (int 类型)
```

**支持的类型**：
- 基础类型：`int`, `long`, `decimal`, `double`, `float`, `bool`, `string`, `DateTime`
- 可空类型：`int?`, `long?`, `DateTime?` 等
- 枚举类型：支持字符串和数值转换

---

## 6. SQL构建流程

### 6.1 完整流程图

```
用户调用 BuilderSqlQueryStatementGeneric()
              │
              ▼
    ┌─────────────────────┐
    │  1. 字段名验证      │
    │  - 验证表名         │
    │  - 验证查询字段     │
    │  - 验证WHERE字段    │
    │  - 验证排序字段     │
    └─────────┬───────────┘
              │
              ▼
    ┌─────────────────────┐
    │  2. 构建SELECT子句   │
    │  SELECT f1,f2...    │
    │  FROM tableName     │
    │  WHERE 1=1          │
    └─────────┬───────────┘
              │
              ▼
    ┌─────────────────────┐
    │  3. 构建WHERE子句    │
    │  遍历 sqlWhereClauses│
    │  └─> 工厂创建操作对象│
    │      └─> 生成参数化SQL│
    └─────────┬───────────┘
              │
              ▼
    ┌─────────────────────┐
    │  4. 构建IN/NOT IN   │
    │  SpecialHandlerIn   │
    │  Operator()         │
    └─────────┬───────────┘
              │
              ▼
    ┌─────────────────────┐
    │  5. 构建ORDER BY    │
    │  ORDER BY f1 ASC,   │
    │  f2 DESC            │
    └─────────┬───────────┘
              │
              ▼
    ┌─────────────────────┐
    │  6. 构建分页         │
    │  LIMIT {0}          │
    │  OFFSET {1}         │
    └─────────┬───────────┘
              │
              ▼
    返回 (SQL, Parameters)
```

### 6.2 WHERE 子句构建详细流程

```
SqlWhereClauseHelper.SplicingWhereConditionSql()
              │
              ▼
    ┌─────────────────────┐
    │  是否有嵌套条件？    │
    └─────────┬───────────┘
         Yes /      \ No
          /          \
         ▼            ▼
    ┌─────────┐  ┌─────────────────┐
    │ 递归处理 │  │ 根据 MatchOperator │
    │ 子条件   │  │ 分派处理：        │
    │         │  │ • Between        │
    └─────────┘  │ • Like/NotLike   │
                 │ • 其他操作符      │
                 └─────────┬─────────┘
                           │
                           ▼
                 ┌─────────────────┐
                 │ SqlOperationFactory│
                 │ .CreateSqlOperation()│
                 └─────────┬─────────┘
                           │
                           ▼
                 ┌─────────────────┐
                 │ 调用具体操作的   │
                 │ GetSqlSentenceResult() │
                 │ • 添加参数到 DynamicParameters │
                 │ • 返回参数化SQL片段 │
                 └─────────┬─────────┘
                           │
                           ▼
                 返回 SQL 片段和参数
```

### 6.3 示例：构建复杂查询

**输入**：
```csharp
var whereClauses = new List<SqlWhereClauseInfoDto>
{
    new("Status", MatchOperator.Equal, 1),
    new("Name", MatchOperator.Like, "%John%"),
    new("Age", MatchOperator.GreaterThan, 25)
};
```

**生成过程**：

1. **SELECT 子句**：
```sql
SELECT Id, Name, Age FROM Users WHERE 1=1
```

2. **处理第一个条件 (Status = 1)**：
   - 创建 `SqlEqualOperation`
   - 生成参数名：`@p_Status_1`
   - 添加参数：`parameters.Add("@p_Status_1", 1)`
   - 生成 SQL：` AND Status = @p_Status_1`

3. **处理第二个条件 (Name LIKE '%John%')**：
   - 创建 `SqlLikeOperation`
   - 生成参数名：`@p_Name_2`
   - 添加参数：`parameters.Add("@p_Name_2", "%John%")`
   - 生成 SQL：` AND Name LIKE @p_Name_2`

4. **处理第三个条件 (Age > 25)**：
   - 创建 `SqlGreaterThanOperation`
   - 生成参数名：`@p_Age_3`
   - 添加参数：`parameters.Add("@p_Age_3", 25)`
   - 生成 SQL：` AND Age > @p_Age_3`

**最终 SQL**：
```sql
SELECT Id, Name, Age FROM Users WHERE 1=1 AND Status = @p_Status_1 AND Name LIKE @p_Name_2 AND Age > @p_Age_3
```

**参数**：
```csharp
{
    "@p_Status_1": 1,
    "@p_Name_2": "%John%",
    "@p_Age_3": 25
}
```

---

## 7. 安全机制

### 7.1 多层防护

```
┌──────────────────────────────────────────────────────────┐
│                    第一层：字段名验证                      │
│  • 正则表达式检查                                         │
│  • SQL 关键字黑名单                                       │
│  • 可疑模式检查                                           │
└──────────────────────┬───────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                    第二层：参数化查询                       │
│  • Dapper.DynamicParameters 自动转义                      │
│  • 所有值通过参数传递                                      │
│  • 数据库驱动层防护                                        │
└──────────────────────┬───────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                    第三层：类型安全                         │
│  • 强类型泛型约束                                          │
│  • 显式类型转换                                           │
│  • 编译时类型检查                                          │
└──────────────────────────────────────────────────────────┘
```

### 7.2 字段名验证详解

**正则表达式**：`^[a-zA-Z_.][a-zA-Z0-9_.]*$`

**允许的格式**：
- `UserName` ✅
- `user_name` ✅
- `_privateField` ✅
- `Table.Column` ✅

**拒绝的格式**：
- `123column` ❌ (数字开头)
- `user-name` ❌ (包含连字符)
- `user name` ❌ (包含空格)
- `DROP;TABLE` ❌ (包含分号)
- `column'OR'1'='1` ❌ (包含单引号)

### 7.3 参数化查询示例

**恶意输入测试**：
```csharp
var userInput = "'; DROP TABLE Users; --";

// 即使输入包含 SQL 注入代码，也能安全处理
var clause = new SqlWhereClauseInfoDto("Name", MatchOperator.Equal, userInput);
```

**生成的 SQL**：
```sql
SELECT * FROM Users WHERE Name = @p_Name_1
```

**参数值**：
```csharp
{ "@p_Name_1": "'; DROP TABLE Users; --" }  // 作为字符串值传递，不执行
```

---

## 8. 扩展点

### 8.1 添加新的操作符

**步骤**：

1. 在 `MatchOperator` 枚举中添加新操作符
```csharp
public enum MatchOperator
{
    // ... 现有操作符
    IsNull,  // 新增：IS NULL 操作符
}
```

2. 创建新的操作类
```csharp
public class SqlIsNullOperation : SqlOperation
{
    public override string GetSqlSentenceResult(
        string fieldName, object fieldValue,
        DynamicParameters parameters, Type valueType)
    {
        return $" {fieldName} IS NULL ";
    }
}
```

3. 在 `SqlOperationFactory` 中注册
```csharp
public static SqlOperation CreateSqlOperation(MatchOperator operation)
{
    return operation switch
    {
        // ... 现有操作符
        MatchOperator.IsNull => new SqlIsNullOperation(),
        _ => throw new NotSupportedException("不支持的类型")
    };
}
```

### 8.2 支持新数据库方言

**PostgreSQL（当前实现）**：
```sql
field = ANY(@param)  -- IN 操作
```

**SQL Server**：
```sql
field IN (SELECT value FROM OPENJSON(@param))
-- 或者
field IN @param  -- SQL Server 2016+
```

**MySQL**：
```sql
field IN @param
```

---

## 附录

### A. 数据库兼容性

| 功能 | PostgreSQL | SQL Server | MySQL | Oracle |
|------|-----------|------------|-------|--------|
| 参数化 IN | `= ANY(@p)` | `IN @p` | `IN @p` | `IN @p` |
| 分页 | `LIMIT/OFFSET` | `OFFSET/FETCH` | `LIMIT/OFFSET` | `OFFSET/FETCH` |
| 参数命名 | `@p_name` | `@p_name` | `?` 或 `@p_name` | `:p_name` |

### B. 性能考虑

1. **正则表达式**：使用 `RegexOptions.Compiled` 提升性能
2. **字符串拼接**：使用 `StringBuilder` 避免频繁内存分配
3. **线程安全**：使用 `Interlocked.Increment` 替代锁
4. **参数复用**：`DynamicParameters` 支持参数复用

### C. 未来改进方向

1. 支持更多数据库方言
2. 支持 JOIN 子句构建
3. 支持 GROUP BY 和 HAVING
4. 支持子查询
5. 提供 LINQ 表达式树支持
6. 添加查询缓存机制

---

**文档版本**: 1.0.0
**最后更新**: 2026-02-17
**维护者**: Azrng
