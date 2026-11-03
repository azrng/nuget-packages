---
rule_id: infra-agents
version: 3.1.0
last_updated: 2026-11-03
dependencies: [agents-root]
---

# 基础设施规则

## 适用范围

- 作用域：打包与发布链路、CI 工作流、环境变量、版本号交付与验证规则
- 本仓库没有容器化部署链路：交付物是 nuget.org 上的包与 GitHub Pages 文档站，不涉及 Docker、端口映射或常驻服务运行
- 触发场景：涉及打包、发布、版本号、CI、环境变量或交付链路时阅读

### 常见任务入口
- 改打包脚本或 CI 工作流：先看发布链路与 `提交前最小回归`
- 改环境变量、连接串或测试配置：先看环境变量约定
- 改包公开 API、版本号或 README：先看发布纪律与 `提交前最小回归`

---

## 技术选择原则
如果仓库已经有真实实现，以现有代码为准，不要强行重构或替换技术栈。
技术债务与重构判断遵循 `collaboration-AGENTS.md` 的「纠错、回退与重构」。

---

## 发布链路

### CI 发布（主链路）
- 触发：push 到 `master` → `.github/workflows/nuget-publish.yml`（windows-latest）
- 流程：`dotnet restore / build / pack PackPackages.slnx --configuration Release` → `NuGet/login@v1` 用 GitHub OIDC 换取 nuget.org 短时 API 密钥 → `dotnet nuget push **/*.nupkg --skip-duplicate`
- 前置条件：nuget.org Trusted Publishing 已登记策略（Owner=azrng / Repository=nuget-packages / Workflow=nuget-publish.yml）；GitHub Secrets 配置 `NUGET_USER`
- 新包要走 CI 发布，必须已加入 `PackPackages.slnx`

### 本地批量打包（备用链路）
- `push-packer.sh`：依赖环境变量 `NUGET_API_KEY`（长期密钥，仅本地使用，禁止提交）；只打包 csproj 中含 `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` 的项目，Debug 配置，输出 `packer/`，逐个 `dotnet nuget push --skip-duplicate`，单个失败继续
- 与 CI 的差异：不经过 `PackPackages.slnx`、用长期密钥；发布正式版本优先走 CI

### 文档站
- `.github/workflows/generate-docs.yml`：push `src/**` 或 `docs-generator/**` 触发，构建 PackPackages（生成 XML 文档）→ Node 20 `docs-generator` 生成静态站 → GitHub Pages 部署

### 技术选择原则
- 修改发布链路（slnx 成员、workflow、脚本）时，必须能说明发布产物来源与推送目标，不允许出现"打包了但没人推"或"推了但版本来源不明"的断链。

---

## 发布纪律（NuGet 包）

- 破坏性变更必须升 major 版本号，不得借 minor / patch 号发布
- 已按某版本开发的内容，发布前不得回退版本号"降级"发出；确需调整版本号时，必须同步核对 README 安装示例、版本更新记录与实际 API 形态是否一致
- 发布前核对三处一致：csproj `<Version>`、README 安装示例 `--version`、README 版本更新记录的最新版本号
- nuget.org 包不可覆盖：发布后发现内容失实，只能在下一版本修正，并在版本更新记录中说明误发情况

> 背景：2026-08 连续出现版本序列事故（JSqlParser beta 序号排序误判、Common.HttpClients 3.1.0 误含 4.0 的 README 与破坏性变更），按 `collaboration-AGENTS.md`「重复问题回顾」固化本节。

---

## 环境变量

- 仓库无统一 `.env` 体系：测试与工具按需读取进程环境变量，命名沿用现有前缀约定（如 `COMMON_CACHE_REDIS_TEST_CONNECTION`、`AZRNG_LOCK_PG_CONN`、`QUACK_PROTOCOL_CONNECTION_STRING`、`MAXCOMPUTE_TEST_*`）
- 新增环境变量：在读取它的测试项目内注释或对应包 README 中说明用途、取值来源与缺省行为（通常未设置即跳过对应用例）；禁止把连接串、密钥写入仓库文件
- `NUGET_API_KEY` 仅本地 `push-packer.sh` 使用；CI 用 OIDC 短时密钥，不落任何长期密钥到仓库或 workflow 明文

---

## 主动建议规则
- 发现打包脚本、CI 工作流、版本号或 README 之间存在不一致、不可复现或文档缺失时，应主动提醒
- 涉及清理环境、删除数据、停止未知进程、修改发布配置等高风险操作时，只能提出建议，必须等待明确确认后执行
- 不确定外部服务地址、密钥来源或包版本号时，应按根 `AGENTS.md` 的查证优先级处理，禁止凭经验写死配置

---

## 提交前最小回归
- 修改 workflow、打包脚本或 slnx 成员时：核对触发分支与推送目标语法正确，并至少执行一次 `dotnet pack`（独立输出目录，见 `backend-AGENTS.md`「.NET 并行隔离」）等价验证产物可生成
- 修改包公开 API、版本号或 README 时：执行 `dotnet pack`（独立输出目录）确认产物可生成，并核对 csproj `<Version>`、README 安装示例 `--version`、CHANGELOG / 版本记录最新版本三处一致
- 修改环境变量、配置项或连接串时：确认读取处的测试缺省行为（未设置即跳过）与文档说明同步更新
- 修改数据库、仓储、依赖注入或外部服务集成时：至少补一项真实链路或等价验证
- CI 工作流无法本地完整复现（OIDC、Pages）时，说明未覆盖项，用 workflow_dispatch 或推送到验证分支的方式补验

---
