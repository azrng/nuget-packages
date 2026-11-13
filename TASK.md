# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前步骤 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T173 | Common.EFCore 系列审查问题修复 | 修复仓储全局缓存跨Scope复用、CreateCommand连接/命令泄漏与事务内失败、Postgres DeleteAsync返回值、MySQL ServerVersion重复探测、GetTypes异常防护、回滚吞异常等；行为变更+新增注册按minor升版本号（EFCore/MySQL/PostgresSql 1.8.0，InMemory/SQLServer/SQLite 1.7.0）并同步README；构建+Postgres测试回归后待评审 | 待用户评审 | ZCode | REVIEW | P1 | 2026-11-13 |
| T174 | EventBus 系列包审查问题修复 | 修复 RabbitMQ 消息丢失（DLX/nack）、channel 泄漏、断线重连、Polly 覆盖、Options 校验、Core 键冲突检测；Core/RabbitMQ 升 1.2.0 并同步文档；真实 RabbitMQ 链路验证+全部单测通过 | 已完成 | ZCode | DONE | P1 | 2026-11-13 |
| T175 | InMemory 包独立审查问题修复 | 直投原事件对象（删序列化往返）、新增 HandlerFailurePolicy 失败策略、补 Nullable、删死配置；测试 6→12；README/ARCHITECTURE 同步；升 1.2.0 | 已完成 | ZCode | DONE | P1 | 2026-11-13 |
| T172 | 审查批次4：文档债与剩余 P2 修复 | CHANGELOG 迁移+ARCHITECTURE 校对+README 修正；HtmlHelper/DbTypeMap/Tree/ToEntity/ToUrlParameter/IList/MarkEqual/Console收敛/异常中间件/结果包装/OCE/ShowAllServices；全部回归通过后提交 | 已完成 | ZCode | DONE | P1 | 2026-11-12 |
| T119 | Azrng.NmcWeather 审查问题修复 | 收紧 LooksLikeCityCode 启发式（基于 2413 样本精确为 5 位 base62），新增 NmcWeatherOptionsValidator 启动期配置校验，补全测试缺失分支 | 等待评审 | ZCode | REVIEW | P1 | 2026-07-23 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。测试 1465→1567（+102） | 等待评审 | ZCode | REVIEW | P1 | 2026-07-18 |
| T129 | InMemory EventBus 同步分发 | 对齐 MediatR：发布方 await 等待处理器完成，移除后台队列；保留多处理器并发与异常隔离，并补直接分发测试 | 等待评审 | Codex | REVIEW | P1 | 2026-08-13 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T167 | Azrng.Core/AspNetCore.Core 审查问题批次1修复 | DONE | 2026-11-11 |
| T168 | Azrng.Core 审查问题批次2修复（P1） | DONE | 2026-11-11 |
| T169 | Azrng.Core 审查问题批次3修复（P2 语义与清理） | DONE | 2026-11-12 |
| T170 | Azrng.Core 审查后续单点修复（D1-D6） | DONE | 2026-11-12 |
| T171 | AspNetCore.Core 测试 net6.0 编译修复 | DONE | 2026-11-12 |

文件结束。
