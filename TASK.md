# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T149 | Azrng.JSqlParser 同步上游 5.4 第二/三档全部缺口 | 清完基线 e847e94b 剩余能力：PG/MySQL DDL 族（USER/ROLE/DOMAIN/EXTENSION/PUBLICATION/SUBSCRIPTION/TRIGGER/EVENT/DO 块/COMMENT 多目标）、ClickHouse（ARRAY JOIN/WITH FILL/INTERPOLATE/COLUMNS 变换）、BigQuery（UNNEST WITH OFFSET/EXPORT·LOAD DATA/ASSERT）、DuckDB（ANTI JOIN/COPY/ATTACH/PRAGMA/MACRO）、三元 ?:、MATCH_RECOGNIZE、IntervalQualifier/Precision 结构化，，与 T148 同发 1.0.0-rc2 | 阶段 2 | ZCode | DONE | P1 | 2026-09-18 |
| T148 | Azrng.JSqlParser 同步上游 5.4 第一档缺口（7 项） | 对齐基线更新至 tag jsqlparser-5.4（e847e94b）；实现 GROUPS 非保留字、CREATE INDEX INCLUDE、SET IDENTITY_INSERT/SET 布尔开关、MERGE NOT MATCHED BY TARGET/SOURCE、MERGE RETURNING、INSERT OVERRIDING USER VALUE、递归 CTE CYCLE，补 round-trip 测试并发 1.0.0-rc2 | 阶段 2 | ZCode | DONE | P1 | 2026-09-18 |
| T147 | 精简根 AGENTS 规则 | 删除重复章节，将模型行为约束与最终输出要求合并到现有流程规则，保留任务、交付和编码约束 | 阶段 2 | Codex | DONE | P2 | 2026-09-18 |
| T146 | Azrng.JSqlParser 拆分版本变更记录 | 将 README 中的完整版本历史迁移至 CHANGELOG.md，README 保留入口说明并提供 GitHub 变更记录链接，并将拆分规则固化到根 AGENTS.md | 阶段 2 | Codex | DONE | P2 | 2026-09-18 |
| T145 | Common.HttpClients 版本升级至 4.1.0 | 发布前将包版本、安装示例和版本记录统一升级至 4.1.0，纳入响应 JSON 属性大小写配置 | 阶段 2 | Codex | DOING | P1 | 2026-09-16 |
| T144 | Common.HttpClients 响应属性大小写配置 | 通过 `HttpClientOptions.PropertyNameCaseInsensitive` 控制响应 JSON 属性匹配，默认忽略大小写；补齐反序列化回归测试与 README 配置说明 | 阶段 2 | Codex | DONE | P1 | 2026-09-16 |
| T119 | Azrng.NmcWeather 审查问题修复 | 收紧 LooksLikeCityCode 启发式（基于 2413 样本精确为 5 位 base62），新增 NmcWeatherOptionsValidator 启动期配置校验，补全测试缺失分支 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-23 |
| T107 | Azrng.JSqlParser 支持 @ 命名参数及 ANY 数组参数 | 保留 @name 参数名，并支持 PostgreSQL `ANY/ALL/SOME` 接收命名数组参数、数组表达式或子查询，补测试并产出新版包 | 阶段 2 | Codex | DONE | P1 | 2026-08-20 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。测试 1465→1567（+102） | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-18 |
| T129 | InMemory EventBus 同步分发 | 对齐 MediatR：发布方 await 等待处理器完成，移除后台队列；保留多处理器并发与异常隔离，并补直接分发测试 | 阶段 1 | Codex | REVIEW | P1 | 2026-08-13 |
| T130 | GitHub 工作流接入 NuGet Trusted Publishing | nuget-publish.yml 改用 NuGet/login@v1 以 GitHub OIDC 换取 1 小时短时 API 密钥推送包，移除对长期 secrets.NUGET_API_KEY 的依赖 | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |
| T131 | Azrng.AspNetCore.Core CORS API 精简 | 删除与框架原生 API 完全重复的 AddCorsPolicy 扩展方法，保留 AddAnyCors 与 AddCorsByOrigins，同步测试、README、ARCHITECTURE 与版本记录（1.5.0，破坏性变更） | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |
| T132 | Azrng.AspNetCore.Core 移除 PreConfigure 预配置体系 | 删除从未被 Options 管道消费的 PreConfigure/AddObjectAccessor 预配置体系（3 个源文件），清理 APIStudy 试验代码与测试，同步文档（并入 1.5.0 破坏性变更） | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |
| T133 | AspNetCore.Core 反向代理真实客户端 IP 扩展 | 新增 UseForwardedHeaders 中间件封装（可信代理配置 + 防伪造）与 GetClientIp 扩展（X-Forwarded-For/X-Real-IP 兜底解析），补测试与文档；另产出 RequestIdMiddleware 评审结论 | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |
| T134 | RequestIdMiddleware 加固优化 | 入站请求 ID 增加长度与字符白名单校验（防日志注入/伪造关联），无合法入站头时沿用宿主 TraceIdentifier 与诊断体系对齐，删除 IHttpRequestIdentifierFeature 死代码，多值头取第一个值 | 阶段 2 | ZCode | DONE | P2 | 2026-08-20 |
| T135 | IHttpHelper Apifox Echo 使用示例 | 在 Common.HttpClients.Next.Test 新增 Samples/ApifoxEchoSamples，为 IHttpHelper 每个方法提供最小调用示例（目标 https://echo.apifox.com，可运行测试），便于学习用法；原库内示例类已按用户要求迁移出包 | 阶段 1 | ZCode | DONE | P2 | 2026-08-20 |
| T136 | IHttpHelper 新增带请求体的 DeleteAsync 重载 | IHttpHelper/HttpClientHelper 新增 DeleteAsync(url, data) 重载，支持 DELETE 请求携带 JSON body（批量删除、注明原因等场景），补单测、示例测试与 README | 阶段 1 | ZCode | DONE | P2 | 2026-08-20 |
| T137 | DefaultHttpLogRedactor 脱敏性能优化 | 增加"敏感命中预检快速返回"与"非 JSON 首字符快查跳过解析"，JSON 路径重写为 Utf8JsonWriter 单次遍历写出（消除反序列化装箱再序列化），补 5 个单测 | 阶段 1 | ZCode | DONE | P2 | 2026-08-20 |
| T139 | 撤回 T138 版本号收敛（f07511b） | 经查 beta10~beta12 实际均已发布成功（nuget.org 页面显示 beta9 为 SemVer 预发布字符串排序假象），收敛前提不成立；revert 恢复 csproj=beta12 与 README 三段版本历史 | 阶段 2 | ZCode | DONE | P1 | 2026-08-21 |
| T140 | Azrng.JSqlParser 版本升至 1.0.0-rc1 | beta 序号按字符串排序永远低于 beta9，页面最新版本无法翻篇；改用 rc 前缀（代码与 beta12 一致，无功能变更），发布后 rc1 成为排序最新版本 | 阶段 2 | ZCode | DONE | P1 | 2026-08-21 |
| T141 | README 版本历史恢复原样（移除 rc1 段） | 按用户澄清："撤回版本历史"仅指恢复折叠前原始记录，不含新增 rc1 段；移除后版本历史与 f07511b 之前逐字节一致，仅安装示例指向 rc1 | 阶段 2 | ZCode | DONE | P2 | 2026-08-21 |
| T142 | Common.HttpClients 4.0.0 正式收编 + 去共享框架强依赖 | 方案A：版本升至 4.0.0 收编已随 3.1.0 误发的破坏性变更（README 补 3.1.0 误发说明）；移除 FrameworkReference Microsoft.AspNetCore.App 改为 Microsoft.AspNetCore.Http 2.3.13 包引用；push-packer.sh 预检经用户确认取消，发布前按 infra-AGENTS.md 发布纪律人工核对三处一致 | 阶段 2 | ZCode | DONE | P1 | 2026-09-11 |
| T143 | Common.HttpClients 代码审查问题修复（5 项） | IHttpHelper 每请求从 IHttpClientFactory 现取 HttpClient（修复缓存绕过 handler 轮换）；反序列化失败返回失败结果；DownloadFileAsync 临时文件替换防误删；查询参数日期 InvariantCulture；ConvertResponseResult 补 cancellation、无参注册去默认值双份维护；补 4 个回归测试（204→208） | 阶段 2 | ZCode | DONE | P1 | 2026-09-11 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T143 | Common.HttpClients 代码审查问题修复（5 项） | DONE | 2026-09-11 |
| T142 | Common.HttpClients 4.0.0 正式收编 + 去共享框架强依赖 | DONE | 2026-09-11 |
| T141 | README 版本历史恢复原样（移除 rc1 段） | DONE | 2026-08-21 |
| T140 | Azrng.JSqlParser 版本升至 1.0.0-rc1 | DONE | 2026-08-21 |
| T139 | 撤回 T138 版本号收敛（beta10~12 实际均已发布） | DONE | 2026-08-21 |

文件结束。
