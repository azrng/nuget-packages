# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T119 | Azrng.NmcWeather 审查问题修复 | 收紧 LooksLikeCityCode 启发式（基于 2413 样本精确为 5 位 base62），新增 NmcWeatherOptionsValidator 启动期配置校验，补全测试缺失分支 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-23 |
| T107 | Azrng.JSqlParser 支持 @ 命名参数 | 修复 @name 被解析成普通 JDBC 参数导致变量名丢失的问题，补测试并产出新版包 | 阶段 2 | Codex | BLOCKED | P1 | 2026-07-16 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。测试 1465→1567（+102） | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-18 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T126 | Azrng.JSqlParser 高价值 issue 清仓 + **1.0.0-beta11** | DONE | 2026-07-28 |
| T122 | Cache.Redis/MemoryCache 审查修复 + 原子计数器 | DONE | 2026-07-27 |
| T121 | DistributeLock 补充测试 | DONE | 2026-07-26 |
| T120 | Azrng.DistributeLock 审查问题修复 | DONE | 2026-07-26 |
| T118 | Azrng.DataAccess 单次 SQL 超时 | DONE | 2026-07-22 |

文件结束。
