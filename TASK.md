# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 当前活跃

| ID | 任务名称 | 目标 | 阶段 | 状态 | 更新时间 |
|----|----------|------|------|------|----------|
| T058 | Azrng.AspNetCore.DbEnvConfig 代码审查问题修复 | 修复审查报告中的必须项(Dispose 模式缺陷/后台线程不可及时停止/InitTable 静默吞异常/SQL 注入文档警告)与建议项(DB→Db 命名统一、ParamVerify→Normalize、DefaultScriptService→PostgreSqlScriptService、移除冗余 System.Data.Common/System.Text.Json 引用),补充 Dispose 停止后台线程/DbException 回滚/OnReload 仅变化时触发 单测,同步示例项目/README/ARCHITECTURE,版本 1.2.0→2.0.0(破坏性)。Release 全 TFM(net6/7/8/9/10)0 警告 0 错误,测试 net8/9 各 9/9 通过,示例项目编译通过。DbConfigurationSource 单例缓存为 ARCHITECTURE 记录的设计决策,保留 | 阶段 1(实现+验证完成,待确认) | REVIEW | 2026-06-26 |
| T057 | docs-generator 类库元数据展示与 tags 筛选 | 新增 csproj.ts 解析 .csproj 的 Title/PackageTags/Description/Version/TargetFrameworks；index.ts 按项目→bin XML 关系注入到 Assembly（多 TFM 共享）；前端首页类库卡片展示标题（无 Title 用包名兜底）/描述/版本/TFM/tag，顶部 tag 标签云（前24）AND 筛选，类库详情页头展示元数据；无元数据字段不渲染占位。修复首页 onclick 字符串转义 SyntaxError；修复首页左侧导航只显示第一个类库（renderNavTreeHome）；首页隐藏右侧目录栏（home-mode）；Logo 可点击回首页；修复选「全部」未重置下拉；多 TFM 去重（搜索索引 -34%、下拉 188→54）；tag 筛选进 URL（#/tags/a,b）+ 卡片/详情 tag 可点击筛选；修复卡片 <a> 嵌 <a> 导致结构错乱（卡片改 <div>，标题/计数/tag 各为独立 <a>）；P2 搜索覆盖类库（标题/名称/tag/描述）并分组（类库/类型/成员）；P3 导航树大库默认折叠（>15 类型不自动展开首命名空间）+ 主题三态（dark/light/auto，auto 跟随系统并实时监听）。tsc 通过、csproj 解析等价验证全 PASS、抽取 SPA 脚本 new Function 解析无语法错误、tag URL 往返编码验证通过、搜索/折叠/主题功能均验证。已知：去重取首条 TFM 会丢 1 个仅 net10.0 存在的编译器内部类型 CallerArgumentExpressionAttribute，业务无影响。待人工预览确认视觉 | 阶段 1（实现+验证完成，待预览确认） | REVIEW | 2026-06-25 |
| T056 | Azrng.ConsoleApp.DependencyInjection 集成测试 | 模仿 Common.HttpClients.Next.Test 范式（Xunit.DependencyInjection 9.4.0 + Logging 8.1.0 + Startup + Integration/ 子目录 + [Trait Category=Integration]）补集成测试；csproj 对齐 xunit 2.9.3/visualstudio 3.0.1/coverlet 6.0.3；Startup 真实装配 ConsoleAppServer（命令行配置+Configure<TOption>）；新增 6 个集成测试覆盖配置加载/选项绑定/日志解析/RunAsync 端到端/异常重抛/注册覆盖；net8/9 各 22 全绿（单元16+集成6），--filter Category!=Integration 可跳过集成 | 阶段 1（实现+验证完成） | REVIEW | 2026-06-24 |
| T055 | Azrng.ConsoleApp.DependencyInjection 增强 #4/#6/#7 | ExtensionsLogger 日志分发由 switch 改静态字典查表统一走 WriteMyLogs；新增 ConfigureLogging(Action<ILoggingBuilder>?) 委托重载（默认行为不变，传委托完全自定义）；新增 Configure<TOption> 便捷封装；SYSLIB1104（微软泛型 Configure<T> 的 AOT 已知限制 runtime#89273）经 csproj NoWarn 抑制；版本 1.3.4→1.3.5；Release net8/9/10 0 警告 0 错误，测试 net8/9 各 16 全绿，ConsoleAppDI 示例编译通过。取消 #5 CancellationToken（避免破坏性变更） | 阶段 1（实现+验证完成） | REVIEW | 2026-06-24 |
| T054 | Azrng.ConsoleApp.DependencyInjection 问题修复 | 修复配置加载异常信息丢失（保留完整异常链）、Build 重载泛型命名不一致（T→TStart）、README .NET 徽章版本不符（6.0→8.0）、ARCHITECTURE 示例与代码不同步（基路径/重载/Scope 解析/签名）；Release 构建 net8/9/10 通过 0 警告 0 错误 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T053 | Azrng.AspNetCore.Core 扩展前基础加固 | 扩展前补齐多目标框架测试、CORS 参数校验、审计日志默认序列化兜底、关键行为回归测试和 README/ARCHITECTURE 说明；测试项目 4 个目标框架通过，Release 构建已生成 1.3.1 包 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T052 | Azrng.DataAccess README 与版本升级 | 针对提交 c85e1879051ce6fb66d27aeb83e463e3b14c7301 将 DynamicSqlBuilder 合并到 Azrng.DataAccess，更新包 README 发布说明并将 Azrng.DataAccess 版本从 1.0.0-beta4 升级到 1.0.0-beta5；Release 构建已生成 beta5 包 | 阶段 2（文档与版本发布完成） | DONE | 2026-06-24 |
| T051 | docs-generator 架构改造与完善 | 单页 HTML 大规模卡顿，改造为纯前端 SPA + hash 路由 + data.json 分离；删除 index-new.ts/generate.js 废弃入口；修泛型参数解析误切、成员丢失、类型分类误判；加可搜索类库下拉、导航树高亮定位与展开记忆、右侧目录滚动修复、npm run preview；XML 收集严格绑定 PackPackages.slnx（58 项目/239 XML/2290 类型/14505 成员）。已编译生成+HTTP 验证 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T050 | Azrng.DataAccess 测试覆盖审查与完善 | 基于覆盖率审查 Azrng.DataAccess.Test，补充 DynamicSqlBuilder/DataAccess 可离线验证的单元测试，避免引入本地数据库依赖 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T049 | DynamicSqlBuilder 测试合并到 DataAccess.Test | 将 DynamicSqlBuilder 离线单元测试并入 Azrng.DataAccess.Test，移除独立测试项目；依赖本地 PostgreSQL 的旧集成测试不纳入默认回归 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
| T048 | DynamicSqlBuilder 合并到 Azrng.DataAccess | 将 Azrng.Database.DynamicSqlBuilder 作为 Azrng.DataAccess 内置动态 SQL 构建模块发布，README 标注当前仅 PostgreSQL 方言已验证，并保留后续多数据库方言扩展设计 | 阶段 1（实现+验证完成） | DONE | 2026-06-24 |
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




