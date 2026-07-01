# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 当前活跃

| ID | 任务名称 | 目标 | 阶段 | 状态 | 更新时间 |
|----|----------|------|------|------|----------|
| T062 | Common.HttpClients.Next 目录结构整理（方案 A：纯物理分文件夹，命名空间不变） | 根目录 16 个 .cs 文件平铺显得凌乱，按职责拆分到 Abstractions/Client/Extensions/Logging/Internal 子文件夹，仅 git mv 改文件物理位置，全部保留 namespace Common.HttpClients 不变，确保 public API 与已发布 NuGet 包（3.0.1）零破坏。Utils/ 目录已存在保持不动。Release 编译 net6/7/8/9/10 全部 0 警告 0 错误并正常生成 3.0.1 nupkg；测试 177/177 通过（与整理前基线一致） | 阶段 1（整理+验证完成） | DONE | 2026-07-01 |
| T061 | Common.HttpClients.Next DI 启动异常修复 + 非命名模式集成测试 + 版本升级 3.0.0→3.0.1 | HttpTestApi 启动报 LoggingHandler 激活异常（Unable to resolve service for type 'System.String'）。根因：Next 项目 LoggingHandler 构造函数首参为 string clientName（命名客户端设计），但 ServiceCollectionExtensions.cs 多了一行 TryAddTransient&lt;LoggingHandler&gt;() 死注册，DI 无法解析 string 参数，Development 环境 ValidateOnBuild 触发激活校验失败。修复：删除该死注册（实际创建走 AddHttpMessageHandler 的 ActivatorUtilities 工厂，不受影响），并补注释说明。同时 Program.cs 同时注册非命名(default)与命名(demo)两种客户端演示双模式；新增 HomeController 演示 IHttpHelper(非命名注入) 与 IHttpHelperFactory(命名注入)。补充 ApifoxEchoDefaultClientIntegrationTests 13 个非命名模式集成测试（GET/POST/PUT/DELETE/表单/自定义头/SendAsync/流式/下载/404/RawBody），回归验证非命名注入路径正常。版本号从 3.0.0 升级到 3.0.1（bug 修复，patch 级别），同步更新 csproj Version、README 安装命令版本与版本更新记录。dotnet build 0 错误，Release 构建生成 Common.HttpClients.3.0.1.nupkg，Build 启动日志证明 DI 异常已修复（端口绑定 SocketException 10013 为 Windows 保留端口环境问题，与代码无关），全量测试 177/177 通过（164 原有 + 13 新增） | 阶段 1（实现+验证完成） | DONE | 2026-06-30 |
| T060 | 修复 DevLogDashboard.Test 构建错误（CommonStudy.slnx） | InMemoryLogStoreTests.cs 中 GetTraceSummariesAsync 调用缺失必填 startTime/endTime 参数（CS7036，4 处无参 + 2 处只传单个命名参数，共 6 个调用点），以及 NotThrowAsync 误用 .Invoking（CS1061，2 处，应改 .Awaiting）。生产签名 GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime, CancellationToken) 自始未变，属测试侧调用过时。修复后 CommonStudy.slnx Release 构建 0 错误 | 阶段 1（实现+验证完成） | DONE | 2026-06-30 |
| T059 | Common.EFCore.MySQL/SQLServer/SQLite 版本升级与 bug 修复 | 修复三个 EFCore 包中 ServiceCollectionExtensions.cs 的 DatabaseType 错误（PostgresSql 改为对应类型），并将版本号从 1.6.0 升级到 1.6.1 | 阶段 1（实现+验证完成） | DONE | 2026-06-27 |
| T058 | docs-generator 类库 README 展示 + 元数据归属修复 + 搜索/导航调整 | README 展示：docs-generator 运行时读取 PackPackages.slnx 内每个类库同目录 README.md（大小写不敏感），用 marked 在 Node 构建阶段渲染成 HTML 注入 data.json，前端类库详情页（#/lib/{lib}）标题下方展示完整 README；新增 src/markdown.ts（renderMarkdown，剔除 script 兜底），parser.ts Assembly 携带 readme，generator.ts 序列化 + README 区块 + CSS。元数据归属修复（既有 bug）：传递依赖 XML 副本导致 fileName 冲突，先到先得抢占 metadata/readme（如 Azrng.Core 的 Title/Version/Tags/README 全丢失），改为按程序集名（<assembly><name>）匹配 metadata/readme，且收集阶段按程序集名去重丢弃传递依赖副本（447→339 XML，data.json 15.3MB→6.4MB）。导航/搜索调整：首页左侧导航树 lib 显示 packageId 而非 title；搜索按页面分流——首页/全部模式只搜类库 packageId，类库详情页只搜当前类库内的类型/成员；路由切换自动清空搜索框。tsc 编译通过、npm start 生成成功（53/58 项目含 README、Azrng.Core 元数据恢复）、index.html 逻辑验证通过。已知限制：相对路径图片部署后裂图 | 阶段 1（实现+验证完成） | REVIEW | 2026-06-26 |
| T057 | docs-generator 类库元数据展示与 tags 筛选 | 新增 csproj.ts 解析 .csproj 的 Title/PackageTags/Description/Version/TargetFrameworks；index.ts 按项目→bin XML 关系注入到 Assembly（多 TFM 共享）；前端首页类库卡片展示标题（无 Title 用包名兜底）/描述/版本/TFM/tag，顶部 tag 标签云（前24）AND 筛选，类库详情页头展示元数据；无元数据字段不渲染占位。修复首页 onclick 字符串转义 SyntaxError；修复首页左侧导航只显示第一个类库（renderNavTreeHome）；首页隐藏右侧目录栏（home-mode）；Logo 可点击回首页；修复选「全部」未重置下拉；多 TFM 去重（搜索索引 -34%、下拉 188→54）；tag 筛选进 URL（#/tags/a,b）+ 卡片/详情 tag 可点击筛选；修复卡片 <a> 嵌 <a> 导致结构错乱（卡片改 <div>，标题/计数/tag 各为独立 <a>）；P2 搜索覆盖类库（标题/名称/tag/描述）并分组（类库/类型/成员）；P3 导航树大库默认折叠（>15 类型不自动展开首命名空间）+ 主题三态（dark/light/auto，auto 跟随系统并实时监听）。tsc 通过、csproj 解析等价验证全 PASS、抽取 SPA 脚本 new Function 解析无语法错误、tag URL 往返编码验证通过、搜索/折叠/主题功能均验证。已知：去重取首条 TFM 会丢 1 个仅 net10.0 存在的编译器内部类型 CallerArgumentExpressionAttribute，业务无影响。待人工预览确认视觉 | 阶段 1（实现+验证完成，待预览确认） | REVIEW | 2026-06-25 |
| T056 | Azrng.ConsoleApp.DependencyInjection 集成测试 | 模仿 Common.HttpClients.Next.Test 范式（Xunit.DependencyInjection 9.4.0 + Logging 8.1.0 + Startup + Integration/ 子目录 + [Trait Category=Integration]）补集成测试；csproj 对齐 xunit 2.9.3/visualstudio 3.0.1/coverlet 6.0.3；Startup 真实装配 ConsoleAppServer（命令行配置+Configure<TOption>）；新增 6 个集成测试覆盖配置加载/选项绑定/日志解析/RunAsync 端到端/异常重抛/注册覆盖；net8/9 各 22 全绿（单元16+集成6），--filter Category!=Integration 可跳过集成 | 阶段 1（实现+验证完成） | REVIEW | 2026-06-24 |
| T055 | Azrng.ConsoleApp.DependencyInjection 增强 #4/#6/#7 | ExtensionsLogger 日志分发由 switch 改静态字典查表统一走 WriteMyLogs；新增 ConfigureLogging(Action<ILoggingBuilder>?) 委托重载（默认行为不变，传委托完全自定义）；新增 Configure<TOption> 便捷封装；SYSLIB1104（微软泛型 Configure<T> 的 AOT 已知限制 runtime#89273）经 csproj NoWarn 抑制；版本 1.3.4→1.3.5；Release net8/9/10 0 警告 0 错误，测试 net8/9 各 16 全绿，ConsoleAppDI 示例编译通过。取消 #5 CancellationToken（避免破坏性变更） | 阶段 1（实现+验证完成） | REVIEW | 2026-06-24 |
| T054 | Azrng.ConsoleApp.DependencyInjection 问题修复 | 修复配置加载异常信息丢失（保留完整异常链）、Build 重载泛型命名不一致（T→TStart）、README .NET 徽章版本不符（6.0→8.0）、ARCHITECTURE 示例与代码不同步（基路径/重载/Scope 解析/签名）；Release 构建 net8/9/10 通过 0 警告 0 错误 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T053 | Azrng.AspNetCore.Core 扩展前基础加固 | 扩展前补齐多目标框架测试、CORS 参数校验、审计日志默认序列化兜底、关键行为回归测试和 README/ARCHITECTURE 说明；测试项目 4 个目标框架通过，Release 构建已生成 1.3.1 包 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T052 | Azrng.DataAccess README 与版本升级 | 针对提交 c85e1879051ce6fb66d27aeb83e463e3b14c7301 将 DynamicSqlBuilder 合并到 Azrng.DataAccess，更新包 README 发布说明并将 Azrng.DataAccess 版本从 1.0.0-beta4 升级到 1.0.0-beta5；Release 构建已生成 beta5 包 | 阶段 2（文档与版本发布完成） | DONE | 2026-06-24 |
| T051 | docs-generator 架构改造与完善 | 单页 HTML 大规模卡顿，改造为纯前端 SPA + hash 路由 + data.json 分离；删除 index-new.ts/generate.js 废弃入口；修泛型参数解析误切、成员丢失、类型分类误判；加可搜索类库下拉、导航树高亮定位与展开记忆、右侧目录滚动修复、npm run preview；XML 收集严格绑定 PackPackages.slnx（58 项目/239 XML/2290 类型/14505 成员）。已编译生成+HTTP 验证 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T041 | Arrow timestamp(ns) struct wire schema + 转换 | 服务端把 timestamp(ns) 按 struct(sec:int64,nano:int32) 发送，原 TimestampType 前置导致 batch index out of range。改 wire schema 按 struct 声明 + reader 读出后转回 TimestampArray（对齐 PyODPS _convert_struct_timestamps），公共 schema 仍为 TimestampType | 阶段 1（实现+离线单测完成，集群 e2e 待用户凭据验证） | REVIEW | 2026-06-21 |
| T022 | Azrng.Security 合并与改名 | 将 Common.Security 全层统一改名为 Azrng.Security，吸收 Common.SecurityCrypto 独有能力（RSA JSON、RandomString），丢弃其 Provider/Factory 抽象与手写 SM 实现 | 阶段 1（9 任务全部完成，待用户确认） | REVIEW | 2026-06-20 |
| T042 | Common.HttpClients.Next 补充 Apifox Echo 集成测试 | 参考 DevLogDashboard.Test 的 Startup（Xunit.DependencyInjection）写法，对 https://echo.apifox.com 补充 IHttpHelper 集成测试，已覆盖 IHttpHelper 全部 17 个成员（GET/POST/PUT/PATCH/DELETE 回显、Query/JSON/Form/文件上传 multipart/Soap、自定义 Header、GetStreamAsync、SendAsync 枚举与原始、DownloadFileAsync 下载 PNG）+ /delay 超时（Fail 降级 503 / FailThrow 抛 TimeoutRejectedException） | 阶段 1（实现完成，全量 164/164 通过，含 22 个集成测试） | REVIEW | 2026-06-21 |
| T043 | Azrng.DuckDB.Quack benchmark 反馈优化 | DuckDBQuackCompareBenchmarks 切本地项目引用、扩 smoke correctness、补 reader/pool lease/batchSize benchmark；smoke 发现并修复 Quack DATE 解码 +1 天 bug，补 DateOnly roundtrip | 阶段 1（实现+验证完成） | DONE | 2026-06-21 |
| T046 | Azrng.DuckDB.Quack 大结果集 Fetch 终止修复 | 修复 Quack result_uuid 非规范 LEB128 编码被重编码导致 benchmark 100k Fetch 报 Result has been closed 的问题，并补无 Catalog 大结果集回归验证 | 阶段 1（实现+验证完成） | DONE | 2026-06-21 |
| T047 | Azrng.DuckDB.Quack beta2 版本说明补充 | 将当前包版本更新为 1.0.0-beta2，并在 README 补充 beta2 / beta1 版本历史说明 | 阶段 2（文档与发布说明） | DONE | 2026-06-21 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T045 | 流式/分块写 WriteRowsChunkedAsync（BufferedRecordWriter.Batch 自动分块）— P1 部分 | DONE | 2026-06-21 |
| T044 | TableTunnel 表级下载（TableDownloadSession + CreateDownloadSessionAsync，复用 TunnelRecordReader/分片/时区）— P1 | DONE | 2026-06-21 |
| T043 | 多批次分页读 BufferedRecordReader（按 sliceSize 分片 reopen + IAsyncEnumerable 流式）— P0 | DONE | 2026-06-21 |
| T042 | datetime/timestamp 时区开关（UseLocalTimeZone，对齐 PyODPS local_timezone）— P0 | DONE | 2026-06-21 |
| T040 | TunnelRecordReader 补 count 校验（zigzag）+ 回归单测，防止 writer count 编码回归 | DONE | 2026-06-21 |




