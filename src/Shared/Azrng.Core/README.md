# Azrng.Core

> 💎 **高效、可靠的.NET核心工具库** - 为您的应用提供通用扩展方法和帮助类

[![NuGet](https://img.shields.io/nuget/v/Azrng.Core)](https://www.nuget.org/packages/Azrng.Core/)
[![License](https://img.shields.io/github/license/azrng/nuget-packages)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-%3E%3D%206.0-purple.svg)](https://docs.microsoft.com/en-us/dotnet/core/)

## 📖 功能特性

### 核心功能
- **扩展方法**: 丰富的 .NET 类型扩展方法
- **帮助类**: 常用功能的工具类封装
- **类型转换**: 灵活的类型转换工具
- **集合操作**: 强大的集合处理能力
- **日志系统**: 本地文件日志支持
- **结果包装**: 统一返回模型、结果工厂、异常转换与数据安全读取

### 跨平台支持
- 支持 .NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 📦 安装

通过 NuGet 安装:

```
Install-Package Azrng.Core
```

或通过 .NET CLI:

```
dotnet add package Azrng.Core
```

## 🚀 快速开始

### 字符串扩展

```csharp
// 判断字符串类型
bool isInt = "123".IsIntFormat();           // true
bool isDecimal = "123.45".IsDecimalFormat(); // true
bool isDate = "2024-01-01".IsDateFormat();   // true
bool hasChinese = "你好".HasChinese();         // true

// 字符串验证
bool isNullOrEmpty = "".IsNullOrEmpty();           // true
bool isNotNullOrWhiteSpace = "  ".IsNotNullOrWhiteSpace(); // false

// 字符串操作
string[] array = "a,b,c".ToStrArray();                    // ["a", "b", "c"]
string indexed = "Hello".GetByIndex(1);                    // "e"
bool equals = "hello".EqualsNoCase("HELLO");               // true
```

### 集合扩展

```csharp
var list = new List<int> { 1, 2, 3, 4, 5 };

// 判断集合
bool isNullOrEmpty = list.IsNullOrEmpty();    // false
bool isNotNullOrEmpty = list.IsNotNullOrEmpty(); // true

// 条件过滤
var filtered = list.WhereIF(true, x => x > 2);  // [3, 4, 5]
var filtered2 = list.WhereIF(false, x => x > 2); // [1, 2, 3, 4, 5]

// 分页
var page = list.ToPage(1, 2);     // [1, 2]
var pageList = list.ToPageList(2, 2); // [3, 4]

// 带索引遍历
foreach (var (item, index) in list.WithIndex())
{
    Console.WriteLine($"{index}: {item}");
}

// 添加如果不存在
list.AddIfNotContains(6);  // 添加成功
list.AddIfNotContains(1);  // 添加失败（已存在）
```

### 日期时间扩展

```csharp
var now = DateTime.Now;

// 格式化
string standard = now.ToStandardString();      // "2024-01-01 12:00:00"
string dateOnly = now.ToDateString();          // "2024-01-01"
string iso = now.ToIsoDateTimeString();        // "2024-01-01T12:00:00.0000000"
string detailed = now.ToDetailedTimeString();  // "2024-01-01 12:00:00.0000000"

// 可空类型支持
DateTime? nullableDate = now;
string fromNullable = nullableDate.ToStandardString(); // "2024-01-01 12:00:00"

// 自定义格式
string custom = now.ToFormatString("yyyy年MM月dd日"); // "2024年01月01日"
```

### 对象扩展

```csharp
// 对象转 URL 参数
var user = new UserInfoDto { Id = 1, Name = "Test" };
string url = user.ToUrlParameter();              // "Id=1&Name=Test"
string urlLower = user.ToUrlParameter(true);     // "id=1&name=test"

// 对象转字典
var dict = user.ToDictionary();  // {"Id": "1", "Name": "Test"}
var dictLower = user.ToDictionary(true); // {"id": "1", "name": "Test"}

// 类型转换
int value = "123".To<int, int>(0);           // 123
DateTime date = "2024-01-01".To<string, DateTime>(DateTime.MinValue);
```

### 枚举扩展和帮助

```csharp
public enum Status
{
    [Description("激活")]
    Active,
    [Description("停用")]
    Inactive
}

// 获取描述
string description = Status.Active.GetDescription(); // "激活"

// 枚举帮助类
var enumDict = EnumHelper.EnumToDictionary<Status>();
// 结果: {0: "激活", 1: "停用"}

var keys = EnumHelper.GetKeys<Status>();
// 结果: ["Active", "Inactive"]

var values = EnumHelper.GetValues<Status>();
// 结果: [0, 1]

Status value = EnumHelper.GetEnumValue<Status>("激活"); // Status.Active
```

### 数字扩展

```csharp
decimal price = 1234.5678m;

// 格式化
string standard = price.ToStandardString(2);      // "1234.57"（四舍五入）
string noZero = price.ToNoZeroString(2);          // "1234.57"（不保留末尾0）
string fixed = price.ToFixedString(3);            // "1234.568"（保留末尾0）

// 取绝对值
decimal abs = (-123.45m).ToAbs();                 // 123.45
```

### 字节数组扩展

```csharp
byte[] data = new byte[] { 0x01, 0x02, 0x03 };

// 转换
string hex = data.ToHexString();                  // "010203"
string base64 = data.ToBase64();                  // "AQID"
Stream stream = data.ToStream();                  // 转为流
int number = new byte[] { 0x01, 0x00, 0x00, 0x00 }.ToInt32();  // 1

// 文件类型检测
byte[] fileBytes = File.ReadAllBytes("test.jpg");
string? contentType = fileBytes.GetContentType();  // "image/jpeg"
string? suffix = fileBytes.GetFileSuffix();        // ".jpg"
```

### 字典扩展

```csharp
var dict = new Dictionary<string, string>
{
    ["name"] = "张三",
    ["age"] = "25"
};

// 获取值或默认值
string name = dict.GetOrDefault("name", "默认值");  // "张三"
string email = dict.GetOrDefault("email", "");      // ""

// 根据列名获取值
string value = dict.GetColumnValueByName("name");   // "张三"
```

### StringBuilder 条件拼接

```csharp
var sb = new StringBuilder();

// 条件追加
sb.AppendIF(true, "Hello");                       // 追加
sb.AppendIF(false, "World");                      // 不追加
sb.AppendLineIF(true, "Line");                    // 追加并换行

// 非空追加
sb.AppendIfNotEmpty("Not Null");                  // 追加
sb.AppendIfNotEmpty(null);                        // 不追加
sb.AppendLineIfNotNullOrWhiteSpace("Text");       // 追加并换行
```

### 正则表达式扩展

```csharp
// 常用验证
bool isIdCard = "110101199001011234".IsIdCard();   // true
bool isPhone = "13800138000".IsPhone();           // true
bool isEmail = "test@example.com".IsEmail();      // true
bool isUrl = "https://example.com".IsUrl();       // true
bool isIP = "192.168.1.1".IsIPAddress();          // true

// 正则匹配
bool isMatch = "test123".IsMatch(@"\d+");         // true

// 转义为正则表达式
string escaped = "test.exe".ToRegex();            // "test\.exe"
```

### 异常扩展

```csharp
try
{
    // 可能抛出异常的代码
}
catch (Exception ex)
{
    string errorInfo = ex.GetExceptionAndStack();
    // "message：错误消息 stackTrace：堆栈跟踪 innerException：内部异常"
}
```

### 结果模型与异常转换

```csharp
// 成功结果，保留业务消息
var success = ResultModelFactory.Success(new[] { "A", "B" }, "加载成功");

// 失败结果，指定错误码
var failure = ResultModelFactory.Failure<string>("参数错误", "400");

// Azrng 异常转换为失败结果时会保留 ErrorCode
try
{
    throw new LogicBusinessException("业务规则不满足");
}
catch (Exception ex)
{
    IResultModel<bool> result = ex.ToFailureResult<bool>();
}

// 普通异常如需隐藏内部细节，可传入对外消息
var safeResult = new InvalidOperationException("internal details")
    .ToFailureResult<bool>("操作失败");

// 安全读取数据，避免调用方重复处理 null
IResultModel<List<int>> listResult = ResultModel<List<int>>.Failure("查询失败");
List<int> rows = listResult.DataOrEmpty();

IResultModel<string> valueResult = ResultModel<string>.Failure("查询失败");
string value = valueResult.DataOrDefault("默认值");
```

### 随机数扩展

```csharp
var random = new Random();

// 随机小数
double rnd = random.NextDouble(1.0, 10.0);        // 1.0 ~ 10.0

// 随机选择
var list = new List<string> { "A", "B", "C", "D" };
string item = random.NextItem(list);              // 随机返回一个
var items = random.NextItems(list, 2);            // 随机返回两个
```

### IQueryable 扩展

```csharp
var query = context.Users.AsQueryable();

// 动态排序
var sorted = query.OrderBy(x => x.Name, true);     // 升序
var sorted2 = query.OrderBy(x => x.Name, false);  // 降序

// 根据字符串排序
var sorted3 = query.OrderBy("Name", true);        // 升序

// 查询映射
var dtos = query.SelectMapper<UserDto>();         // 自动映射属性
```

## 📚 API 参考

### 扩展方法

#### StringExtension
```csharp
// 类型判断
bool IsIntFormat(this string str)                         // 判断是否是整数
bool IsDecimalFormat(this string str)                     // 判断是否是decimal
bool IsDateFormat(this string str)                        // 判断是否是日期
bool HasChinese(this string str)                          // 判断是否包含中文

// 空值判断
bool IsNullOrEmpty(this string currentString)            // 是 null 或空
bool IsNotNullOrEmpty(this string currentString)         // 不是 null 或空
bool IsNullOrWhiteSpace(this string currentString)       // 是 null/空/空白
bool IsNotNullOrWhiteSpace(this string currentString)    // 不是 null/空/空白

// 字符串操作
string[] ToStrArray(this string str, string separator = ",")  // 分割成数组
StringBuilder AppendIF(this StringBuilder builder, bool condition, string str)  // 条件拼接
string GetByIndex(this string str, int index)             // 获取特定位置字符
bool EqualsNoCase(this string aimStr, string comparaStr)  // 忽略大小写比较
```

#### EnumerableExtension
```csharp
bool IsNullOrEmpty<T>(this IEnumerable<T>? source)                      // 是 null 或空
bool IsNotNullOrEmpty<T>(this IEnumerable<T>? source)                   // 不是 null 或空
IEnumerable<T> WhereIF<T>(this IEnumerable<T>? source, bool condition, Func<T, bool> predicate)  // 条件过滤
IEnumerable<T> ToPage<T>(this IEnumerable<T>? source, int pageIndex, int pageSize)  // 分页
List<T> ToPageList<T>(this IEnumerable<T>? source, int pageIndex, int pageSize)  // 分页转List
T[] ToPageArray<T>(this IEnumerable<T>? source, int pageIndex, int pageSize)  // 分页转Array
Dictionary<string, string> AsciiDictionary(this Dictionary<string, string> dic)  // ASCII排序
IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)  // 带索引遍历
```

#### CollectionExtension
```csharp
bool AddIfNotContains<T>(this ICollection<T> source, T item)  // 如果不存在则添加
```

#### DateTimeExtension
```csharp
// 格式化方法
string ToStandardString(this DateTime time)                   // "yyyy-MM-dd HH:mm:ss"
string ToStandardString(this DateTime? time)                  // 可空版本
string ToDateString(this DateTime time)                       // "yyyy-MM-dd"
string ToDateString(this DateTime? time)                      // 可空版本
string ToIsoDateTimeString(this DateTime dateTime)            // ISO 8601 格式
string ToIsoDateTimeString(this DateTime? dateTime)           // 可空版本
string ToDetailedTimeString(this DateTime time)               // "yyyy-MM-dd HH:mm:ss.fffffff"
string ToDetailedTimeString(this DateTime? time)              // 可空版本
string ToFormatString(this DateTime time, string format)      // 自定义格式
```

#### ObjectExtension
```csharp
string ToUrlParameter<T>(this T source, bool paramLower = false)  // 转URL参数
Dictionary<string, string> ToDictionary<T>(this T source, bool paramLower = false)  // 转字典
TTo To<TFrom, TTo>(this TFrom from, TTo defaultValue)            // 类型转换
```

#### UrlExtension
```csharp
string? UrlEncode(this string? target)                           // URL编码
string? UrlEncode(this string? target, Encoding encoding)        // 指定编码
string? UrlDecode(this string? target)                           // URL解码
string? UrlDecode(this string? target, Encoding encoding)        // 指定编码
string AttributeEncode(this string target)                       // HTML属性编码
string HtmlEncode(this string target)                            // HTML编码
string HtmlDecode(this string target)                            // HTML解码
```

#### EnumExtension
```csharp
string GetDescription(this Enum value)                           // 获取Description特性
string GetEnglishDescription(this Enum value)                    // 获取英文描述
```

#### QueryableExtension
```csharp
// 查询映射
IQueryable<Tm> SelectMapper<T, Tm>(this IQueryable<T> queryable)  // 查询映射

// 排序
IQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> query, Expression<Func<TSource, TKey>> keySelector, bool isAsc)  // 根据条件排序
IQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> query, Expression<Func<TSource, TKey>> keySelector, SortEnum sortEnum)  // 根据枚举排序
IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, params SortContent[] orderContent)  // 多列排序
IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, SortContent orderContent)  // 单列排序
IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, string sortField, bool isAsc)  // 根据字段名排序
```

#### ByteExtension
```csharp
// 转换方法
string ToHexString(this byte[] bytes)                           // 转十六进制字符串
string ToBase64(this byte[] bytes)                              // 转Base64字符串
Stream ToStream(this byte[] bytes)                              // 转为Stream
int ToInt32(this byte[] data)                                   // 转为int32

// 文件操作
string? GetFileSuffix(this byte[] bytes)                        // 获取文件后缀
string? GetContentType(this byte[] bytes)                       // 获取Content-Type
string? GetFileCode(this byte[] bytes)                          // 获取文件编码
```

#### DictionaryExtension
```csharp
TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key, TValue defaultValue)  // 获取或返回默认值
string? GetColumnValueByName(this IDictionary<string, string>? keyValues, string? key)  // 获取string列值
string GetColumnValueByName(this IDictionary<string, object?>? keyValues, string? key)  // 获取object列值
T? GetColumnValueByName<T>(this IDictionary<string, object?>? keyValues, string? key)  // 获取指定类型列值
```

#### DecimalExtension
```csharp
// 格式化
string ToStandardString(this decimal dec, int number = 2)       // 转标准格式（四舍五入）
string ToStandardString(this decimal? dec, int number = 2)      // 可空版本
string ToNoZeroString(this decimal dec, int number = 2)         // 不保留小数点后的0
string ToNoZeroString(this decimal? dec, int number = 2)        // 可空版本
string ToFixedString(this decimal dec, int number = 2)          // 保留几位小数（保留结尾0）
string ToFixedString(this decimal? dec, int number = 2)         // 可空版本

// 运算
decimal ToAbs(this decimal @decimal)                            // 取绝对值
```

#### DoubleExtension
```csharp
// 格式化
string ToStandardString(this double dec, int number = 2)        // 转标准格式（四舍五入）
string ToStandardString(this double? dec, int number = 2)       // 可空版本
string ToNoZeroString(this double dec, int number = 2)          // 不保留小数点后的0
string ToNoZeroString(this double? dec, int number = 2)         // 可空版本
string ToFixedString(this double dec, int number = 2)           // 保留几位小数（保留结尾0）
string ToFixedString(this double? dec, int number = 2)          // 可空版本

// 运算
double ToAbs(this double @decimal)                              // 取绝对值
```

#### StringBuilderExtension
```csharp
// 条件拼接
StringBuilder AppendIF(this StringBuilder builder, bool condition, string? str)  // 条件追加
StringBuilder AppendLineIF(this StringBuilder builder, bool condition, string? str)  // 条件追加并换行
StringBuilder AppendFormatIF(this StringBuilder builder, bool condition, string format, params object[] args)  // 条件格式化追加

// 非空判断追加
StringBuilder AppendLineIfNotEmpty(this StringBuilder builder, string? str)  // 追加并换行（如果不为空）
StringBuilder AppendLineIfNotNullOrWhiteSpace(this StringBuilder builder, string? str)  // 追加并换行（如果不为空或空白）
StringBuilder AppendIfNotEmpty(this StringBuilder builder, string? str)  // 追加（如果不为空）
StringBuilder AppendIfNotNullOrWhiteSpace(this StringBuilder builder, string? str)  // 追加（如果不为空或空白）
```

#### RegexExtension
```csharp
// 正则操作
string ToRegex(this string value)                               // 转为正则表达式可识别字符串
bool IsMatch(this string input, string pattern, RegexOptions options)  // 验证是否匹配
bool IsMatch(this string inputStr, string patternStr)           // 验证是否匹配
bool IsMatch(this string inputStr, string patternStr, bool isIgnoreSpace, bool isIgnoreCase)  // 验证是否匹配（带选项）

// 常用验证
bool IsIdCard(this string? idCard)                             // 验证身份证是否合法
bool IsPhone(this string? cellPhone)                           // 验证手机格式是否合法
bool IsEmail(this string? email)                               // 验证邮箱格式是否合法
bool IsUrl(this string? url)                                   // 验证URL格式是否合法
bool IsIPAddress(this string? ip)                              // 验证IP地址格式是否合法
```

#### JsonExtension
```csharp
bool IsJArrayString(this string jsonStr)                        // 验证是否是JArray字符串
```

#### TypeExtension
```csharp
T? CustomAttributeCommon<T>(this Type sourceType, string? fieldName) where T : Attribute  // 获取自定义特性
Dictionary<Enum, T> ToEnumAndAttributes<T>(this Type type) where T : Attribute  // 获取枚举值和对应的特性
```

#### ExceptionExtension
```csharp
string GetExceptionAndStack(this Exception ex)                  // 获取异常详细信息包含堆栈
```

#### ResultModelExtensions
```csharp
IReadOnlyList<T> DataOrEmpty<T>(this IResultModel<IReadOnlyList<T>> result)  // 数据为空时返回空只读列表
List<T> DataOrEmpty<T>(this IResultModel<List<T>> result)                    // 数据为空时返回空列表
T DataOrDefault<T>(this IResultModel<T> result, T defaultValue)              // 数据为空时返回默认值
```

#### ResultModelExceptionExtensions
```csharp
IResultModel<T> ToFailureResult<T>(this Exception exception, string? message = null)  // 异常转换为失败结果
```

#### RandomExtension
```csharp
double NextDouble(this Random random, double minValue, double maxValue)  // 生成指定范围随机小数
T NextItem<T>(this Random random, IList<T> list)               // 随机返回列表中的一项
IEnumerable<T> NextItems<T>(this Random random, IList<T> list, int count)  // 随机返回列表中的多项
```

#### TreeExtension
```csharp
IEnumerable<T> Flatten<T>(this T node, Func<T, IEnumerable<T>> childrenSelector)  // 将树结构展平为一维
IEnumerable<T> GetAncestors<T>(this T node, Func<T, T> parentSelector)  // 获取所有祖先节点
IEnumerable<T> GetDescendants<T>(this T node, Func<T, IEnumerable<T>> childrenSelector)  // 获取所有后代节点
```

#### IListExtension
```csharp
void SortBy<T>(this IList<T> list, string propertyName, bool ascending = true)  // 根据属性名排序
```

#### DataTableExtension
```csharp
List<T> ToList<T>(this DataTable table)                        // DataTable转List
```

#### MimeExtension
```csharp
string GetMimeType(this string fileName)                        // 根据文件名获取MIME类型
string GetMimeType(this byte[] fileBytes)                      // 根据文件字节获取MIME类型
```

#### PersonExtension
```csharp
int GetAge(this DateTime birthDate)                            // 根据出生日期获取年龄
string GetChineseZodiac(this DateTime birthDate)               // 获取生肖
string GetZodiacSign(this DateTime birthDate)                  // 获取星座
```

#### AssemblyExtension
```csharp
IEnumerable<Type> GetLoadableTypes(this Assembly assembly)     // 获取可加载的类型
IEnumerable<T> GetInstances<T>(this Assembly assembly)         // 获取程序集中所有T实例
```

#### ExpressExtension
```csharp
string GetPropertyName<T>(Expression<Func<T, object>> expression)  // 获取属性名称
string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)  // 获取属性名称
```

### 帮助类

#### LocalLogHelper
```csharp
// 同步方法
void LogInformation(string message)                              // 记录信息日志
void LogWarning(string message)                                  // 记录警告日志
void LogError(string message)                                    // 记录错误日志

// 异步方法
Task WriteMyLogsAsync(string level, string message)              // 异步记录日志
Task FlushAsync()                                                // 立即写入当前队列中的日志

// 配置
CoreGlobalConfig.MinimumLevel = LogLevel.Warning;                // 设置最小日志级别
CoreGlobalConfig.LogRetentionDays = 7;                          // 日志保留天数（默认7天）
```

**使用示例：**
```csharp
// 基本使用
LocalLogHelper.LogInformation("这是一条信息日志");
LocalLogHelper.LogWarning("这是一条警告日志");
LocalLogHelper.LogError("这是一条错误日志");

// 异步使用
await LocalLogHelper.WriteMyLogsAsync("Info", "这是一条测试日志");
await LocalLogHelper.FlushAsync();

// 配置日志级别（只输出Warning及以上级别）
CoreGlobalConfig.MinimumLevel = LogLevel.Warning;
```

#### RetryHelper
```csharp
// 基本重试
Task<T> ExecuteAsync<T>(Func<Task<T>> func, int maxRetryCount = 3)
Task<T> ExecuteAsync<T>(Func<Task<T>> func, int maxRetryCount, TimeSpan delay)
Task<T> ExecuteAsync<T>(Func<Task<T>> func, int maxRetryCount, Func<int, TimeSpan> delayStrategy)

// 条件重试
IRetryContext<T> WhenCatch<TException>(IRetryContext<T> context) where TException : Exception
IRetryContext<T> WhenResult<T>(IRetryContext<T> context, Func<T, bool> predicate)
```

**使用示例：**
```csharp
// 简单重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3);

// 带延时重试
var result = await RetryHelper.ExecuteAsync(
    () => GetDataAsync(),
    3,
    TimeSpan.FromSeconds(2)
);

// 指数退避重试
var result = await RetryHelper.ExecuteAsync(
    () => GetDataAsync(),
    3,
    i => TimeSpan.FromSeconds(Math.Pow(2, i))
);

// 条件重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenCatch<HttpRequestException>();

// 基于结果重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenResult(data => data == null);
```

#### CollectionHelper
```csharp
// 分页处理集合
Task<int> ExecuteCollectionInPagesAsync<T>(ICollection<T> collection, Func<ICollection<T>, int, Task<int>> processFunc, int pageSize = 5000)

// 批次执行
int ExecuteInBatches(int totalNumber, Func<int, int> processFunc, int batchSize = 1000)
Task<int> ExecuteInBatchesAsync(int totalNumber, Func<int, Task<int>> processFunc, int batchSize = 1000)

// 随机删除
T RemoveRandomItem<T>(IList<T> list)
```

#### EnumHelper
```csharp
T GetEnumValue<T>(string description)                           // 根据描述获取枚举值
Dictionary<int, string> EnumToDictionary<T>() where T : Enum    // 枚举转字典
List<string> GetKeys<TEnum>()                                   // 获取所有键
List<int> GetValues<TEnum>()                                    // 获取所有值
```

#### StringHelper
```csharp
string CompressText(string originStr)                           // 压缩文本
string TextToUnicode(string text)                               // 文本转Unicode
string UnicodeToText(string unicode)                            // Unicode转文本
string ReplaceWithDictionary(string input, Dictionary<string, string> replacements)  // 批量替换
string ReplaceWithOrder(string input, Dictionary<string, string> replacements)      // 有序批量替换
```

#### FileHelper
```csharp
string FormatFileSize(long bytes)                               // 格式化文件大小
// 示例: 1024 -> "1.00 KB", 1048576 -> "1.00 MB"
```

#### DateTimeHelper
```csharp
string GetTimeDifferenceText(DateTime dateTime)                 // 获取时间差文本
// 示例: "刚刚", "5分钟前", "2小时前", "3天前"
```

#### ObjectHelper
```csharp
T ConvertToTargetType<T>(object value)                         // 转换为目标类型
T? ConvertToTargetTypeOrNull<T>(object value)                  // 转换为目标类型（可空）
```

#### UrlHelper
```csharp
string AddQueryString(string url, Dictionary<string, string> queryString)  // 添加查询字符串
string SortUrlParameters(string url)                           // 排序URL参数
string ExtractUrl(string url)                                  // 提取URL
```

#### DateTimeHelper
```csharp
string GetTimeDifferenceText(DateTime dateTime)                // 获取时间差文本
DateTime GetStartOfWeek(DateTime date)                         // 获取周开始
DateTime GetEndOfWeek(DateTime date)                           // 获取周结束
DateTime GetStartOfMonth(DateTime date)                        // 获取月开始
DateTime GetEndOfMonth(DateTime date)                          // 获取月结束
```

#### IPHelper
```csharp
string GetLocalIPAddress()                                     // 获取本机IP地址
string GetLocalIPv4Address()                                   // 获取IPv4地址
string GetLocalIPv6Address()                                   // 获取IPv6地址
bool IsValidIPAddress(string ipAddress)                        // 验证IP地址是否有效
```

#### SystemHelper
```csharp
long GetTotalMemory()                                          // 获取总内存
long GetAvailableMemory()                                      // 获取可用内存
long GetUsedMemory()                                           // 获取已用内存
string GetOSInfo()                                             // 获取操作系统信息
string GetMachineName()                                        // 获取机器名
```

#### ApplicationHelper
```csharp
string GetApplicationVersion()                                 // 获取应用版本
string GetRuntimeInfo()                                        // 获取运行时信息
```

#### AsyncHelper
```csharp
void RunSync(Func<Task> task)                                 // 同步执行异步方法
T RunSync<T>(Func<Task<T>> task)                              // 同步执行异步方法（带返回值）
```

#### TaskHelper
```csharp
Task RunTimeLimitAsync(Func<Task> task, TimeSpan timeout)      // 运行带时间限制的任务
Task<T> RunTimeLimitAsync<T>(Func<Task<T>> task, TimeSpan timeout)  // 运行带时间限制的任务（带返回值）
```

#### TimerHelper
```csharp
void SetInterval(Action action, int interval)                  // 设置定时器
void SetTimeout(Action action, int delay)                      // 设置延迟执行
```

#### CodeTimerHelper
```csharp
TimeSpan Time(Action action)                                   // 计时执行操作
T Time<T>(Func<T> func)                                       // 计时执行操作（带返回值）
```

#### CompressHelper
```csharp
byte[] Compress(byte[] data)                                   // 压缩数据
byte[] Decompress(byte[] data)                                 // 解压数据
string CompressString(string text)                             // 压缩字符串
string DecompressString(string compressedText)                 // 解压字符串
```

#### CronHelper
```csharp
bool IsValidCronExpression(string cronExpression)              // 验证Cron表达式是否有效
DateTime? GetNextOccurrence(string cronExpression, DateTime baseTime)  // 获取下次执行时间
List<DateTime> GetNextOccurrences(string cronExpression, DateTime baseTime, int count)  // 获取接下来的执行时间
```

#### CsvHelper
```csharp
string ToCsv<T>(List<T> data)                                  // 转为CSV格式
List<T> FromCsv<T>(string csv)                                // 从CSV格式解析
```

#### DbTypeMapHelper
```csharp
string GetDbType(Type clrType)                                // 获取数据库类型
Type GetClrType(string dbType)                                // 获取CLR类型
```

#### HtmlHelper
```csharp
string HtmlToText(string html)                                // HTML转纯文本
string StripHtml(string html)                                  // 移除HTML标签
string EncodeHtml(string text)                                 // HTML编码
string DecodeHtml(string html)                                 // HTML解码
```

#### IOHelper
```csharp
void EnsureDirectoryExists(string path)                        // 确保目录存在
void CopyDirectory(string source, destination)                 // 复制目录
void DeleteDirectory(string path, bool recursive)             // 删除目录
void MoveDirectory(string source, destination)                 // 移动目录
```

#### NumberHelper
```csharp
string ToChineseNumber(int number)                             // 转中文数字
string ToChineseNumber(decimal number)                         // 转中文数字（小数）
string ToChineseCurrency(decimal number)                       // 转中文大写金额
```

#### RMBHelper
```csharp
string ToRMB(decimal amount)                                   // 转人民币大写
string ToRMB(double amount)                                    // 转人民币大写
```

#### SqlHelper
```csharp
bool HasSqlInjectionRisk(string input)                         // 检测SQL注入风险
string EscapeSqlString(string input)                           // 转义SQL字符串
string SanitizeSqlInput(string input)                          // 清理SQL输入
```

#### ChineseTextHelper
```csharp
int GetChineseStringLength(string text)                        // 获取中文字符串长度
string GetChinesePinyin(string chinese)                        // 获取中文拼音
bool IsAllChinese(string text)                                 // 是否全部是中文
```

#### ConsoleHelper
```csharp
void WriteInfoLine(string message)                             // 输出信息
void WriteWarningLine(string message)                          // 输出警告
void WriteErrorLine(string message)                            // 输出错误
void WriteSuccessLine(string message)                          // 输出成功
string? ReadLineWithPrompt(string prompt)                      // 带提示符读取输入
ConsoleKeyInfo ReadKeyWithPrompt(string prompt)                // 带提示符读取按键
```

#### AssemblyHelper
```csharp
List<Assembly> GetAllAssemblies()                              // 获取所有程序集
List<Type> GetAllTypes()                                      // 获取所有类型
List<Type> GetTypesWithInterface<T>()                          // 获取实现指定接口的类型
```

#### ChinaDateHelper
```csharp
string GetChineseDate(DateTime date)                           // 获取中文日期
string GetChineseDayOfWeek(DateTime date)                      // 获取中文星期
bool IsLunarLeapMonth(int year, int month)                     // 是否是农历闰月
```

### 其他工具类

- `CommonHelper` - 通用帮助方法
- `GuardClause` - 参数验证帮助类（Guard.Against.Null等）
- `MimeExtensions` - MIME类型扩展
- `RandomExtensions` - 随机数扩展
- `TreeExtensions` - 树结构扩展
- `IListExtensions` - IList扩展
- `DataTableExtensions` - DataTable扩展
- `PersonExtensions` - 个人信息扩展
- `AssemblyExtensions` - 程序集扩展
- `ExpressExtensions` - 表达式树扩展
- `PredicateExtensions` - 谓词扩展
- `RetryTaskExtensions` - 重试任务扩展

## 🎯 公共返回类

封装了统一的返回结果类

```csharp
// 接口定义
public interface IResultModel
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    string Code { get; }
    string Message { get; }
    IEnumerable<ErrorInfo> Errors { get; }
}

public interface IResultModel<T> : IResultModel
{
    T? Data { get; set; }
}

// 具体实现
public class ResultModel : IResultModel
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; }
    public string Code { get; set; }
    public string Message { get; set; }
    public IEnumerable<ErrorInfo> Errors { get; }
}

public class ResultModel<T> : IResultModel<T>
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; }
    public string Code { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<ErrorInfo> Errors { get; }
}
```

### 使用示例

#### 返回成功
```csharp
[HttpGet]
public IResultModel<IEnumerable<WeatherForecast>> Get()
{
    var result = Enumerable.Range(1, 3).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    }).ToArray();

    return ResultModel<IEnumerable<WeatherForecast>>.Success(result, "查询成功");
}
```

**返回结果：**
```json
{
  "data": [
    {
      "date": "2024-01-01T12:00:00",
      "temperatureC": 52,
      "temperatureF": 125,
      "summary": "Freezing"
    }
  ],
  "isSuccess": true,
  "code": "200",
  "message": "查询成功",
  "errors": []
}
```

#### 返回错误
```csharp
[HttpGet]
public IResultModel<IEnumerable<WeatherForecast>> Get()
{
    return ResultModel<IEnumerable<WeatherForecast>>.Error("参数为空", "400");
}
```

**返回结果：**
```json
{
  "data": null,
  "isSuccess": false,
  "code": "400",
  "message": "参数为空",
  "errors": []
}
```

#### 统一工厂与异常转换
```csharp
public IResultModel<bool> Save()
{
    try
    {
        // 保存业务数据
        return ResultModelFactory.Success(true, "保存成功");
    }
    catch (BaseException ex)
    {
        return ex.ToFailureResult<bool>();
    }
    catch (Exception ex)
    {
        return ex.ToFailureResult<bool>("保存失败");
    }
}
```

## 📝 更新记录

完整版本历史见 [CHANGELOG.md](https://github.com/azrng/nuget-packages/blob/master/src/Shared/Azrng.Core/CHANGELOG.md)。


## 📝 当前版本更新

### 2.0.0
- Snowflake 新增 WorkerId 配置、范围校验和生成后的切换保护
- Snowflake 默认 WorkerId 由随机数改为机器标识（非回环 IPv4，无则机器名）稳定哈希 + 进程 Id（右移 2 位规避 Windows 进程 Id 按 4 对齐）派生，大幅降低多机/多进程默认值撞号导致雪花 Id 重复的概率，但不能保证唯一；分布式部署仍建议显式 `ConfigureWorkerId`
- 修复 `PersonExtensions.GetSexByIdCard`：15 位身份证恒返回未知的缺陷，修正女性应返回 2（行为变更：原 18 位偶数返回 0 现返回 2）
- 修复 `DictionaryExtension.GetColumnValueByName<T>` 对 Guid（含 `Guid?` 与 `Guid`）取值必抛 `InvalidCastException` 的问题，支持字符串形式 Guid 值
- 修复 `ByteExtensions.GetContentType`/`GetFileSuffix`/`GetRandomFileName` 遇未登记文件头抛 `InvalidOperationException` 的问题，统一返回 null（行为变更：`GetRandomFileName` 返回类型改为 `string?`）
- 修复 `ToFileByBytes`/`ToFile`/`ToFileAsync` 使用 `FileMode.OpenOrCreate` 不截断旧文件导致输出损坏的问题，改为 `FileMode.Create`
- 修复 `FromBase64Decode` 非法输入抛 `FormatException` 的问题，按容错契约返回空串
- 修复表达式合并工具（`Expressionable`/`PredicateExtensions`/`ExpressExtensions.MergeAnd`/`MergeOr`）参数替换器会错误替换嵌套 lambda（如 `Any`/`Where` 内层）参数导致表达式树非法的问题；`PredicateExpressionVisitor` 构造函数变更为 `(来源参数, 目标参数)` 或 `(来源参数集合, 目标参数)`（**破坏性变更**，同仓库 `Common.Db.Core` 内副本已同步修复）
- `ChinaDateHelper.GetChinaDate` 增加 1900-2049 支持范围校验，越界抛 `ArgumentOutOfRangeException`（原为索引崩溃）
- 修复 `RandomGenerator.GenerateIdCard` 指定女性时顺序码 999 加一产出 19 位非法身份证的问题
- `DateTimeHelper.GetNetworkTime` 改用共享 `HttpClient`（10 秒超时），避免每次调用新建实例导致 socket 耗尽；新增 `SetNetworkTimeUrl` 支持更换接口地址（仅接受 http/https 绝对地址，非法地址前置抛 `ArgumentException`，避免 `GetNetworkTime` 被未捕获的 `UriFormatException` 打穿）
- `TimerHelper.GetNetworkTime` 修复 NTP socket 异常路径句柄泄漏与 DNS 首地址为 IPv6 时的崩溃
- 修复 `CsvHelper.DateTableToCsv` 写入异常时 `StreamWriter` 句柄泄漏
- 修复 `IoExtensions.ToBytes`/`ToBytesAsync`/`ToFile`/`ToFileAsync` 单次 `Read` 当完整读取导致部分读流（网络流）数据截断的问题，改为循环读满
- `IoExtensions.ToBytesFromFileAsync`/`ToBytesAsync` 返回类型由可空 `Task<T>?` 修正为 `Task<T>`（async 方法不会返回 null 任务，仅可空性标注修正，无行为变更），消除调用方 `await` 处的可空警告
- `LocalLogHelper` 写盘链路全量 `ConfigureAwait(false)`，消除同步日志 API（`WriteMyLogs`/`LogTrace` 等）在同步上下文下的死锁风险
- `TaskHelper.RunTimeLimitAsync` 由 `task.Wait` 同步阻塞改为 `Task.WhenAny` 异步等待，失败传播原始异常（行为变更：不再包装为 `AggregateException`），修复超时后 `CancellationTokenSource` 不释放；任务先完成时立即取消释放超时计时器（原 `Task.Delay` 悬空占用计时器直至超时满），并移除对 func 不可达的取消令牌传递
- 重试任务（`Retry`/`HandleAsDefaultWhenException`）耗尽后携带原始异常作为 `InnerException`（原仅拼接消息文本）；取消异常（`OperationCanceledException`）不再被 `WhenCatch<Exception>` 吞掉转默认值或参与重试（行为变更：显式声明 `WhenCatch<OperationCanceledException>` 不受影响）
- 语义统一（行为变更）：`OrderBy(params SortContent[])` 多字段排序改为首键为主、后续 ThenBy（原为最后一个键为主，与 `FiledOrderParam[]` 版语义相反）；`GetTargetTimeStart/End` 的 Week 周日归入当周（原平移到下周一/下周日）；`CronHelper.Week/Month/Year` 统一输出 Quartz 6 字段（原为 5 字段，周数字按 Quartz 约定 1=周日）
- 异常 ErrorCode 统一（行为变更）：`ForbiddenException` 全部构造为 403（原无参 401）；`LogicBusinessException`/`ParameterException` 带 inner 构造为 400（原 500）；`NotFoundException` 全部构造为 404（原无参 500）；`BaseException` 新增 `(code, message, innerException)` 三参构造；`RetryMarkException.ThrowIfNull` 改抛自身类型（原误抛 `ParameterException`）
- 文化稳定性：`ToFormatString` 统一 InvariantCulture（it-IT 等文化下时间分隔符不再漂移为 "."）；`DoubleExtension.ToNoZeroString`、`GetFileFormatSize`、`GetFileCode` 补 InvariantCulture
- 舍入文档如实化：`ToStandardString` 明确为银行家舍入（四舍六入五成双），并修复测试用例中 double 字面量精度问题（该问题曾导致历史版本 1 例测试失败）
- `CodeTimerHelper`：`iteration<1` 抛 `ArgumentOutOfRangeException`（原除零崩溃）；非 Windows 平台跳过 kernel32 CPU 采样（原 `DllNotFoundException`）；移除静态构造中设置进程 High 优先级且从不恢复的副作用；`IAction` 重载委托 `ActionDelegate` 版消除重复实现
- `IpHelper.ToLongFromIp` 改用 `IPAddress.TryParse` + 四段校验，非法输入抛 `ArgumentException`（原对 null/段数不足/非数字分别抛 NRE/IndexOutOfRange/FormatException）
- `SqlHelper` 高风险模式拼写修正：`sp_executeexternalcript` → `sp_execute_external_script`（该项检测原本永不命中）
- `GetQueryPageResult` 对 `PageSize<=0` 防御性返回 `TotalPage=0`（原产生 Infinity/负数垃圾值）
- `TaskHelper.TryWaitAsync` 先执行后等待，超时窗口小于 1 秒时保证 func 至少执行一次（原一次不执行直接返回超时）
- 清理死代码：`ChinaDateHelper.Lunar`（与 `CalElement` 重复且含必抛构造）、`EnumHelper` 私有 `GetDescription`、`ByteExtensions.ToInt32` 重复长度检查、`MimeExtensions` 大写 `.IVF` 死条目；`GetTimestamp` 的 Obsolete 补充提示消息
- 文档承诺如实化：`ToStandardString` 等注释明确为银行家舍入（原注释“四舍五入”与 Math.Round 默认行为不符）
- 修复 `DataTableExtensions.ToEntity` 多行表静默覆盖为最后一行的问题，明确取首行语义（行为变更）；转换支持 Guid/枚举列（原 `Convert.ChangeType` 必抛）并统一 InvariantCulture
- `ObjectExtensions.ToUrlParameter` 键值 URL 编码（原值含 & = 中文时产出错位 query 串）
- `IListExtensions.ToDataTable(hasColumns: false)` 返回空表结构（行为变更：原走 `LoadDataRow` 意外抛 `ArgumentException`）
- `ExpressExtensions.MarkEqual` 与 null 值对象比较、`TProp` 无属性时抛明确 `ArgumentException`（原抛晦涩的表达式构建异常）
- `TaskHelper`/`CollectionHelper`/`ObjectHelper` 移除数据处理路径上的 `Console` 输出（行为变更：帮助类不再向控制台刷屏）
- `StringExtension`：`IsBool` 支持 "true"/"false"/"no"；`ToBytesFromHexString` 空串返回空数组（原返回含单字节的数组）
- `GuardClause` 空集合校验改用 `MoveNext`，不再消费一次性枚举的首个元素
- `EnumerableExtensions.ToPage` 大 pageIndex 乘法溢出防护
- `QueryableExtensions` 修正误导性参数报告（无效属性名/排序参数改抛 `ArgumentException` 并指向正确参数）
- `AssemblyHelper`：引用链加载失败跳过不中断；`GetType` 未找到类型抛明确异常；`GetStringType` 校验输入格式
- `HtmlHelper.HtmlToText`：script/style 正则加 Singleline（多行块的 JS/CSS 不再残留在文本里）；HTML 实体还原为对应字符（原替换为空串，`R&amp;D` 会变成 `RD`）（行为变更）
- 清理死代码：`CompressHelper`/`JsonExtensions` 大段注释尸体
- 补充上述修复的单元测试

## 📄 许可证

MIT License
