# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T122 | Cache.Redis/MemoryCache 审查修复 + 原子计数器 | 修复评审问题：Memory GetOrCreate 降级路径回源（FailThrowException=false 时改为回源而非返回 default）、Redis 重连退避防惊群（ConnectCoreAsync 拿锁后复查退避窗口）、订阅关闭竞态（TryBeginCloseIfEmpty + 按 handler 精确退订 + 按键值对移除避免误删并发新建订阅）、DI 生命周期统一为 Singleton、日志降噪、正则匹配超时；ICacheProvider 1.0→1.1 新增 IncrementAsync/DecrementAsync（Redis 服务端 INCRBY / 内存 Interlocked，单节点安全）；RedisTransport.cs 拆分为 9 个单类型文件；新增计数器+降级+防惊群测试。Core 1.0→1.1，Redis/MemoryCache 3.0→3.1，Newtonsoft.Json 13.0.1→13.0.3。MemoryCache 21 项全过；Redis 44 项全过（15 单测 + 29 集成，含真实 Redis 172.16.127.100:25089 计数器并发原子性 8×500=4000 精确）。含缓存策略变更（降级语义）与 DI 生命周期变更两项破坏性改动，待用户确认 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-27 |
| T121 | DistributeLock 补充测试 | 补齐本次修复核心路径的真实服务测试：Redis/PG 锁丢失通知端到端（外部删 key/行）、PG 续期仅未过期可续的新条件、Redis/PG 过期接管后旧实例误释放防护、注册参数校验。真实服务回归：PG 14 项、Redis 16 项全过 | 阶段 1 | ZCode | DONE | P2 | 2026-07-26 |
| T120 | Azrng.DistributeLock 审查问题修复 | 修复评审必须处理项：续期失败可感知（LockLostToken）、移除终结器、InMemory 实现过期语义、升级 Npgsql 修复 CVE-2024-32655、PG 改用数据库时钟；顺带修复日志泄漏连接串、配置校验、默认过期时间不一致、短过期续期空窗。Core/InMemory/Redis 0.3.0→0.4.0，PG 0.2.0→0.3.0；InMemory 测试 13 项全过；用户提供真实服务后 PG 9 项、Redis 12 项全过（测试 Startup 改为优先读 AZRNG_LOCK_PG_CONN / AZRNG_LOCK_REDIS_CONN 环境变量，避免真实连接信息入库），测试后锁表清理为空 | 阶段 1 | ZCode | DONE | P1 | 2026-07-26 |
| T119 | Azrng.NmcWeather 审查问题修复 | 收紧 LooksLikeCityCode 启发式（基于 2413 样本精确为 5 位 base62），新增 NmcWeatherOptionsValidator 启动期配置校验，补全测试缺失分支 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-23 |
| T118 | Azrng.DataAccess 单次 SQL 超时 | 为分页 SQL 查询提供单次 commandTimeout 参数并透传至 Dapper 命令 | 阶段 1 | Codex | DONE | P1 | 2026-07-22 |
| T107 | Azrng.JSqlParser 支持 @ 命名参数 | 修复 @name 被解析成普通 JDBC 参数导致变量名丢失的问题，补测试并产出新版包 | 阶段 2 | Codex | BLOCKED | P1 | 2026-07-16 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样：A 类运算符符号错配(Contains/ContainedBy/JsonOperator) + C 类 ExpressionVisitorAdapter 空 Visit(5) + D 类 ExpressionDescendantsWalker 空 Visit(6) + E 类结构性沉默丢弃(ORDER BY NULLS/WITH RECURSIVE/JOIN 多 ON) + 中危项(IsNullExpression PG 简写/InExpression Global/LikeExpression useBinary+REGEXP_LIKE 下划线/FullTextSearch 类型/Pivot 多聚合/SELECT INTO/DISTINCT ON/LIMIT BY/ORDER BY WITH ROLLUP/MySQL INDEX FOR 等)。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。9 批 commit + 1 批文档，测试 1465→1567（+102）。MIGRATION.md 第 13 节同步对照表 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-18 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T117 | Azrng.Core 新增 UnauthorizedException（401）并接入异常中间件 | Azrng.Core 新增 UnauthorizedException（ErrorCode 401，未认证语义）版本 1.19.0→1.20.0；Azrng.AspNetCore.Core 将 Azrng.Core 引用由 PackageReference 1.8.4 改为本地 ProjectReference（对齐 Common.EFCore 主流），异常中间件新增 UnauthorizedException→401 映射；补两库单测与文档。Azrng.Core.Test net8.0 全过 2424，Azrng.AspNetCore.Core.Test 4 TFM × 29 项通过 | DONE | 2026-07-19 |
| T115 | Azrng.JSqlParser 上游 issue 修复（AST 正确性 + MySQL DDL 索引族） | 探针逐条核实 ⑨ AST 5 条 + ① DDL 索引族 5 条共 10 条：仅 #1570（CONSTRAINT 双名吞约束名）、#538（UNIQUE 后直接跟索引名 grammar 不支持）真实复现需修；其余 8 条移植版不复现/已支持/不适用。⑨ AST 5 条转绿 + 结构断言（零源码改动）；DDL 真修 2 条（grammar tableConstraint 新分支 + Constraint.IndexName 字段 + visitor 双名分离）+ 转绿 3 条。3 commit（AST 探针 + DDL 修复 + 文档），测试 1635→1654（+19 active，17 Skip→10 Skip）。MIGRATION.md 第十五节 + issue 分类清单状态列同步。修正清单原"⛔ 复现且未修复"误判 | DONE | 2026-07-19 |
| T116 | Azrng.AspNetCore.Core 审查问题修复（P0+P1） | 修复审查 P0+P1：CommonMvcConfig 改 IOptions 注入使配置生效；移除异常中间件 HasStarted 有害判断；审计中间件 EndTime/Elapsed 统一在响应完成回调内计算消除竞态并补异常保护；ForbiddenException 401→403（破坏性）；移除 IsAotCompatible 声明；异常中间件 JsonSerializerOptions 改静态复用。版本 1.3.2→1.4.0，5 TFM 编译 0 警告，测试 4 TFM × 28 项通过 | DONE | 2026-07-19 |
| T114 | Azrng.JSqlParser 非 PG 上游 issue 修复（8 条） | 修复探针复现的 8 条上游 issue：#1169 GROUP BY DESC（仅解析兼容，方向不结构化为字段）、#854 INTO @var、#1314 INSERT SET 主体（AS 行别名不修）、#1589 PRIMARY KEY NONCLUSTERED、#161 OPTION hint、#2298 CAST CHARACTER SET、#2427+#2006 _utf8mb4 introducer、#911 @table 表变量。按方言分 3 批 commit + 1 批文档 + 1 批回退精简。每条改 lexer+grammar+visitor+模型，新增 26 探针 + 25 round-trip 测试，全量 1599→1635 通过。剔除 #2421（BigQuery 小众）、#2428（MySQL 已死语法）。MIGRATION.md 第 14 节 + issue 分类清单状态列同步 | DONE | 2026-07-19 |
| T113 | Azrng.JSqlParser PostgreSQL 专项 12 条上游 issue 验证与修复 | 探针核查 issue 分类清单 ④ 全部 12 条：4 条移植版已支持，8 条复现已修复（#187 FTS @@/@@@ + gist 索引、#1416 EXPLAIN 选项、#1511 WITH ORDINALITY、#1728 interval hour to minute、#2326 XMLTable、#2411 ROWS FROM、#2412 (expr).*、#2432 LIKE ANY/ALL）。补 23 项探针 + 10 项 round-trip 测试，全量 1566→1599 通过 | DONE | 2026-07-18 |

文件结束。
