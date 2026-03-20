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
文档生成器会：
1. 从 `src` 目录开始递归搜索
2. 只收集 `bin` 目录下的 `.xml` 文件
3. 自动跳过其他位置的 XML 文件（如配置文件）

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
│    - 生成 HTML                           │
│    - 输出到 docs/index.html             │
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

📁 扫描完成: 45 个文件 (0ms)

🔍 正在解析 XML 文件...
  ✓ Azrng.AspNetCore.Core.xml
  ✓ Azrng.Core.xml
  ✓ APIStudy.xml
  ...

✅ 解析完成 (17ms)
   类库: 45
   命名空间: 234
   类型: 567
   成员: 2341

🔄 正在生成 HTML...
✅ 生成完成 (6ms)

📄 输出文件: D:\Gitee\nuget-packages\docs\index.html
📊 文件大小: 1.2 MB

⏱️  总耗时: 25ms
========================================
```
