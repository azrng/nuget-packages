# 文档生成工作流程说明

## XML 文档位置

### 生成位置
.NET 项目构建后，XML 文档会生成在各个项目的 `bin` 目录下：

```
src/
├── Services/
│   ├── ApiSettingConfig/
│   │   └── bin/
│   │       └── Release/
│   │           └── net8.0/
│   │               └── Azrng.AspNetCore.Core.xml  ← XML 文档
│   ├── APIStudy/
│   │   └── bin/
│   │       └── Release/
│   │           └── net6.0/
│   │               └── APIStudy.xml              ← XML 文档
│   └── ...
```

### 收集规则
文档生成器的 XML 收集范围**严格绑定 `PackPackages.slnx`**：
1. 解析 `PackPackages.slnx`，取出其中包含的所有 `.csproj` 项目路径
2. 对每个项目，递归扫描其 `bin/` 子目录下的 `.xml` 文件
3. 不在该解决方案内的项目（即使本地曾 build 过、bin 下有 xml 残留）不会被收集

> 这样保证本地与 CI 的文档内容一致，不受其他解决方案（如 `CommonStudy.slnx`）build 残留的影响。
> 配置项在 `docs-generator/src/index.ts` 的 `CONFIG.solutionFile`。

## GitHub Actions 工作流程

```yaml
┌─────────────────────────────────────────┐
│ 1. 检出代码                              │
│    Checkout repository                   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ 2. 构建 .NET 项目                        │
│    dotnet build PackPackages.slnx       │
│    - 生成 XML 文档到 bin/Release/       │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ 3. 验证 XML 文件                         │
│    find src -name "*.xml"               │
│         -path "*/bin/Release/*"         │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ 4. 运行文档生成器                        │
│    cd docs-generator                    │
│    npm start                            │
│    - 扫描 src/bin/**/*.xml              │
│    - 解析 XML 内容                       │
│    - 生成 index.html (UI 壳) + data.json │
│    - 输出到 docs/ 目录                   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ 5. 部署到 GitHub Pages                   │
│    Upload docs/ directory               │
└─────────────────────────────────────────┘
```

## 配置说明

### .csproj 配置
确保项目启用了 XML 文档生成：

```xml
<PropertyGroup>
  <!-- 生成 XML 文档文件 -->
  <GenerateDocumentationFile>true</GenerateDocumentationFile>

  <!-- 忽略 CS1591 警告（缺少 XML 注释） -->
  <NoWarn>CS1591</NoWarn>
</PropertyGroup>
```

### 文档生成器配置
`docs-generator/src/index.ts`:

```typescript
const CONFIG = {
  // 从 src 目录查找 XML 文件
  sourceDir: path.join(process.cwd(), 'src'),

  // 输出到 docs/index.html
  outputFile: path.join(process.cwd(), 'docs', 'index.html')
};
```

### Workflow 路径过滤
只在以下路径变更时触发：
- `src/**` - .NET 源代码
- `docs-generator/**` - 文档生成器代码
- `.github/workflows/generate-docs.yml` - Workflow 配置

## 本地测试

### 1. 构建项目生成 XML
```bash
# 构建 PackPackages 解决方案
dotnet build PackPackages.slnx \
  -p:GenerateDocumentationFile=true \
  -p:NoWarn=CS1591 \
  -c Release
```

### 2. 运行文档生成器
```bash
cd docs-generator
npm start
```

### 3. 预览文档
```bash
# 在浏览器中打开
start docs/index.html
```

## 故障排除

### 问题：没有找到 XML 文件

**原因**：
- 项目未启用 XML 文档生成
- 构建配置不正确

**解决**：
1. 检查 `.csproj` 文件中是否有 `GenerateDocumentationFile>true</GenerateDocumentationFile>`
2. 确认已运行 `dotnet build`
3. 检查 `src/**/bin/Release/**/*.xml` 是否存在

### 问题：文档内容不完整

**原因**：
- 代码缺少 XML 注释
- 项目未正确生成文档

**解决**：
1. 在代码中添加 `/// <summary>` 注释
2. 重新构建项目
3. 确认 XML 文件包含内容

### 问题：GitHub Actions 失败

**原因**：
- 构建失败
- 权限问题
- 路径配置错误

**解决**：
1. 查看 Actions 日志
2. 确认 GitHub Pages 已启用
3. 检查 workflow 配置

## 性能指标

- **构建时间**: ~2-3 分钟（取决于项目数量）
- **文档生成**: ~20-50ms（取决于 XML 文件数量）
- **总耗时**: ~3-4 分钟

## 输出示例

成功运行后会看到：

```
========================================
  API Documentation Generator v2.0
  高性能 TypeScript 版本
========================================

📁 扫描完成: 265 个文件 (Xms)

🔍 正在解析 XML 文件...
  ✓ Azrng.AspNetCore.Core.xml
  ✓ Azrng.Core.xml
  ...

✅ 解析完成 (916ms)
   类库: 213
   命名空间: 158
   类型: 2542
   成员: 15658

🔄 正在生成文档...
✅ 生成完成 (41ms)

📄 输出目录: .../docs
   index.html: 30.3 KB (UI 壳 + 前端 SPA)
   data.json:  5.6 MB (全量文档数据)
✨ 已创建 .nojekyll 文件（跳过 Jekyll 处理）

⏱️  总耗时: 2352ms
========================================
```
