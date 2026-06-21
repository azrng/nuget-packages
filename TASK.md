# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 当前活跃

| ID | 任务名称 | 目标 | 阶段 | 状态 | 更新时间 |
|----|----------|------|------|------|----------|
| T022 | Azrng.Security 合并与改名 | 将 Common.Security 全层统一改名为 Azrng.Security，吸收 Common.SecurityCrypto 独有能力（RSA JSON、RandomString），丢弃其 Provider/Factory 抽象与手写 SM 实现 | 阶段 1（9 任务全部完成，待用户确认） | REVIEW | 2026-06-20 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T038 | Tunnel 写链路集群端到端验证 + count zigzag bug 修复 + TableUploadSession schema(2.0) 支持 | DONE | 2026-06-21 |
| T037 | README 补 Arrow 说明 + user_info 真实数据读验证（幂等，含中文） | DONE | 2026-06-21 |
| T036 | Arrow schema 前置（ODPS→Arrow 类型转换 + IPC 前置）+ user_info 读集成 + Arrow 集群测试(集群batch兼容待查,暂Skip) | DONE | 2026-06-21 |
| T035 | 边界/回归集成测试：并发查询(守护NRE)/空结果/非法SQL/多行顺序/特殊字符 | DONE | 2026-06-21 |
| T034 | 离线测试补强：Arrow 端到端 IPC 往返 + 写路径 datetime/timestamp/interval/vector 往返 | DONE | 2026-06-21 |
| T025 | 剩余覆盖补全：binary/定长/嵌套复合/Result API + 3 个 parser/factory bug 修复 + 离线解码基准 | DONE | 2026-06-20 |
| T024 | 大结果集单流全量覆盖 + 端到端性能基准项目 | DONE | 2026-06-20 |
| T023 | 集成测试补全：标量/NULL/Unicode/日期时间/复合类型/真·大结果集覆盖 | DONE | 2026-06-20 |




