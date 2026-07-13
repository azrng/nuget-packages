# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

> 当前无活跃任务。

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T103 | JwtBearer README 补充事件扩展配置说明（补充 UseAzrngJwtBearerDefaultResponses、UseTokenExpiredHeader、UseUnauthorizedJsonResponse 的拆分说明、单独启用、组合启用、参数自定义和与 SignalR/自定义事件组合方式） | DONE | 2026-07-13 |
| T102 | Azrng.AspNetCore.Authorization.Default 库审查修复（P0 匿名路径 Contains 子串匹配越权缺陷改用 StartsWithSegments 路径段前缀匹配、P1 认证检查改用 result.Succeeded、P2 PermissionRequirement 收紧为只读 + null 防御、P1 修正 README 双重注册与 ARCHITECTURE 示例编译错误，版本 1.1.0→1.2.0，测试 5→20 项） | DONE | 2026-07-13 |
| T101 | JwtBearer 包审查后修复（升级认证依赖到各 TFM 最新 patch、移除配置热更新语义、默认回到 ASP.NET Core JwtBearer 标准事件行为，新增 UseTokenExpiredHeader/UseUnauthorizedJsonResponse/UseAzrngJwtBearerDefaultResponses 辅助扩展，测试 26→28 项） | DONE | 2026-07-13 |
| T100 | Azrng.AspNetCore.Authentication.JwtBearer 审查修复与测试补充（默认密钥清空+IValidateOptions 收口、CreateToken/ValidateToken/GetJwtInfo 异常处理修正、共享 TVP、Singleton+IOptionsMonitor、OnChallenge 开关，测试 7→27 项，commit 662c35e） | DONE | 2026-07-13 |
| T099 | T098 缺陷修复补充回归测试（M1/M2/M3/L8/H1/M7 共 25 项，commit b85d8f6） | DONE | 2026-07-11 |

文件结束。
