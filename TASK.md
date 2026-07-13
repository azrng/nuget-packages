# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

> 当前无活跃任务。

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T102 | Azrng.AspNetCore.Authorization.Default 库审查修复（P0 匿名路径 Contains 子串匹配越权缺陷改用 StartsWithSegments 路径段前缀匹配、P1 认证检查改用 result.Succeeded、P2 PermissionRequirement 收紧为只读 + null 防御、P1 修正 README 双重注册与 ARCHITECTURE 示例编译错误，版本 1.1.0→1.2.0，测试 5→20 项） | DONE | 2026-07-13 |
| T101 | JwtBearer 包审查后修复（升级认证依赖到各 TFM 最新 patch、移除配置热更新语义、默认回到 ASP.NET Core JwtBearer 标准事件行为，新增 UseTokenExpiredHeader/UseUnauthorizedJsonResponse/UseAzrngJwtBearerDefaultResponses 辅助扩展，测试 26→28 项） | DONE | 2026-07-13 |
| T100 | Azrng.AspNetCore.Authentication.JwtBearer 审查修复与测试补充（默认密钥清空+IValidateOptions 收口、CreateToken/ValidateToken/GetJwtInfo 异常处理修正、共享 TVP、Singleton+IOptionsMonitor、OnChallenge 开关，测试 7→27 项，commit 662c35e） | DONE | 2026-07-13 |
| T099 | T098 缺陷修复补充回归测试（M1/M2/M3/L8/H1/M7 共 25 项，commit b85d8f6） | DONE | 2026-07-11 |
| T098 | Azrng.JSqlParser 库代码审查修复（审查发现 22 项缺陷全部修复：H1 Merge 三连失 SourceTable/WHEN AND/InsertValues、H2 区域性数值解析静默数据损坏、H3 多语句静默丢弃、H4 TablesNamesFinder 表名提取遗漏、M1/M2 ExpressionVisitorAdapter context 丢失与 Function 子树不全、M3 JsonFunction null 非法 SQL、M7/M8 Validation 校验补全、L1 CTE 括号、L2 Offset ROWS、L3 ASTNode 漏末 token、L4 SyntaxErrorListener 死代码、L6 KsqlJoinWindow 越界、L7 死变量；M5 误报、M6 边缘跳过） | DONE | 2026-07-11 |

文件结束。
