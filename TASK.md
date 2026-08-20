# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T119 | Azrng.NmcWeather 审查问题修复 | 收紧 LooksLikeCityCode 启发式（基于 2413 样本精确为 5 位 base62），新增 NmcWeatherOptionsValidator 启动期配置校验，补全测试缺失分支 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-23 |
| T107 | Azrng.JSqlParser 支持 @ 命名参数及 ANY 数组参数 | 保留 @name 参数名，并支持 PostgreSQL `ANY/ALL/SOME` 接收命名数组参数、数组表达式或子查询，补测试并产出新版包 | 阶段 2 | Codex | DONE | P1 | 2026-08-20 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。测试 1465→1567（+102） | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-18 |
| T129 | InMemory EventBus 同步分发 | 对齐 MediatR：发布方 await 等待处理器完成，移除后台队列；保留多处理器并发与异常隔离，并补直接分发测试 | 阶段 1 | Codex | REVIEW | P1 | 2026-08-13 |
| T130 | GitHub 工作流接入 NuGet Trusted Publishing | nuget-publish.yml 改用 NuGet/login@v1 以 GitHub OIDC 换取 1 小时短时 API 密钥推送包，移除对长期 secrets.NUGET_API_KEY 的依赖 | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |
| T131 | Azrng.AspNetCore.Core CORS API 精简 | 删除与框架原生 API 完全重复的 AddCorsPolicy 扩展方法，保留 AddAnyCors 与 AddCorsByOrigins，同步测试、README、ARCHITECTURE 与版本记录（1.5.0，破坏性变更） | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T131 | Azrng.AspNetCore.Core CORS API 精简（移除 AddCorsPolicy，1.5.0） | DONE | 2026-08-20 |
| T128 | Common.HttpClients 4.0 增量（HttpHeaders 多值请求头 + JsonNamingPolicy 命名策略） | DONE | 2026-08-10 |
| T127 | Common.HttpClients 4.0 API 精简与统一（统一签名/废弃FailThrowException/精简方法表） | DONE | 2026-08-10 |
| T126 | Azrng.JSqlParser 高价值 issue 清仓 + **1.0.0-beta11** | DONE | 2026-07-28 |
| T122 | Cache.Redis/MemoryCache 审查修复 + 原子计数器 | DONE | 2026-07-27 |

文件结束。
