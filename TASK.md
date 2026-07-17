# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T107 | Azrng.JSqlParser 支持 @ 命名参数 | 修复 @name 被解析成普通 JDBC 参数导致变量名丢失的问题，补测试并产出新版包 | 阶段 2 | Codex | BLOCKED | P1 | 2026-07-16 |
| T108 | Azrng.JSqlParser 加 Descendants/Walk 扩展方法消除 visitor 副作用返回 + ARCHITECTURE 同步对照（beta6） | 新增 ExpressionExtension/StatementExtension（Descendants/Walk/ExtractTableNames），内部 walker 引擎不改 AST 与 visitor 接口；ARCHITECTURE 加对照表；补 42 项测试，全量 1399 项通过；版本 beta5→beta6 | 阶段 1 | ZCode | DONE | P1 | 2026-07-16 |
| T109 | Azrng.JSqlParser 下沉结构化提取（GetTableReferences/GetSelectColumns/GetWhereConditions + 通用 DTO，beta7） | 下沉 LocalSqlParser 纯 AST 提取为库扩展方法 + 中性 DTO（TableReference/SelectColumn/WhereCondition），业务约定与 DTO 装配留业务方；WHERE 递归穿透 Parenthesis（比原逻辑更完整）；补 32 项测试，全量 1431 项通过；版本 beta6→beta7 | 阶段 1 | ZCode | DONE | P1 | 2026-07-17 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T106 | Azrng.AspNetCore.Authorization.Default 审查问题修复（Requirement 保留 string[] 公开 API 并补防御性拷贝、路径规范化、setter/constructor null 校验，处理器使用内部规范化集合，README/ARCHITECTURE/IPermissionVerifyService 示例改用 StartsWithSegments，测试 20→26 项） | DONE | 2026-07-13 |
| T105 | JwtBearer 版本号修正（包版本与 README 最新版本说明从 1.5.1 改回 1.5.0，并验证 nupkg 产物版本为 1.5.0） | DONE | 2026-07-13 |
| T104 | JwtBearer 当前未提交改动审查与提交（修正 UseJwtBearerDefaultResponses 命名一致性、README/ARCHITECTURE/测试同步，示例保留 AddMyAuthentication 的 Basic+Bearer 分发并显式启用预置响应，纯 JWT helper 设置 Bearer 默认方案） | DONE | 2026-07-13 |
| T103 | JwtBearer README 补充事件扩展配置说明（补充 UseAzrngJwtBearerDefaultResponses、UseTokenExpiredHeader、UseUnauthorizedJsonResponse 的拆分说明、单独启用、组合启用、参数自定义和与 SignalR/自定义事件组合方式） | DONE | 2026-07-13 |
| T102 | Azrng.AspNetCore.Authorization.Default 库审查修复（P0 匿名路径 Contains 子串匹配越权缺陷改用 StartsWithSegments 路径段前缀匹配、P1 认证检查改用 result.Succeeded、P2 PermissionRequirement 收紧为只读 + null 防御、P1 修正 README 双重注册与 ARCHITECTURE 示例编译错误，版本 1.1.0→1.2.0，测试 5→20 项） | DONE | 2026-07-13 |

文件结束。
