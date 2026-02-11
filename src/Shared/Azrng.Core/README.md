# Azrng.Core

一个公共的帮助类库，包含一些常用通用的方法。

> 下面的文档还没补充完整，待完善

## 扩展类

### StringExtension

```c#
//判断字符串是否是数值类型
bool IsIntFormat(this string str)
// 判断字符串是否是decimal类型
bool IsDecimalFormat(this string str)
//判断字符串是否是日期类型
bool IsDateFormat(this string str)
//判断字符串是否包含中文
bool HasChinese(this string str)
//判断字符串不是  null、空和空白字符
bool IsNotNullOrWhiteSpace(this string currentString)
//判断字符串 是  null、空和空白字符
bool IsNullOrWhiteSpace(this string currentString)
//判断字符串是  null、空
bool IsNullOrEmpty(this string currentString)
//判断字符串不是  null、空
bool IsNotNullOrEmpty(this string currentString)
//字符串分割成字符串数组
string[] ToStrArray(this string str, string separator = ",")
//根据条件拼接字符串
StringBuilder AppendIF(this StringBuilder builder, bool condition, string str)
//获取特定位置的字符串
string GetByIndex(this string str, int index)
//忽略大小写的字符串比较
bool EqualsNoCase(this string aimStr, string comparaStr)
```

### ObjectExtensions

模型类转url参数格式(1.0.5)

```c#
var userInfo = new UserInfoDto { Id = 1, Name = "Test", };
// 参数不转小写
var result = userInfo.ToUrlParameter();// Id=1&Name=Test
// 参数转小写
var result = userInfo.ToUrlParameter(true);// id=1&name=Test
```
## 帮助类

### LocalLogHelper

支持异步写入日志、支持历史日志清理(默认7天)、支持设置是否输出指定等级的日志

```C#
// 异步记录日志
await LocalLogHelper.WriteMyLogsAsync("Info", "这是一条测试日志");

// 或继续使用现有的同步方法
LocalLogHelper.LogInformation("这是一条信息日志");

// 如果想设置只输出指定等级以上的日志，可以进行如下设置
GlobalConfig.MinimumLevel = LogLevel.Warning;
// 再次之后输出输出的日志就只会输出警告及以上的日志
```

### RetryHelper

提供便捷的异步操作重试功能，支持多种重试策略和条件配置。

> 默认配置：只有当遇到抛出 `RetryMarkException` 异常时才会进行重试。

#### 基本用法

```csharp
// 简单重试（无延时）
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3);

// 带固定延时重试
var result = await RetryHelper.ExecuteAsync(
    () => GetDataAsync(),
    3,
    TimeSpan.FromSeconds(2)
);

// 带参数的方法
var result = await RetryHelper.ExecuteAsync(() => ProcessAsync(id, name), 3);
```

#### 动态延时策略

```csharp
// 指数退避重试（每次重试延时翻倍）
var result = await RetryHelper.ExecuteAsync(
    () => GetDataAsync(),
    maxRetryCount: 3,
    delayStrategy: i => TimeSpan.FromSeconds(Math.Pow(2, i))
);

// 自定义延时策略
var result = await RetryHelper.ExecuteAsync(
    () => GetDataAsync(),
    maxRetryCount: 3,
    delayStrategy: i => TimeSpan.FromSeconds(i + 1) // 第1次1s，第2次2s，第3次3s
);
```

#### 条件重试

```csharp
// 当捕获到特定异常时重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenCatch<HttpRequestException>();

// 当捕获到特定异常且满足条件时重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenCatch<HttpRequestException>(ex => ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable);

// 带异常处理的异步重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenCatchAsync<TimeoutException>(async ex =>
    {
        await LogErrorAsync(ex);
        return true; // 返回 true 表示继续重试
    });
```

#### 基于结果的重试

```csharp
// 当结果满足条件时重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenResult(data => data == null || data.Count == 0);

// 异步检查结果
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3)
    .WhenResultAsync(async data =>
    {
        await ValidateAsync(data);
        return data.IsValid == false;
    });
```

#### 触发重试

在需要重试的地方抛出 `RetryMarkException`：

```csharp
public async Task<string> GetDataAsync()
{
    try
    {
        var response = await httpClient.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            throw new RetryMarkException("服务暂时不可用");
        }
        return await response.Content.ReadAsStringAsync();
    }
    catch (HttpRequestException ex)
    {
        throw new RetryMarkException("请求失败", ex);
    }
}

// 使用重试
var result = await RetryHelper.ExecuteAsync(() => GetDataAsync(), 3);
```

## 公共返回类

封装了公共的返回类

```c#
IResultModel
IResultModel<T>
ResultModel:IsSuccess、Code、Message、Errors
ResultModel<T>：IsSuccess、Code、Message、Data
```

> 属性描述
>
> IsSuccess：是否成功
> Code:状态码
> Data:返回的数据
> Errors：模型校验的错误信息

返回正确的方法

```c#
[HttpGet]
public IResultModel<IEnumerable<WeatherForecast>> Get()
{
    var result = Enumerable.Range(1, 3).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    })
    .ToArray();
    return ResultModel<IEnumerable<WeatherForecast>>.Success(result);
}
```

返回的示例效果

```json
{
  "data": [
    {
      "date": "2022-05-20T22:13:35.2501522+08:00",
      "temperatureC": 52,
      "temperatureF": 125,
      "summary": "Freezing"
    },
    {
      "date": "2022-05-21T22:13:35.2505438+08:00",
      "temperatureC": 4,
      "temperatureF": 39,
      "summary": "Balmy"
    },
    {
      "date": "2022-05-24T22:13:35.250546+08:00",
      "temperatureC": 7,
      "temperatureF": 44,
      "summary": "Hot"
    }
  ],
  "isSuccess": true,
  "code": "200",
  "message": "success",
  "errors": []
}
```

返回错误的效果

```c#
[HttpGet]
public IResultModel<IEnumerable<WeatherForecast>> Get()
{
    return ResultModel<IEnumerable<WeatherForecast>>.Error("参数为空", "400");
}
```

返回结果

```json
{
  "data": null,
  "isSuccess": false,
  "code": "400",
  "message": "参数为空",
  "errors": []
}
```


## 版本更新记录

* 1.15.5
  * 新增DictionaryExtension、DoublelExtension、ByteExtensions、CollectionExtensions、DataTableExtensions的完整单元测试
  * 完善DateTimeExtension、DecimalExtension单元测试
  * 修复ByteExtensions.GetFileCode方法IndexOutOfRangeException问题
  * 修复QueryableExtensions.GreaterWhere方法比较类型错误
  * 修复GetWeekOfMonth计算问题，增加WeekCalculationMode枚举支持两种周次计算方式
  * 修复TimeSpan.ToDateTime方法ArgumentOutOfRangeException问题
  * 修复MimeExtensions相关问题
  * 新增GetWeekOfMonth方法重载，支持指定周计算模式
* 1.15.4
  * 修复IResultModel的null引用
* 1.15.3
  * 更新null引用问题
* 1.15.2
  * 修复Nullable问题
* 1.15.1
  * 启用Nullable
* 1.15.0
  * 枚举增加GetEnglishDescription
* 1.14.0
  * 增加StringBuilderExtension扩展
* 1.13.0
  * 增加方法重试操作到RetryHelper
* 1.12.0
  * StringHelper增加字符串批量替换方法
  * 增加针对url的AddQueryString
  * 修改Url的GetUrl为ExtractUrl
* 1.11.0
  * 增加关于时间、字典、表达式树扩展方法
  * GetCustomerObj变更为GetCustomerAttribute
  * 变更CalculateDaysDifference改为扩展方法DateDiff
* 1.10.0
  * 增加ObjectHelper.ConvertToTargetType类型转换方法
  * 增加GetColumnValueByName的IDictionary<string, string>重载方法
* 1.9.0
  * 未知
* 1.8.5
  * 增加方法GetStringPropertiesWithValues
  * 增加ToDetailedTimeString
  * 增加ChineseTextHelper扩展类
* 1.8.4
  * 增加ToGuid
  * 优化sql检测类SqlHelper
  * 支持.Net10
* 1.8.3
  * IEnumerable增加ToPage方法
* 1.8.2
  * 更新IJsonSerializer中ToJson泛型约束
  * 增加计算相差的天数
* 1.8.1
  * 将数字字符串转换为中文数字
  * 增加CollectionHelper.RemoveRandomItem从集合中随机删除指定数量元素
  * 增加sql注入检测方法SqlHelper.HasSqlInjectionRisk
  * 更新帮助类CollectionHelper
* 1.8.0
  * 增加一些EFCore相关模型类
* 1.8.0-beta4
  * 更新BaseRequestDto，修改默认用户标识为字符串
  * 新增BaseCustomerRequestDto
* 1.8.0-beta3
  * 增加分批处理方法BatchFor，支持同步异步
* 1.8.0-beta2
  * 增加RandomArraySelector，随机返回函数元素
* 1.8.0-beta1
  * 增加随机生成器
  * 调整方法存储位置
* 1.7.0
  * 增加double格式化字符串方法和decimal格式化字符串方法
* 1.6.0
  * 优化异常类
* 1.5.0
  * 更新GlobalConfig文件为CoreGlobalConfig，避免与其他项目冲突
  * 完善ICurrentUser
  * 增加实体类转字典ToDictionary
  * 增加StringHelper
* 1.4.0
  * 增加本地日志是否清理旧日志可配置，可以修改GlobalConfig配置来实现
  * 更新包名AzrngCommon.Core更新为Azrng.Core
* 1.3.0
    * 更新List转DataTable操作
    * 优化本地日志写法，支持异步处理
* 1.2.1
    * 增加排序url参数值 UriHelper.SortUrlParameters，调整url扩展方法和帮助类
    * 日志帮助类增加修改日志输出的级别
    * 日志帮助类增加超过7天自动删除
    * 增加IJsonSerializer接口
    * 增加sql安全检测帮助类
    * 在HtmlHelper下增加HtmlToText
    * DateTimeHelper下增加GetTimeDifferenceText
    * 增加CsvHelper
    * 调整部分扩展方法目录
* 1.2.0
    * 移除包Newtonsoft.Json的依赖
    * 更新获取网络时间的接口API
    * 禁用部分涉及到序列化的方法
* 1.1.6
    * 修复GetColumnValueByName方法bug
    * 支持.Net9
* 1.1.5
    * 修改GetColumnValueByName方法bug
* 1.1.4
    * 增加IEnumerable的WithIndex扩展
    * ApplicationHelper类增加RuntimeInfo方法
    * 优化IO操作方法
* 1.1.3
    * 字典增加GetOrDefault扩展方法
    * 调整Guard中message和参数的顺序
* 1.1.2
    * 更新时间扩展，增加 DateTime?.ToDateString、DateTime?.ToStandardString
    * 将一部分方法抽离到DateTimeHelper中，防止扩展污染
    * 修改where筛选QueryableWhereIF为QueryableWhereIf
* 1.1.1
    * 增加默认无参数的ToStandardString
    * 增加将时间转年月日：ToDateString
    * 增加Guard帮助类，使用方法比如Guard.Against.Null("输入值", "参数名");
    * 增加Console的输出ReadLineWithPrompt、ReadKeyWithPrompt
    * 增加ApplicationHelper应用程序帮助类
    * 将LongHelper改名为NumberHelper
    * 增加获取无时区的当前时间
* 1.1.0
    * 优化ResultModel类，增加Failure方法
* 1.0.10
    * 增加AsyncHelper执行同步方法
    * 移除获取时间戳弃用的方法
    * 增加CollectionHelper支持分页批次处理数据
    * 增加FileHelper.FormatFileSize
* 1.0.9
    * 获取特殊文件夹之桌面文件路径
    * 增加检查文件是否被其他进程锁定方法
    * 增加获取NTP网络远程时间
    * 增加系统操作：获取本机ip、ipv4地址、ipv6地址
    * 增加MimeExtensions扩展
* 1.0.8
    * 升级支持.net8
* 1.0.7
    * 更新ICurrentUser默认为string类型UserId
* 1.0.6
    * 增加程序集扩展方法和程序集帮助类以及获取所有程序集的方法
    * 增加枚举帮助类EnumHelper，迁移枚举扩展中部分方法到枚举帮助类中
    * 枚举扩展类中方法GetDescriptionString改名GetDescription
* 1.0.5
    * 增加Random的NextDouble扩展方法
    * 支持框架net6.0;net7.0
* 1.0.4
    * 迁移base64的扩展到StringExtension，并且改名为ToBase64Encode、FromBase64Decode
    * 增加时间段和时间点相互转换代码
    * 增加ExceptionExtension、DecimalExtension、DoublelExtension
* 1.0.3
    * 增加任务运行时间限制方法，TaskHelper.RunTimeLimitAsync

    * 增加字符串输出扩展
* 1.0.2
    * 增加本地日志文件操作类
* 1.0.1
    * 引用Newtonsoft.Json包，增加json操作扩展
    * 将扩展方法的命名空间改为Common.Extension
* 1.0.0
    * 将common中的部分类移动到该类库中