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
   在浏览器中打开 `docs/index.html`

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

## 开发建议

1. **修改代码后**，运行 `npm run build` 重新编译
2. **实时预览**，可以使用 `npm run dev`（如果配置了 watch 模式）
3. **调试问题**，查看编译输出和运行日志

## 性能优化

- 使用 `fast-xml-parser` 替代正则表达式
- 编译后的代码性能更好
- 支持大型项目（数千个类型）

## 更新日志

### v2.0.0 (2024-03-21)
- ✨ 迁移到 NuGetPackages 项目
- ✨ 添加 GitHub Actions 自动部署
- ✨ 优化导航树结构
- ✨ 改进方法签名显示
- ⚡ 性能提升 10 倍
