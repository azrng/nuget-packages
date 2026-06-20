# Azrng.NMaxCompute.Perf

Azrng.NMaxCompute 的端到端**性能基准**（直连真实 MaxCompute 集群）。

> 这是**控制台项目**（`IsTestProject=false`），不会被 `dotnet test` 收纳，需手动 `dotnet run`。

## 运行

配置环境变量后执行（建议 Release）：

```bash
MAXCOMPUTE_TEST_ENDPOINT="https://service.cn-shanghai.maxcompute.aliyun.com/api" \
MAXCOMPUTE_TEST_ACCESS_ID="<id>" \
MAXCOMPUTE_TEST_SECRET_KEY="<secret>" \
MAXCOMPUTE_TEST_PROJECT="<project>" \
MAXCOMPUTE_TEST_REGION="cn-shanghai" \
MAXCOMPUTE_TEST_TUNNEL_ENDPOINT="https://dt.cn-shanghai.maxcompute.aliyun.com" \
MAXCOMPUTE_PERF_ROWS="50000" \
dotnet run --project test/Azrng.NMaxCompute.Perf -c Release
```

| 变量 | 必需 | 说明 |
|------|------|------|
| `MAXCOMPUTE_TEST_ENDPOINT` | 是 | ODPS REST API 端点 |
| `MAXCOMPUTE_TEST_ACCESS_ID` | 是 | AccessKey ID |
| `MAXCOMPUTE_TEST_SECRET_KEY` | 是 | AccessKey Secret |
| `MAXCOMPUTE_TEST_PROJECT` | 是 | 项目名 |
| `MAXCOMPUTE_TEST_REGION` | 是 | 区域（V4 签名必需） |
| `MAXCOMPUTE_TEST_TUNNEL_ENDPOINT` | 否 | Tunnel 端点 |
| `MAXCOMPUTE_PERF_ROWS` | 否 | 大结果拉取行数，默认 50000 |

## 基准项

| 项 | 含义 |
|----|------|
| [A] 冷启动 SELECT 1 | 完整链路（签名→提交→等待→Tunnel 拉取）单次延迟 |
| [B] 大结果拉取 | 单次拉取 N 行的吞吐（rows/s、µs/row） |
| [C] 混合类型（5 列） | 标量类型解码路径延迟 |
| [D] 小查询平均 | 连续 3 次 SELECT 1 的平均延迟 |

## 说明

- 每次查询都含**实例提交 + 等待 + Tunnel 拉取**，[B] 的吞吐同时受网络与服务端实例调度影响，结果仅供横向参考，不可作为绝对基准。
- 想压纯解码性能，可预先抓取 wire 流离线喂给 `TunnelRecordReader`（本项目未覆盖）。
