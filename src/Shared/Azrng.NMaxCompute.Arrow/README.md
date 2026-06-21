# Azrng.NMaxCompute.Arrow

[Azrng.NMaxCompute](../Azrng.NMaxCompute) 的 **Apache Arrow 格式读取**可选包。

## 作用

让消费者以 **Arrow 列式格式**（`Apache.Arrow.RecordBatch`）读取 MaxCompute 查询结果，便于与 DataFrame / Arrow 分析生态互操作。核心库 `Azrng.NMaxCompute` 不依赖本包；需要 Arrow 时才引入，避免把 `Apache.Arrow` 强加给所有消费者。

## 原理（对应 PyODPS `TunnelArrowReader` / `ArrowStreamReader`）

1. 核心库 `InstanceDownloadSession.OpenArrowStreamAsync` 用 `?arrow` 拉取 **MaxCompute 分帧流**
   （`[4B BE chunk_size][data][4B BE crc32c]` 逐块，末块累计 crccrc）。
2. 本包 `MaxComputeArrowFramedStream` 做**分帧解码**，产出原始 IPC 字节流。
3. MaxCompute arrow 流**不含 schema 消息**（首条即 RecordBatch），`OdpsArrowSchemaConverter` 把 ODPS schema 转 Arrow schema 并经 `PrefixStream` 前置。
4. `Apache.Arrow.Ipc.ArrowStreamReader` 解析 IPC，逐个产出 `RecordBatch`。

## 用法

```csharp
using Azrng.NMaxCompute.Arrow;

using var arrow = await session.OpenArrowReaderAsync(0, session.RecordCount);
Apache.Arrow.Schema schema = arrow.Schema;
while (arrow.ReadNext() is { } batch)
{
    // batch.Column(i) 为 Apache.Arrow.Arrays.*
}
```

## 安装

```bash
dotnet add package Azrng.NMaxCompute.Arrow
```

仅支持 **net8.0+**（受 `Apache.Arrow` 约束）。

## 状态

- 分帧解码、ODPS→Arrow 类型转换、schema 前置、合成 IPC 往返：单元测试通过。
- 真实集群 RecordBatch buffer 布局兼容：校准中（集群集成测试暂 Skip，详见核心库 `MIGRATION.md` P2）。

## 参考

基于 [PyODPS](https://github.com/aliyun/aliyun-odps-python-sdk) 的 `tunnel/io/reader.py`（`ArrowStreamReader` / `TunnelArrowReader`）与 `tunnel/io/types.py`（`odps_schema_to_arrow_schema`）移植。

## License

MIT
