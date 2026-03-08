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
    string Code { get; }
    string Message { get; }
    List<string> Errors { get; }
}

public interface IResultModel<T> : IResultModel
{
    T Data { get; }
}

// 具体实现
public class ResultModel : IResultModel
{
    public bool IsSuccess { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}

public class ResultModel<T> : IResultModel<T>
{
    public bool IsSuccess { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
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

    return ResultModel<IEnumerable<WeatherForecast>>.Success(result);
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
  "message": "success",
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

## 📝 版本更新记录

### 1.15.6
- 修复IJsonSerializer的null问题

### 1.15.5
- 新增DictionaryExtension、DoublelExtension、ByteExtensions、CollectionExtensions、DataTableExtensions的完整单元测试
- 完善DateTimeExtension、DecimalExtension单元测试
- 修复ByteExtensions.GetFileCode方法IndexOutOfRangeException问题
- 修复QueryableExtensions.GreaterWhere方法比较类型错误
- 修复GetWeekOfMonth计算问题，增加WeekCalculationMode枚举支持两种周次计算方式
- 修复TimeSpan.ToDateTime方法ArgumentOutOfRangeException问题
- 修复MimeExtensions相关问题
- 新增GetWeekOfMonth方法重载，支持指定周计算模式

### 1.15.4
- 修复IResultModel的null引用

### 1.15.3
- 更新null引用问题

### 1.15.2
- 修复Nullable问题

### 1.15.1
- 启用Nullable

### 1.15.0
- 枚举增加GetEnglishDescription

### 1.14.0
- 增加StringBuilderExtension扩展

### 1.13.0
- 增加方法重试操作到RetryHelper

### 1.12.0
- StringHelper增加字符串批量替换方法
- 增加针对url的AddQueryString
- 修改Url的GetUrl为ExtractUrl

### 1.11.0
- 增加关于时间、字典、表达式树扩展方法
- GetCustomerObj变更为GetCustomerAttribute
- 变更CalculateDaysDifference改为扩展方法DateDiff

### 1.10.0
- 增加ObjectHelper.ConvertToTargetType类型转换方法
- 增加GetColumnValueByName的IDictionary<string, string>重载方法

### 1.9.0
- 未知

### 1.8.5
- 增加方法GetStringPropertiesWithValues
- 增加ToDetailedTimeString
- 增加ChineseTextHelper扩展类

### 1.8.4
- 增加ToGuid
- 优化sql检测类SqlHelper
- 支持.Net10

### 1.8.3
- IEnumerable增加ToPage方法

### 1.8.2
- 更新IJsonSerializer中ToJson泛型约束
- 增加计算相差的天数

### 1.8.1
- 将数字字符串转换为中文数字
- 增加CollectionHelper.RemoveRandomItem从集合中随机删除指定数量元素
- 增加sql注入检测方法SqlHelper.HasSqlInjectionRisk
- 更新帮助类CollectionHelper

### 1.8.0
- 增加一些EFCore相关模型类

### 1.8.0-beta4
- 更新BaseRequestDto，修改默认用户标识为字符串
- 新增BaseCustomerRequestDto

### 1.8.0-beta3
- 增加分批处理方法BatchFor，支持同步异步

### 1.8.0-beta2
- 增加RandomArraySelector，随机返回函数元素

### 1.8.0-beta1
- 增加随机生成器
- 调整方法存储位置

### 1.7.0
- 增加double格式化字符串方法和decimal格式化字符串方法

### 1.6.0
- 优化异常类

### 1.5.0
- 更新GlobalConfig文件为CoreGlobalConfig，避免与其他项目冲突
- 完善ICurrentUser
- 增加实体类转字典ToDictionary
- 增加StringHelper

### 1.4.0
- 增加本地日志是否清理旧日志可配置，可以修改GlobalConfig配置来实现
- 更新包名AzrngCommon.Core更新为Azrng.Core

### 1.3.0
- 更新List转DataTable操作
- 优化本地日志写法，支持异步处理

### 1.2.1
- 增加排序url参数值 UriHelper.SortUrlParameters，调整url扩展方法和帮助类
- 日志帮助类增加修改日志输出的级别
- 日志帮助类增加超过7天自动删除
- 增加IJsonSerializer接口
- 增加sql安全检测帮助类
- 在HtmlHelper下增加HtmlToText
- DateTimeHelper下增加GetTimeDifferenceText
- 增加CsvHelper
- 调整部分扩展方法目录

### 1.2.0
- 移除包Newtonsoft.Json的依赖
- 更新获取网络时间的接口API
- 禁用部分涉及到序列化的方法

### 1.1.6
- 修复GetColumnValueByName方法bug
- 支持.Net9

### 1.1.5
- 修改GetColumnValueByName方法bug

### 1.1.4
- 增加IEnumerable的WithIndex扩展
- ApplicationHelper类增加RuntimeInfo方法
- 优化IO操作方法

### 1.1.3
- 字典增加GetOrDefault扩展方法
- 调整Guard中message和参数的顺序

### 1.1.2
- 更新时间扩展，增加 DateTime?.ToDateString、DateTime?.ToStandardString
- 将一部分方法抽离到DateTimeHelper中，防止扩展污染
- 修改where筛选QueryableWhereIF为QueryableWhereIf

### 1.1.1
- 增加默认无参数的ToStandardString
- 增加将时间转年月日：ToDateString
- 增加Guard帮助类，使用方法比如Guard.Against.Null("输入值", "参数名")
- 增加Console的输出ReadLineWithPrompt、ReadKeyWithPrompt
- 增加ApplicationHelper应用程序帮助类
- 将LongHelper改名为NumberHelper
- 增加获取无时区的当前时间

### 1.1.0
- 优化ResultModel类，增加Failure方法

### 1.0.10
- 增加AsyncHelper执行同步方法
- 移除获取时间戳弃用的方法
- 增加CollectionHelper支持分页批次处理数据
- 增加FileHelper.FormatFileSize

### 1.0.9
- 获取特殊文件夹之桌面文件路径
- 增加检查文件是否被其他进程锁定方法
- 增加获取NTP网络远程时间
- 增加系统操作：获取本机ip、ipv4地址、ipv6地址
- 增加MimeExtensions扩展

### 1.0.8
- 升级支持.net8

### 1.0.7
- 更新ICurrentUser默认为string类型UserId

### 1.0.6
- 增加程序集扩展方法和程序集帮助类以及获取所有程序集的方法
- 增加枚举帮助类EnumHelper，迁移枚举扩展中部分方法到枚举帮助类中
- 枚举扩展类中方法GetDescriptionString改名GetDescription

### 1.0.5
- 增加Random的NextDouble扩展方法
- 支持框架net6.0;net7.0

### 1.0.4
- 迁移base64的扩展到StringExtension，并且改名为ToBase64Encode、FromBase64Decode
- 增加时间段和时间点相互转换代码
- 增加ExceptionExtension、DecimalExtension、DoublelExtension

### 1.0.3
- 增加任务运行时间限制方法，TaskHelper.RunTimeLimitAsync
- 增加字符串输出扩展

### 1.0.2
- 增加本地日志文件操作类

### 1.0.1
- 引用Newtonsoft.Json包，增加json操作扩展
- 将扩展方法的命名空间改为Common.Extension

### 1.0.0
- 将common中的部分类移动到该类库中

## 📄 许可证

MIT License
