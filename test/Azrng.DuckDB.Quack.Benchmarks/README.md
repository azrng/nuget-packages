# Azrng.DuckDB.Quack Benchmarks

针对 Azrng.DuckDB.Quack ADO.NET 客户端的端到端性能基准测试。测试结果衡量的是**客户端协议解析 + 网络往返**的开销，DuckDB 引擎本身的执行时间已包含在内但通常可忽略。

---

## 目录

- [快速开始](#快速开始)
- [测试场景说明](#测试场景说明)
- [运行方式](#运行方式)
- [结果解读](#结果解读)
- [与其他语言对比的方法](#与其他语言对比的方法)
- [环境要求](#环境要求)
- [已知问题](#已知问题)

---

## 快速开始

```bash
# 1. 启动 Quack 测试容器（首次需要构建镜像）
docker compose -f docker/compose.yml up -d --build

# 2. 等待容器健康检查通过
docker ps --filter "publish=9494" --format "{{.Status}}"
# 输出应包含 "(healthy)"

# 3. 运行全部基准测试（约 2~3 分钟）
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release

# 4. 查看结果
# 控制台输出 + BenchmarkDotNet.Artifacts/results/ 下的 .md 和 .json 文件
```

---

## 测试场景说明

共 6 类基准测试、16 个测试用例：

### 1. ConnectionBench — 连接握手

| 用例 | 说明 |
|------|------|
| `Open+Dispose handshake` | 完整的 TCP + 认证 + 协议 Connect 握手。这是连接池存在感的核心——池化后此开销被摊销到零。 |

**关注指标**：Mean（越低越好）、Allocated（连接建立的 GC 压力）

### 2. QueryBench — 查询延迟

| 用例 | 说明 |
|------|------|
| `SELECT 1` | 最小查询，衡量纯协议往返开销 |
| `SELECT @a + @b` | 参数化查询，额外衡量参数渲染成本 |
| `COUNT/SUM over 10k rows` | 服务端聚合，衡量小结果集往返 |

**关注指标**：三个用例的差距 = 参数渲染 + 执行时间的差异

### 3. InsertBench — 写入吞吐量

| 用例 | Rows | 说明 |
|------|------|------|
| `Batch VALUES insert` | 100/1000/10000 | 单条 INSERT 拼接多行 VALUES |
| `Per-row parameterised insert` | 100/1000/10000 | 每行一条参数化 INSERT |

**关注指标**：Batch vs Per-row 的**加速比**（理想值应接近 N 倍）

### 4. ResultSetBench — 大结果集读取

| 用例 | Rows | 说明 |
|------|------|------|
| `Read N rows (2 cols)` | 10000 | 单次 PrepareResponse 即可返回 |
| `Read N rows (2 cols)` | 100000 | 需要多次 FetchAsync 往返 |

**关注指标**：Mean（客户端解码速度）、Allocated（GC 压力）、rows/sec 吞吐量

### 5. PoolBench — 连接池

| 用例 | 说明 |
|------|------|
| `Pool acquire + return` | 从池中获取连接并归还的开销 |

**关注指标**：Mean（应远低于 ConnectionBench 的握手时间）

### 6. ConcurrencyBench — 并发吞吐量

| 用例 | Degree | 说明 |
|------|--------|------|
| `Degree parallel scalar queries` | 4/16/64 | N 个独立连接并发执行标量查询 |

**关注指标**：Degree / Mean = 每秒查询吞吐量 (queries/sec)

---

## 运行方式

### 运行全部测试

```bash
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release
```

### 运行指定测试类

```bash
# 只跑查询测试
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release -- --filter "*Query*"

# 只跑结果集测试
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release -- --filter "*ResultSet*"

# 只跑连接和池测试
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release -- --filter "*Connection*|*Pool*"
```

### 干跑模式（快速验证代码改动不报错）

```bash
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release -- --filter "*ResultSet*" --job dry
```

### 自定义连接字符串

```bash
# 使用环境变量覆盖默认连接
export QUACK_PROTOCOL_CONNECTION_STRING="Host=remote-host;Port=9494;Token=YOUR_TOKEN;DisableSsl=true"
dotnet run --project tests/Azrng.DuckDB.Quack.Benchmarks -c Release
```

### 输出格式

结果自动导出到 `BenchmarkDotNet.Artifacts/results/` 目录：

| 文件 | 用途 |
|------|------|
| `*-report-github.md` | GitHub Markdown 表格，适合贴到 PR/文档 |
| `*-report-full-compressed.json` | 完整 JSON 数据，适合程序化分析 |

---

## 结果解读

### BenchmarkDotNet 输出字段含义

| 字段 | 含义 |
|------|------|
| **Mean** | 算术平均值，核心指标 |
| **Error** | 99.9% 置信区间半宽 |
| **StdDev** | 标准差，衡量稳定性 |
| **Allocated** | 单次操作的托管内存分配量 |
| **Gen0/Gen1/Gen2** | 每 1000 次操作的 GC 回收次数 |
| **NA** | 测试失败（通常因异常），需查看日志 |

### 如何判断"好"与"坏"

- **连接握手** < 5ms → 良好；> 10ms → 需要排查网络/TLS
- **标量查询** < 1ms → 优秀；1~3ms → 正常；> 5ms → 网络瓶颈
- **读取吞吐量** > 2M rows/s → 优秀（HTTP 协议）；< 0.5M rows/s → 需要优化解码
- **批量插入加速比** > 10× → 良好；< 5× → 批量实现有问题

---

## 与其他语言对比的方法

### 方法一：使用公开基准测试数据

许多数据库驱动/ORM 发布了官方基准测试结果，可直接对比：

| 语言/驱动 | 数据库 | 基准测试来源 |
|-----------|--------|-------------|
| Go pgx | PostgreSQL | [github.com/jackc/pgx#benchmarks](https://github.com/jackc/pgx) |
| Rust tokio-postgres | PostgreSQL | [github.com/sfackler/rust-postgres](https://github.com/sfackler/rust-postgres) |
| Java HikariCP | PostgreSQL | [github.com/brettwooldridge/HikariCP](https://github.com/brettwooldridge/HikariCP/#benchmarks) |
| C# Npgsql | PostgreSQL | [github.com/npgsql/npgsql](https://github.com/npgsql/npgsql) |
| Python psycopg3 | PostgreSQL | [www.psycopg.org/psycopg3/docs/perf](https://www.psycopg.org/psycopg3/docs/perf/) |

**对比时注意对齐的维度**：

```
对比维度          我们的数据            对方的数据
─────────────────────────────────────────────────
标量查询延迟      SELECT 1 → 0.6ms     对方的单次查询延迟
大结果集吞吐      10k rows → 3.7M/s    对方的批量读取 rows/s
内存效率          10k rows → 2.82MB     对方的内存占用
连接建立          握手 → 2.6ms          对方的连接创建延迟
```

### 方法二：本地复现对比测试

如果需要严格的苹果对苹果对比，可以在同一台机器上运行对方的基准测试：

#### Python (psycopg2 + DuckDB)

```python
import time
import duckdb

conn = duckdb.connect()
start = time.perf_counter()
for _ in range(1000):
    conn.execute("SELECT 1").fetchone()
elapsed = time.perf_counter() - start
print(f"Python DuckDB: {elapsed/1000*1000:.2f} ms/query")
```

#### Go (database/sql + DuckDB)

```go
// 使用 github.com/marcboeker/go-duckdb
db, _ := sql.Open("duckdb", "")
start := time.Now()
for i := 0; i < 1000; i++ {
    db.QueryRow("SELECT 1").Scan(&result)
}
fmt.Printf("Go DuckDB: %v/query\n", time.Since(start)/1000)
```

#### Rust (duckdb crate)

```rust
use duckdb::Connection;
use std::time::Instant;

let conn = Connection::open_in_memory().unwrap();
let start = Instant::now();
for _ in 0..1000 {
    conn.query_row("SELECT 1", [], |_| Ok(())).unwrap();
}
println!("Rust DuckDB: {:?}/query", start.elapsed() / 1000);
```

#### Java (JDBC + DuckDB)

```java
Connection conn = DriverManager.getConnection("jdbc:duckdb:");
long start = System.nanoTime();
for (int i = 0; i < 1000; i++) {
    PreparedStatement ps = conn.prepareStatement("SELECT 1");
    ps.executeQuery();
    ps.close();
}
long elapsed = System.nanoTime() - start;
System.out.printf("Java DuckDB: %.2f ms/query%n", elapsed / 1_000_000.0 / 1000);
```

### 方法三：标准化对比指标

为确保对比公平，统一使用以下指标：

| 指标 | 计算公式 | 说明 |
|------|---------|------|
| **单次查询延迟** (ms) | `总时间 / 查询次数` | 越低越好 |
| **读取吞吐量** (rows/s) | `总行数 / 总时间` | 越高越好 |
| **每行内存开销** (bytes/row) | `总分配内存 / 总行数` | 越低越好 |
| **写入吞吐量** (rows/s) | `插入行数 / 总时间` | 越高越好 |
| **连接建立延迟** (ms) | `从 new 到 Open 完成` | 越低越好 |

### 对比结果记录模板

建议将每次对比结果记录在下方表格中，便于跟踪性能变化：

```markdown
| 指标 | C# Quack | Python psycopg2 | Go pgx | Rust | Java JDBC |
|------|----------|----------------|--------|------|-----------|
| 标量查询 (ms) | | | | | |
| 10k 行读取 (rows/s) | | | | | |
| 10k 行内存 (MB) | | | | | |
| 连接建立 (ms) | | | | | |
| 1000 行批量插入 (ms) | | | | | |
```

---

## 环境要求

| 组件 | 版本要求 | 说明 |
|------|---------|------|
| .NET SDK | 10.0+ | `net10.0` 目标框架 |
| Docker | 20.10+ | 运行 Quack 测试容器 |
| DuckDB | 1.5.3 | 容器内已包含 |
| Quack 扩展 | 匹配 DuckDB 版本 | 容器内已包含 |

### 连接字符串

默认连接字符串（硬编码在 `Program.cs` 中）：

```
Host=localhost;Port=9494;Token=E7231CE2CE78902BA280F3B9158BEB30;DisableSsl=true
```

可通过环境变量 `QUACK_PROTOCOL_CONNECTION_STRING` 覆盖。

### 性能测试注意事项

1. **必须使用 Release 构建**：Debug 模式会禁用 JIT 优化，结果无参考价值
2. **关闭后台程序**：避免其他进程抢占 CPU/网络
3. **多次运行取稳定值**：BenchmarkDotNet 默认 warmup 2 次 + 迭代 3~5 次
4. **避免笔记本电池模式**：CPU 降频会严重影响结果
5. **容器健康检查**：确保 `docker ps` 显示 `(healthy)` 后再运行测试

---

## 已知问题

### ResultSetBench 100k 行 NA

- **现象**：`Read N rows (2 cols) end-to-end [Rows=100000]` 输出 NA
- **根因**：Quack 服务端结果集生命周期过短，多轮 FetchAsync 期间服务端关闭结果集
- **状态**：客户端 UUID 前向保持已修复；服务端 TTL 问题待 Quack 上游修复
- **影响**：≤ 20k 行不受影响；≥ 50k 行间歇性失败

### ConcurrencyBench Degree=64 NA

- **现象**：`Degree parallel scalar queries [Degree=64]` 输出 NA
- **根因**：Windows 临时端口耗尽 (SocketException 10048)
- **解决**：调低 Degree 参数，或调整 Windows 注册表增加临时端口范围

---

## 添加新的基准测试

在 `QuackBenchmarks.cs` 中添加新的 benchmark 类：

```csharp
/// <summary>
/// 你的测试描述。
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 3)]
public class MyNewBench : IAsyncDisposable
{
    private QuackConnection _connection = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _connection = new QuackConnection(Program.ConnectionString);
        await _connection.OpenAsync();
    }

    [Benchmark(Description = "My benchmark description")]
    public async Task MyBenchmark()
    {
        // 你的测试逻辑
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
```

然后在 `Program.cs` 的 `BenchmarkRunner.Run` 中注册：

```csharp
BenchmarkRunner.Run(new[]
{
    typeof(ConnectionBench),
    typeof(QueryBench),
    typeof(InsertBench),
    typeof(ResultSetBench),
    typeof(PoolBench),
    typeof(ConcurrencyBench),
    typeof(MyNewBench),  // ← 新增
}, config);
```
