---
rule_id: backend-agents
version: 3.1.0
last_updated: 2026-11-03
dependencies: [agents-root]
---

# 后端规则

## 适用范围

- 作用域：NuGet 包实现（`src/Shared/` 类库）、示例宿主（`src/Services/`）、数据访问、缓存、异常处理与测试（`test/`）
- 本仓库主体是多包单仓库：多数改动对象是无宿主的类库包，其“契约”即公开 API（公开接口、DTO、Options、扩展方法）；包改动以公开 API 设计 + 测试项目验证为主。`src/Services/` 示例项目是包的真实消费方，结构保持最小（`Program.cs` / `Startup.cs` + 少量 Controller），不套用额外分层模板
- 触发场景：涉及包公开 API、数据访问、缓存、权限、异常处理或测试时阅读

### 常见任务入口
- 改包公开 API：先看契约变更同步链（根 `AGENTS.md`「NuGet 包变更记录」）与 `infra-AGENTS.md`「发布纪律」版本号口径
- 改示例宿主接口响应或状态码：先看 Controller 规则、统一响应格式与异常处理规范
- 改查询、分页、缓存、迁移：先看数据访问规则与验证要求
- 补回归测试：先看 `提交前最小回归` 与测试规则

---

## 技术栈

### 后端
- .NET 10 + ASP.NET Core（示例宿主项目）；类库包按 csproj 目标框架多目标构建

### 核心库
- `Azrng.Core`：基础类库（实体基类、扩展方法、工具类、结果包装、异常体系）
- `Azrng.Core.Json`：JSON 序列化（基于 `System.Text.Json`）
- `Azrng.AspNetCore.Core`：API 基础设施（统一响应、模型校验、异常中间件）
- `Azrng.Swashbuckle`：Swagger 扩展（简化配置、自动 XML 注释、JWT 认证支持）
- `Azrng.SqlMigration`：SQL 脚本迁移执行
- `Common.Cache.Redis`：Redis 缓存封装（基于 StackExchange.Redis）
- `Common.Cache.MemoryCache`：内存缓存封装（基于 `IMemoryCacheProvider`）
- `Common.HttpClients`：HTTP 客户端封装（基于 `IHttpClientFactory`，支持依赖注入和弹性策略）
- 外部 HTTP 调用默认走 `Common.HttpClients`（`IHttpHelper`，当前核对 4.2.0，按项目锁定的 4.x 版本为准），不为单次调用重复封装 HttpClient；只有需要包未暴露的底层控制时才使用原生 `HttpClient` / `IHttpClientFactory`；无外部调用时不引入该包。
- 存量例外：既有代码库从未引入 `Common.HttpClients` 且既有外部调用均为原生 `HttpClient`（如静态共享实例 + 显式超时）时，新增同类外部调用沿用仓库现状（原生 `HttpClient`，须有共享实例与超时设置），不强制引包迁移；新仓库或已引入该包的仓库按默认口径执行。两类口径并存时（同仓库既有 `IHttpHelper` 又有裸 `HttpClient`），以仓库主导口径为准并保持一致，不在同一服务内混用。
- 引入 `Common.HttpClients` 时宿主须在组合根显式调用其 `AddHttpClientService` 注册运行时依赖：包不随工程引用自动注册，调用方编译通过不代表 `IHttpHelper` 运行时已可注入。
- `IHttpHelper` 默认开启自动重试（`MaxRetryAttempts=3`，重试 5xx / 408 / 超时）：token 换发、委托授权、扣款等**非幂等端点**必须在调用点传 `HttpSendOptions { EnableRetry = false }` 单独关闭（4.2.0+），或为该服务单独注册命名客户端并配 `MaxRetryAttempts = 0`，不得让自动重试放大非幂等请求。

### 数据库访问库
- `Common.EFCore`：EF Core 基础封装（必须安装，所有 Provider 都依赖它）
- 根据项目使用的数据库，按需选择以下其中一个 Provider 包：
  - `Common.EFCore.PostgresSql`：PostgreSQL（默认推荐；注意实际 NuGet 包 id 拼写为 `PostgresSql`）
  - `Common.EFCore.MySQL`：MySQL / MariaDB
  - `Common.EFCore.SQLServer`：SQL Server
  - `Common.EFCore.SQLite`：SQLite
  - `Common.EFCore.InMemory`：内存数据库（仅用于测试）

### 数据库
- 默认使用 PostgreSQL
- 可根据项目需求切换为其他数据库（MySQL、SQL Server、SQLite 等）
- ORM：Entity Framework Core，通过 `Common.EFCore` 系列包访问
- 测试环境可使用 `Common.EFCore.InMemory` 内存数据库

如果仓库已经有真实实现，以现有代码为准，不要强行重构或替换技术栈。
技术债务与重构判断遵循 `collaboration-AGENTS.md` 的「纠错、回退与重构」。

---

## 主动建议规则
- 发现包公开 API 与实现职责混杂、包内依赖越界、示例宿主把业务逻辑写进 Controller、权限校验缺失、事务边界不清或异常处理不一致时，应主动提醒
- 发现接口契约、DTO、存储结构、缓存策略或状态流转可能影响历史数据、兼容性或权限安全时，必须先说明风险，不得直接扩大修改
- 不确定业务规则、权限规则、数据含义或外部接口行为时，应按根 `AGENTS.md` 的查证优先级处理，禁止按通用经验补写规则

---

## 后端规则

### Controller 规则
- Controller 只负责 HTTP 处理，不包含业务逻辑。
- 使用 `[ApiController]` 特性，自动处理 400 响应。
- 使用 `[Route("api/[controller]")]` 统一路由前缀。
- 通过构造函数注入服务；禁止直接操作 DbContext 或 `IBaseRepository<T>`。
- Controller action 直接返回 `Task<T>` 并一行转发服务实现；不写 `Ok()`，不手动构造 `ResultModel`，统一包装由全局过滤器与异常中间件完成（见 `统一响应格式`）。

### 代码组织规范
- 一个文件只放一个主类型：Entity、DTO、Request、Response、枚举等公开契约类，默认一个类一个文件，文件名与类名一致。
- DTO 目录组织：包内请求 / 响应 / 查询参数 DTO 按所属包既有目录归组；示例宿主中按业务域或用途独立文件，Request 与 Response 分别独立文件，不合并到 `Dtos.cs` 或 `{模块}Dto.cs`。
- 禁止在 Controller、实体、DTO 等 C# 类中使用主构造函数（Primary Constructor），统一使用显式构造函数。
- 触发拆分的信号：职责混杂、同文件出现多个主类型、参数或字段持续堆叠、方法跨多个不相关业务时，应主动拆分。
- 允许例外：`private` / `internal` 且只服务当前文件的小辅助类型、与主类型强绑定的局部 mapping extension、测试文件中只服务当前测试类的小型 fixture。
- 反模式：`Dtos.cs` 长期堆放十几个 Request / Response；把 Controller 专用 request 内联写在 Controller 文件里。

### 核心逻辑可读性（KISS）
- 以下场景优先显式直写，不要本能压成一条链：聚合、分组排序后取 Top N、多层字典构造、有失败边界的控制流、需要稳定输出顺序的逻辑。
- 优先形式：`foreach`、`if / else`、中间变量、明确命名的临时集合、显式排序 / 裁剪 / 显式异常。
- 谨慎使用：多层嵌套 LINQ、一次性 `GroupBy + Select + ToDictionary + ToList` 链、只为“看起来干净”而拆的 helper。
- helper / 拆分只在降低认知负担时才成立：让职责边界更清楚、真正复用、让调用方少理解细节；若拆完读者要在多个位置来回跳，或用 helper 隐藏本该直说的业务判断，就退回直白写法。
- 共性的“先读再写”“范围最小扰动”见根 `AGENTS.md`「全局工作规则」与 `collaboration-AGENTS.md`「纠错、回退与重构」

### 数据模型规则
- 实体类继承自 Azrng 基类（如 `IdentityOperatorStatusEntity`，自带雪花 Id、审计与软删除字段），放在所属包或示例项目既有数据模型目录。
- DTO 按用途区分（查询参数、创建请求、更新请求、响应结果），文件组织见 `代码组织规范`。
- 跨模块复用枚举独立成文件，放所属包既有目录；仅服务单个 DTO 的可随主类型。
- 禁止在 DTO 中暴露数据库内部字段。

### 数据访问规则
- 数据访问默认直接注入 `Common.EFCore` 提供的 `IBaseRepository<T>`，不自建仓储层；只读查询优先使用 `EntitiesNoTacking`。
- 使用 LINQ 查询，避免原生 SQL。
- 多步原子写操作优先使用 `IUnitOfWork` 事务，不直接操作 `IDbContextTransaction`。
- 分页列表查询使用 `GetPageRequest` + `ToPageListAsync` + `GetQueryPageResult<T>`，不手写 `CountAsync()` + `Skip().Take()`。
- 软删除统一使用实体基类的 `Deleted` 字段；包不自动做全局过滤，默认在查询中显式过滤 `!x.Deleted`，或在实体配置类中统一配置查询过滤器。
- DbContext 与 EF Core 实体配置类（`{实体名}Etc.cs`）放所属包或示例项目既有数据访问目录，DbContext 通过 `ApplyConfigurationsFromAssembly` 扫描应用配置。

### 统一响应格式
- 所有 API 响应统一为 `ResultModel<T>` 结构，由框架三件套自动完成，业务代码不手动包装：
  - `services.AddMvcResultPackFilter("/swagger")`：成功路径自动把 action 返回值包成 `ResultModel<T>`
  - `services.AddMvcModelVerifyFilter()`：模型校验失败统一转为失败 `ResultModel`
  - `app.UseGlobalException()`：业务异常与未处理异常统一映射为失败 `ResultModel`，必须放在中间件管道最外层
- 服务实现成功直接返回业务对象（`Task<T>`），失败抛出 Azrng 业务异常；新代码禁止手写 `ResultModel<T>.Success / Error` 手动包装（该写法仅存量代码迁移期可容忍）。
- HTTP 状态码默认按异常类型返回真实状态码（400 / 401 / 404 / 500）；`ResultModel` 的 `code` 字段承载业务码。

### 依赖注入规范
- 服务类必须实现以下接口之一：
  - `ITransientDependency`：每次请求创建新实例
  - `IScopedDependency`：每个请求作用域内单例
  - `ISingletonDependency`：应用程序生命周期单例
- Controller 通过构造函数注入服务。
- 禁止使用静态服务定位器 pattern。
- 禁止手动在 `Program.cs` 中逐一注册服务，应使用 `RegisterBusinessServices` 批量扫描注册。

### 异常处理规范
- 业务异常继承 Azrng 异常体系：
  - `LogicBusinessException`：业务逻辑异常
  - `ParameterException`：参数校验异常
  - `NotFoundException`：资源不存在
  - `ForbiddenException`：禁止访问
  - `InternalServerException`：服务器内部错误
- 异常由中间件统一捕获并转换为 `ResultModel` 响应。
- 业务失败一律通过抛出上述异常表达；禁止在业务层手工构造失败 `ResultModel` 或直接设置错误状态码。

### Redis 缓存使用规范
- 使用 `Common.Cache.Redis` 封装的 `IRedisProvider` 接口。
- 通过 `services.AddRedisCacheStore()` 注册 Redis 服务，配置项使用 `RedisCacheOptions`。
- 缓存操作必须设置合理的过期时间。
- 热点数据使用本地缓存 + Redis 二级缓存。
- 发布订阅场景使用 `PublishAsync` / `SubscribeAsync`。
- 禁止在循环中频繁调用 Redis。

### 内存缓存使用规范
- 使用 `Common.Cache.MemoryCache` 封装的 `IMemoryCacheProvider` 接口。
- 通过 `services.AddMemoryCacheStore()` 注册内存缓存服务，配置项使用 `MemoryCacheOptions`。
- `MemoryCacheOptions.DefaultExpiry` 默认仅 5 秒，业务缓存必须显式传入过期时间，不依赖默认值。
- 适用于单实例部署、不需要分布式共享的缓存场景。
- 与 Redis 搭配使用时作为一级缓存，Redis 作为二级缓存。

### 数据库迁移规则
- 使用 `Azrng.SqlMigration` 进行数据库脚本迁移，不使用 EF Core Code-First Migrations。
- 迁移脚本放在项目根目录 `MigrationSql/` 下，文件名按 `版本号.sql` 或 `版本号.txt` 命名（如 `1.0.0.sql`、`1.1.0.txt`），不含描述文字——去掉版本前缀与扩展名后的剩余部分会被整体解析为版本号；按版本号顺序执行。
- 通过 `AddSqlMigrationService(...).AddAutoMigration()` 在应用启动时自动执行未应用的脚本；已执行版本由包自动记录在版本日志表，不重复执行，无需手工维护版本表。
- 已应用的脚本不可修改；结构调整必须新增更高版本号的脚本。
- 涉及数据的高风险结构变更，建议同步准备回滚脚本并在文档中说明恢复方式。

### Swagger 规范
- 使用 `Azrng.Swashbuckle` 包，禁止直接引用 `Swashbuckle.AspNetCore`。
- 服务注册：`services.AddDefaultSwaggerGen(title: "项目名称", showJwtToken: true)`。
- 中间件启用：`app.UseDefaultSwagger(onlyDevelopmentEnabled: true)`。
- 项目必须启用 XML 文档生成：`<GenerateDocumentationFile>true</GenerateDocumentationFile>`。
- Controller 和公开方法必须添加 XML 注释，Swagger 会自动展示；XML 注释内容统一使用中文。
- 非 Controller 的公开接口方法 XML 注释按 `代码注释规范` 处理（不依赖 Swagger 自动展示，但需保证可读与可维护）。
- 接口需通过 Swagger UI 独立验证，确保请求 / 响应结构正确；仅适用于带宿主的 Web API 项目，纯类库包改由测试项目 / 示例项目真实调用验证。

### 字段命名约定
- C# 属性：PascalCase
- JSON 输出：camelCase（通过 `System.Text.Json` 配置自动转换）
- 数据库字段：snake_case（通过 EF Core 命名约定配置）

### 代码注释规范
- 必须补 XML 注释的位置：
  - Controller action：用 `<summary>` 说明接口用途、关键参数、返回结果（Swagger 会消费这些说明）。
  - 公开接口方法（包公开接口、Adapter 接口等）：说明该能力做什么，不写实现细节。
  - 不来自接口的实现类自有方法（接口未约束的核心方法）：同上，补 `<summary>` 说明该能力做什么。
- 接口已注释时，实现方法不重复注释，也不加 `/// <inheritdoc />`。
- 默认不写 `<remarks>`：实现类、实现方法上的 `<remarks>` 多为实现细节，按"不写实现细节"原则省略。
  - 例外：Controller action 需要向调用方暴露事件格式、鉴权要求、分页约定等对外契约信息时，可保留精简后的 `<remarks>`，但不写内部实现说明。
- 涉及时间字段的 DTO / 接口注释必须写明转换责任方：入参是本地墙钟还是 UTC、由哪一层完成转换（如“本地墙钟（服务层统一转换）”），而不是只标存储单位。
- 优先补行内注释的位置：涉及权限、状态流转、事务、缓存、幂等或降级的复杂业务分支，说明“为什么这样做”——包括 fail fast 的原因、为什么跳过某类脏数据、为什么保留某一层结构。
- 不要补的注释：普通属性的 get / set、简单 `if` / `return`、"返回结果"这类复述代码的低价值注释。
- 注释统一使用中文，不在注释里泄露密钥、token、连接串或真实生产地址。

### 时间字段转换链路规则
- 修改时间字段的 DTO、接口、映射、持久化或调用链路前，先检索该字段的全链路转换次数，确认同一方向的转换位置有且只有一处再动手。

### 日志安全基线
- 密码、令牌、密钥、认证头、完整请求体等敏感数据禁止写入日志，包括异常文本、脱敏不彻底的差异内容与导出文件。
- 需要记录可归因操作（审计类）时，在服务端可信的业务操作入口记录事件，操作者从已认证身份解析，不信任浏览器或客户端上报的操作者与结果。
- 日志内容超出预期敏感面时先修日志入口，不用放宽脱敏或调低级别的方式绕过。

### 服务端授权边界
- 权限校验必须落在服务端入口（应用服务、API 端点、资源读写入口）；前端隐藏按钮、路由保护、客户端缓存的角色信息只改善交互，不构成安全边界。
- 不信任客户端提交的角色、权限、用户 ID、资源范围等身份与授权信息；服务端从已认证身份和当前授权数据重新解析并校验。
- 末位管理员保护、内置角色不可删等关键业务约束必须在所有写入入口统一执行，不能只靠前端禁用控件。

### 反模式与修根因清单
- 这是根 `AGENTS.md`「修 Bug 必须定位根因」在 .NET 中的具体禁止项，禁止用下列方式掩盖问题：
  - 空 `catch` 或 `catch (Exception)` 吞异常后静默继续
  - 用 `!`（null-forgiving）压制可空告警而非真正处理 null
  - 在异步上下文用 `.Result` / `.Wait()` 同步阻塞
  - `#pragma warning disable` 关闭编译 / 分析器告警掩盖问题
  - 删除或注释测试让构建通过
  - 用一层抽象重写掩盖原始复杂度，而不是简化它
- 出现这些倾向时立刻回退一步：复杂 LINQ 比业务逻辑本身更难读；helper 数量增加但理解成本没下降；核心逻辑缺注释只能靠猜。

### 可维护性自检（提交前）
提交前对核心逻辑至少问一遍：
- 维护者能否一遍看懂这段核心逻辑？
- 是否必须跳转多个 helper 才知道发生了什么？
- 关键方法和关键逻辑有没有注释？
- 这是不是当前需求的最简单可行实现？
- 我是不是为了“写得像高手”而牺牲了可读性？
- 犹豫写“抽象版”还是“直白版”时，默认选直白版。

---

## 测试规则

### 提交前最小回归
- 默认执行：项目现有的静态检查、类型检查或编译校验
- 业务逻辑改动：至少补一项业务规则、权限判断或状态流转验证
- 接口 / 路由 / 控制器改动：至少补一项输入输出、状态码或异常分支验证
- 数据访问、缓存、迁移改动：至少补一项查询结果、缓存行为、事务或迁移同步验证
- 若仓库暂时缺少自动化测试基础，可使用接口调用、脚本或命令行验证替代，但必须能证明核心逻辑真实生效

### 总体要求
- 影响行为的改动应优先补充或更新测试。
- 若本次改动未补测试，必须在最终说明中写明原因和风险。
- 测试应覆盖真实业务行为。

### 后端测试
- 测试框架：xUnit + Moq
- Controller 测试：验证状态码、响应结构、参数校验
- 核心服务测试：验证核心业务规则与公开 API 行为，使用 mock 隔离外部依赖
- 数据访问测试：In-Memory 数据库仅覆盖基础行为；涉及 PostgreSQL 时间语义、JSON 映射或 SQL 方言差异的查询逻辑优先连真实数据库验证（现有 `Common.EFCore.PostgreSql.Test` 等通过环境变量连接真实库），In-Memory 无法暴露此类差异
- 集成测试：使用 `WebApplicationFactory` 测试完整请求管道

### .NET 并行隔离
- 多会话并行时构建 / 测试输出落到任务独立目录：`dotnet build <测试项目.csproj> -o artifacts/test/<任务名>` 后 `dotnet vstest <测试程序集.dll>`；运行验收：`dotnet build <入口项目.csproj> -p:OutputPath=artifacts/run/<任务名>/`（`-p:OutputPath` 相对项目文件目录解析，需落仓库根时改用绝对路径）；`dotnet publish -o` 仍写默认 `bin`，不能绕过输出锁。隔离只改输出目录（`-o` / `OutputPath`），保持项目默认 `obj` 中间目录，不自定义 `BaseIntermediateOutputPath`（会引发重复生成代码）。
- 启动隔离实例时确认 `ContentRoot` 与迁移脚本等按物理路径读取的资源指向预期；直接 `dotnet <dll>` 时 `ContentRoot` 默认为当前工作目录。
- NuGet 打包验证同样用 `-o artifacts/packages/<任务名>` 独立输出，不与其他会话共用输出目录。

### 外部依赖与数据
- 测试中不要真实调用第三方服务，统一使用 mock。
- 测试数据应尽量最小化、可读、可重复执行。
- 不要让测试依赖本地人工状态。

### 无法执行测试时
- 必须说明未执行的测试类型。
- 必须说明未执行原因。
- 必须说明潜在影响范围和风险。

---
