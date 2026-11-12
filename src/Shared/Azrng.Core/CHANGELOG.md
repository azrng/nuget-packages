# Azrng.Core 更新记录

### 1.20.1
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

### 1.20.0
- 新增 `UnauthorizedException`（ErrorCode 401），表示未认证（未登录或凭证无效），与 `ForbiddenException`（403，已认证但无权限）区分

### 1.18.0

- `ResultModel` / `ResultModel<T>` 新增成功消息重载，支持业务消息保真
- 新增 `ResultModelFactory`，统一创建成功、失败和异常转换结果
- 新增 `ResultModelExtensions`，提供 `DataOrEmpty`、`DataOrDefault` 数据安全读取扩展
- 新增 `ResultModelExceptionExtensions`，支持 `Exception.ToFailureResult<T>()`

### 1.17.0

- 优化写入本地日志的操作

### 1.16.0
- 完善 DatabaseType 枚举的 XML 文档注释，为所有数据库类型添加详细的中文说明

### 1.15.8
- 修复DateDiff扩展方法引用本身问题

### 1.15.7
- 修复 HandleAsDefaultWhenException 方法实现错误，异常时正确返回默认值而不是重新抛出异常
- 修复 GuardClauseNullExtensions 中 NullOrEmpty 方法调用 Null 方法时参数顺序错误
- 修复 TaskHelperTest 中空引用异常问题
- 优化 AssemblyHelperTest 测试断言逻辑

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

