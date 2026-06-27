# API 文档生成器

为 NuGetPackages 项目自动生成 API 文档的 TypeScript 工具。

## 功能特点

- ⚡ **高性能 SPA 架构** - 数据与 UI 分离，首屏只加载 UI 壳（~30KB），按需 fetch data.json，支持上千类型规模不卡顿
- 📊 **详细信息** - 完整展示方法签名、参数、返回值
- 🎨 **美观界面** - Microsoft 风格的深色主题设计（支持深色/浅色切换）
- 🔍 **全局搜索** - 实时搜索类型和成员（Ctrl+K 快捷键），结果点击直达
- 📂 **可搜索类库选择器** - 左上角类库下拉支持输入关键字筛选，快速定位目标类库
- 🌳 **智能导航树** - 左侧按命名空间→类型组织，自动展开当前类型所在命名空间，当前选中项高亮并滚动定位，展开/折叠状态记忆
- 📱 **响应式** - 支持桌面和移动设备
- 🔗 **深链分享** - hash 路由（`#/type/...`），天然适配 GitHub Pages 子路径
- 🎯 **范围精确** - 文档内容严格绑定 `PackPackages.slnx`，不受其他解决方案 build 残留影响
- 📖 **类库 README 展示** - 构建时读取 `PackPackages.slnx` 内每个类库同目录的 `README.md`（范围与文档同源），渲染成 HTML 在类库详情页展示

## 本地开发

### 安装依赖

```bash
cd docs-generator
npm install
```

### 编译 TypeScript

```bash
npm run build
```

### 生成文档

```bash
npm start
```

生成的文档将保存在 `docs/` 目录：
- `index.html` - UI 壳 + 前端 SPA 逻辑（约 30KB，不含数据）
- `data.json` - 全量文档数据（按规模 1-6MB，前端按需 fetch）

### 预览文档

```bash
npm run preview
```

然后在浏览器打开 `http://localhost:8080`。

> ⚠️ **不要直接双击 `docs/index.html` 打开**：SPA 架构下页面需 `fetch('data.json')` 加载数据，`file://` 协议会被浏览器 CORS 策略拦截导致加载失败。必须通过 HTTP 服务器访问，`npm run preview` 已内置静态服务器（基于 serve）。

### 开发模式

```bash
npm run dev
```

## 自动化部署

项目配置了 GitHub Actions，会在以下情况自动生成和部署文档：

### 触发条件

1. **推送到主分支**
   - 当代码推送到 `main` 或 `master` 分支时
   - 路径匹配：`src/**`, `docs-generator/**`

2. **Pull Request**
   - 创建针对主分支的 PR 时

3. **手动触发**
   - 在 GitHub Actions 页面手动运行

### 工作流程

```
┌─────────────────┐
│  代码提交        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  构建 .NET 项目  │
│  生成 XML 文档   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  运行文档生成器  │
│  解析 XML        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  生成 HTML 文档  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  部署到 Pages    │
└─────────────────┘
```

## 文档位置

- **本地预览**: `docs/index.html`
- **在线访问**: `https://[your-username].github.io/nuget-packages/`

## 项目结构

```
docs-generator/
├── src/
│   ├── index.ts       # 主入口（解析 slnx、收集 XML、生成双产物）
│   ├── parser.ts      # XML 解析器（两遍扫描，成员归属到类型）
│   ├── generator.ts   # 文档生成器（输出 index.html 壳 + data.json）
│   ├── markdown.ts    # Markdown 渲染（README.md → HTML，marked，同步）
│   └── utils.ts       # 工具函数（分类、转义、压缩）
├── dist/              # 编译输出
├── docs/              # 生成的文档（index.html + data.json + .nojekyll）
├── package.json       # 项目配置（含 build/start/preview 脚本）
└── tsconfig.json      # TypeScript 配置
```

## 配置说明

### XML 文档生成

确保 `.csproj` 文件中启用了 XML 文档生成（本仓库已通过根目录 `Directory.Build.props` 统一开启）：

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>CS1591</NoWarn>
</PropertyGroup>
```

### 文档生成器配置

文档生成范围通过 `docs-generator/src/index.ts` 的 `CONFIG.solutionFile` 控制，默认绑定仓库根的 `PackPackages.slnx`：

```typescript
const CONFIG = {
  solutionFile: path.join(projectRoot, 'PackPackages.slnx'),
  outputDir: path.join(projectRoot, 'docs'),
  // ...
};
```

如需改为其他解决方案，修改 `solutionFile` 即可。生成器会解析该 slnx 包含的项目，只收集这些项目 `bin/` 下的 XML。

### Workflow 配置

GitHub Actions 会自动：
1. 构建 `PackPackages.slnx` 解决方案
2. 文档生成器解析该 slnx，只收集其中项目的 `bin/**/*.xml`
3. 运行文档生成器（输出 index.html + data.json）
4. 部署到 GitHub Pages

## 故障排除

### 文档没有生成

1. 检查项目是否启用了 XML 文档生成
2. 确认已构建 `PackPackages.slnx`，其项目的 `bin/**/*.xml` 已生成
3. 查看构建日志中的错误信息

### 本地打开 docs/index.html 一直"正在加载文档数据..."

这是 **直接双击打开（`file://` 协议）导致的**。SPA 架构下页面需 `fetch('data.json')` 加载数据，浏览器会以 CORS 策略拦截 `file://` 的请求。

**解决**：用 `npm run preview` 启动静态服务器，通过 `http://localhost:8080` 访问。

### 文档内容包含了不属于 PackPackages.slnx 的项目

文档生成器只收集 `CONFIG.solutionFile`（默认 `PackPackages.slnx`）包含项目的 XML。若发现多出/少了项目：

1. 确认 `docs-generator/src/index.ts` 里 `solutionFile` 指向预期的 slnx
2. 确认该 slnx 的 `<Project>` 节点包含了目标项目
3. 确认目标项目已 build（`bin/` 下有 xml）

### GitHub Pages 部署失败

1. 检查仓库设置中是否启用了 GitHub Pages
2. 确认源设置为 `GitHub Actions`
3. 查看部署日志中的错误

## 性能数据

基于真实仓库（`PackPackages.slnx`，58 个项目，239 个 XML 文件，2290 类型，14505 成员）实测：

- **扫描速度**: ~185ms（解析 slnx + 收集 239 个 XML）
- **生成速度**: ~39ms
- **总耗时**: ~932ms
- **index.html**: ~42 KB（UI 壳 + 前端 SPA，不含数据）
- **data.json**: ~5.1 MB（全量文档数据，按需 fetch）

对比改造前：旧版把全部数据内联进单个 index.html（同等规模约 5-10 MB），首屏全量渲染导致卡顿；新版数据与 UI 分离，首屏只加载 ~42KB，按需渲染，彻底解决大规模卡顿问题。

## 技术栈

- TypeScript 5.3
- Node.js 20
- fast-xml-parser 4.3（XML 解析）
- 原生 JavaScript（前端 SPA，hash 路由 + IntersectionObserver，零运行时依赖）
- GitHub Actions

## 许可证

MIT
