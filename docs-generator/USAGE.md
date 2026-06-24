# 快速开始

## 本地运行

1. **安装依赖**
   ```bash
   cd docs-generator
   npm install
   ```

2. **编译项目**
   ```bash
   npm run build
   ```

3. **生成文档**
   ```bash
   npm start
   ```

4. **预览文档**
   ```bash
   npm run preview
   ```
   然后在浏览器打开 `http://localhost:8080`。

   > ⚠️ **注意**：SPA 架构下不要直接双击 `docs/index.html` 打开（`file://` 协议会被浏览器 CORS 策略拦截，导致 `data.json` 加载失败）。必须通过 HTTP 服务器访问，`npm run preview` 已内置静态服务器。

## GitHub Actions 自动部署

文档会在以下情况自动生成和部署：

### 触发条件

- ✅ 推送到 `main` 或 `master` 分支
- ✅ 修改了 `src/` 或 `docs-generator/` 目录的文件
- ✅ 创建 Pull Request
- ✅ 手动触发（GitHub Actions 页面）

### 查看部署状态

1. 进入仓库的 **Actions** 标签
2. 选择 **Generate API Documentation** workflow
3. 查看运行日志

### 访问在线文档

部署成功后，文档将发布到：
```
https://azrng.github.io/nuget-packages/
```

## 配置 GitHub Pages

1. 进入仓库 **Settings**
2. 选择 **Pages**
3. **Source** 设置为 **GitHub Actions**
4. 保存设置

## 常见问题

### Q: 文档没有更新？

A: 检查以下几点：
- 确认代码已推送到主分支
- 查看 Actions 页面的运行状态
- 确认 XML 文档已正确生成（检查 `.csproj` 配置）

### Q: 本地运行失败？

A: 确保：
- Node.js 版本 >= 20
- 已运行 `npm install`
- 已运行 `npm run build`

### Q: 如何自定义样式？

A: 修改 `src/generator.ts` 中的 `getStyles()` 函数。

### Q: 如何修改前端 SPA 行为（路由、搜索、渲染）？

A: 修改 `src/generator.ts` 中的 `getSpaScript()` 函数，该函数返回内联在 index.html 中的前端 JavaScript 代码。

## 开发建议

1. **修改代码后**，运行 `npm run build` 重新编译
2. **实时预览**，可以使用 `npm run dev`（如果配置了 watch 模式）
3. **调试问题**，查看编译输出和运行日志

## 架构说明

采用 **纯前端 SPA + 数据分离** 架构：
- `index.html`（~30KB）：UI 壳 + 前端 SPA 逻辑（hash 路由、按需渲染、搜索、主题切换）
- `data.json`：全量文档数据，前端启动时 fetch 一次，之后按路由渲染对应视图

这种架构在 2500+ 类型规模下仍能流畅运行，因为首屏只加载 UI 壳，不会全量渲染所有类型节点。hash 路由（`#/type/...`）天然适配 GitHub Pages 子路径部署，无需处理 base path。

## 性能优化

- 使用 `fast-xml-parser` 替代正则表达式
- 数据与 UI 分离，按需渲染（解决大规模卡顿）
- IntersectionObserver 替代全量 offsetTop 重算（滚动目录高亮）
- 编译后的代码性能更好
- 支持大型项目（数千个类型）

## 更新日志

### v3.0.0 (2026-06-24)
- 🏗️ 架构改造：单页 HTML → 纯前端 SPA + data.json 分离
- ⚡ 首屏只加载 ~30KB UI 壳，按需 fetch data.json，解决上千类型规模卡顿
- 🐛 修复泛型参数解析（`List{String}` 内部逗号不再被误切）
- 🐛 修复成员丢失（两遍扫描，XML 中成员排在类型前不再静默丢弃）
- 🐛 优化类型分类准确性（interface 判断改为 `I` + 大写约定）
- 🧹 清理废弃的 index-new.ts / generate.js
- 🔗 hash 路由深链，适配 GitHub Pages 子路径

### v2.0.0 (2024-03-21)
- ✨ 迁移到 NuGetPackages 项目
- ✨ 添加 GitHub Actions 自动部署
- ✨ 优化导航树结构
- ✨ 改进方法签名显示
- ⚡ 性能提升 10 倍
