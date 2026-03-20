# API 文档生成器

为 NuGetPackages 项目自动生成 API 文档的 TypeScript 工具。

## 功能特点

- ⚡ **高性能** - 使用 fast-xml-parser，解析速度提升 10 倍+
- 📊 **详细信息** - 完整展示方法签名、参数、返回值
- 🎨 **美观界面** - Microsoft 风格的深色主题设计
- 🔍 **智能搜索** - 实时搜索类型和成员
- 📱 **响应式** - 支持桌面和移动设备

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

生成的文档将保存在 `docs/index.html`。

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
│   ├── index.ts       # 主入口
│   ├── parser.ts      # XML 解析器
│   ├── generator.ts   # HTML 生成器
│   └── utils.ts       # 工具函数
├── dist/              # 编译输出
├── docs/              # 生成的文档
├── package.json       # 项目配置
└── tsconfig.json      # TypeScript 配置
```

## 配置说明

### XML 文档生成

确保 `.csproj` 文件中启用了 XML 文档生成：

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>CS1591</NoWarn>
</PropertyGroup>
```

### Workflow 配置

GitHub Actions 会自动：
1. 构建所有 `.csproj` 项目
2. 从 `bin/**/*.xml` 收集生成的 XML 文件
3. 运行文档生成器
4. 部署到 GitHub Pages

## 故障排除

### 文档没有生成

1. 检查项目是否启用了 XML 文档生成
2. 确认 `src/` 目录下有 `bin/**/*.xml` 文件
3. 查看构建日志中的错误信息

### GitHub Pages 部署失败

1. 检查仓库设置中是否启用了 GitHub Pages
2. 确认源设置为 `GitHub Actions`
3. 查看部署日志中的错误

## 性能数据

- **解析速度**: ~17ms (2个 XML 文件)
- **生成速度**: ~6ms
- **总耗时**: ~25ms
- **文件大小**: ~600 KB

## 技术栈

- TypeScript 5.3
- Node.js 20
- fast-xml-parser 4.3
- GitHub Actions

## 许可证

MIT
