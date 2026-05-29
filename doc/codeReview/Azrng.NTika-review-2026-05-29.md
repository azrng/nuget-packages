# Azrng.NTika 代码审查报告

**审查日期：** 2026-05-29
**审查范围：** Azrng.NTika.Core + 16 个 Parser 包 + 17 个 Test 项目
**审查维度：** 安全性、正确性、资源管理、测试覆盖

---

## 一、问题汇总

| 严重程度 | 数量 | 关键主题 |
|---------|------|---------|
| Critical | 7 | 解压炸弹、XXE、输出损坏、无限递归 |
| High | 12 | 异常吞没、RTF Unicode 错误、路径穿越、输出无大小限制 |
| Medium | 13 | 线程安全、Markdown 注入、HTML 转义不全、共享状态可变 |
| Low | 7 | 哈希质量、冗余流定位、静默异常 |

---

## 修复状态（2026-05-29）

**修复提交：** `4909838 fix(ntika): address parser review issues`

| 编号 | 状态 | 说明 |
| --- | --- | --- |
| C1 | 已修复 | ArchiveParser 增加嵌入条目大小检查和有界复制 |
| C2 | 已修复 | ArchiveParser 移除裸 catch，仅捕获格式和 IO 相关异常 |
| C3 | 已修复 | EmlParser 附件解码写入 `LimitedMemoryStream`，执行字节上限 |
| C4 | 已修复 | OfficeParser 自动识别改为事件缓冲，成功后才回放输出 |
| C5 | 已修复 | ODF XML 读取改用禁用 DTD/Resolver 的安全 `XmlReader` |
| C6 | 已修复 | `EmbeddedDocumentUtil.PushDepth` 执行深度限制 |
| C7 | 已修复 | ToHtml/ToMarkdown 接入 `LimitedContentHandler` 输出限制 |
| H1 | 已修复 | 压缩比检查改为浮点除法 |
| H2 | 已修复 | HTML/Markdown suppress 状态改为嵌套计数 |
| H3 | 未处理 | 路径字符串重载安全责任/根目录白名单尚未设计 |
| H4 | 已修复 | HTML charset 检测增加安全白名单 |
| H5 | 部分修复 | Office/Odf/Email 相关裸 catch 已处理，YamlParser 等仍待后续收敛 |
| H6 | 已修复 | Email HTML-only body 改为剥离标签后输出文本 |
| H7 | 已修复 | RTF 负 Unicode 值按 unsigned 16-bit 转换 |
| H8 | 已修复 | RTF 十六进制转义按 ANSI 代码页解码 |
| H9 | 已修复 | Markdown 链接 href 转义括号和方括号 |
| H10 | 已修复 | HTML 属性值补充单引号转义 |
| H11 | 已修复 | EmbeddedLimits 默认资源数和字节数改为有限值 |
| H12 | 未处理 | TimeoutLimits 仍未接入解析管道，需独立设计 |
| M1 | 未处理 | Tika.MaxStringLength 线程安全未调整 |
| M2 | 已修复 | ParseContext 改为 ConcurrentDictionary |
| M3 | 已修复 | Metadata 改为 ConcurrentDictionary |
| M4 | 已修复 | MediaTypeRegistry 继承表改为 ConcurrentDictionary |
| M5 | 已修复 | HTML/Markdown 分支统一应用输出限制 |
| M6 | 未处理 | XHTMLContentHandler `new`/`override` 设计未调整 |
| M7 | 已修复 | OfficeParser 处理 Formula Error 单元格 |
| M8 | 已修复 | CsvConfig 使用防御性副本，避免修改共享实例 |
| M9 | 已修复 | ArchiveParser 增加条目数量限制 |
| M10 | 未处理 | Archive entry.Key 仅空值兜底，未做路径规范化 |
| M11 | 未处理 | 语言检测词匹配性能优化未处理 |
| M12 | 未处理 | Html/Json/Rtf 流式解析改造未处理 |
| M13 | 未处理 | ODF 重复文本风险未处理 |
| L1 | 未处理 | ContentHandlerDecorator.Handler readonly 未调整 |
| L2 | 未处理 | NameDetector 裸 catch 未处理 |
| L3 | 未处理 | TextDetector 裸 catch 未处理 |
| L4 | 未处理 | TemporaryResources 清理异常策略未处理 |
| L5 | 已修复 | EmbeddedLimits 哈希改为 `HashCode.Combine` |
| L6 | 未处理 | Property 静态注册表增长风险未处理 |
| L7 | 未处理 | CompositeEncodingDetector 提前退出位置恢复未处理 |

**验证状态：** 已顺序执行全部 `Azrng.NTika*.Test` 项目的 `net8.0` 测试，全部通过。`ThirdNugetStudy.slnx --framework net8.0` 构建未通过，原因是 solution 中既有项目未包含 `net8.0` 目标，不属于本次 NTika 修复。

---

## 二、Critical 级别问题

### C1. ArchiveParser 无解压炸弹保护
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Archive/ArchiveParser.cs:73-76`
- **问题：** 每个压缩条目被完整复制到 `MemoryStream`，无大小限制。`EmbeddedLimits.MaxEmbeddedBytes` 已定义但从未检查。恶意 zip bomb 可导致 `OutOfMemoryException`。
- **修复建议：** 复制前检查 `entry.Size` 是否超过 `limits.MaxEmbeddedBytes`，使用有界复制。

### C2. ArchiveParser 裸 catch 吞没 OutOfMemoryException
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Archive/ArchiveParser.cs:91-93, 100-103`
- **问题：** `catch { }` 吞没所有异常包括 `OutOfMemoryException`，掩盖解压炸弹导致的内存耗尽。
- **修复建议：** 仅捕获格式相关异常，让 `OutOfMemoryException` 传播。

### C3. EmlParser 附件解压无大小限制
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Email/EmlParser.cs:77`
- **问题：** `mimePart.Content.DecodeTo(ms)` 将整个附件解码到内存，无大小限制。恶意邮件的炸弹附件可耗尽内存。
- **修复建议：** 使用有界复制，检查 `limits.MaxEmbeddedBytes`。

### C4. OfficeParser TryAutoDetect 部分解析失败导致 XHTML 输出损坏
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Office/OfficeParser.cs:143-169`
- **问题：** `StartDocument()` 在第 34 行调用一次，`TryAutoDetect` 依次尝试 `ParseXlsx` → `ParseXls` → `ParseDocx`。若 `ParseXlsx` 部分输出 XHTML 后抛异常，catch 吞掉异常后 `ParseXls` 在不完整结构上继续输出，产生未闭合标签的畸形 XHTML。
- **修复建议：** 使用临时缓冲区捕获输出，成功后才提交；或在失败时重置 handler 状态。

### C5. OdfParser XXE 漏洞
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Odf/OpenDocumentParser.cs:46, 95`
- **问题：** `XDocument.Load(entryStream)` 未使用安全的 `XmlReaderSettings`。恶意 ODF 的 `content.xml` 或 `meta.xml` 中的外部实体定义可能导致数据泄露或 SSRF。
- **修复建议：** 通过 `XmlReader.Create(entryStream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null })` 加载。

### C6. 嵌入文档深度限制未执行 — 可能 StackOverflowException
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Extractor/EmbeddedDocumentUtil.cs:27-35`
- **问题：** `PushDepth()` 递增深度计数器但从未检查 `EmbeddedLimits.MaxEmbeddedDepth`。恶意文档（zip 嵌套 zip 嵌套 zip...）可触发无限递归。
- **修复建议：** 在 `PushDepth()` 或 `ParseEmbedded` 中递归前检查深度。

### C7. ToHtml/ToMarkdown 输出无大小限制
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Tika.cs:130-182`
- **问题：** `ToText()` 使用 `WriteOutContentHandler(_maxStringLength)` 限制输出，但 `ToHtml()` 和 `ToMarkdown()` 传递原始 handler 无写入限制。恶意文档可产生任意大输出。
- **修复建议：** 对 HTML/Markdown handler 应用相同的大小限制机制。

---

## 三、High 级别问题

### H1. SecureContentHandler 压缩比检查使用整数除法
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Sax/SecureContentHandler.cs:54`
- **问题：** `_characterCount / (_stream.CurrentPosition - _startByte)` 整数除法截断小数，低估压缩比，可能放过 zip bomb。
- **修复建议：** 使用浮点除法 `(double)_characterCount / (...)` 。

### H2. ToMarkdownContentHandler / ToHTMLContentHandler 嵌套 suppress 元素 bug
- **文件：** `ToMarkdownContentHandler.cs:145-164`, `ToHTMLContentHandler.cs:51-104`
- **问题：** `_suppressOutput` 是单个 bool。对于 `<head><style>...</style></head>`，关闭 `</style>` 时设置 `_suppressOutput = false`，但外层 `<head>` 仍开启，`</style>` 和 `</head>` 之间的内容被错误输出。
- **修复建议：** 将 `_suppressOutput` 改为 `int` 计数器，开始时递增，结束时递减。

### H3. 路径穿越 — TikaInputStream.Get(string) 和 Tika 字符串重载
- **文件：** `IO/TikaInputStream.cs:52-55`, `Tika.cs:72-75, 125-128, 152-155, 179-182`
- **问题：** 字符串路径直接传给 `new FileInfo()` 无验证。若调用者传入用户控制的输入，攻击者可通过 `../../../etc/passwd` 读取任意文件。
- **修复建议：** 记录安全责任或添加路径验证。

### H4. HtmlMetaEncodingDetector — 攻击者控制的 charset 传入 GetEncoding
- **文件：** `src/Shared/Tika/Azrng.NTika.EncodingDetectors/HtmlMetaEncodingDetector.cs:49`
- **问题：** HTML meta 标签中的 charset 值直接传给 `Encoding.GetEncoding(charset)`。恶意 HTML 可注入异常编码名。
- **修复建议：** 对 charset 进行已知安全值白名单验证。

### H5. 多个 Parser 的裸 catch 吞没所有异常
- **文件：**
  - `OfficeParser.cs:152, 159, 168` — TryAutoDetect 三处
  - `YamlParser.cs:48-54`
  - `OpenDocumentParser.cs:81-84, 125-127`
  - `EmlParser.cs:95-97`
- **问题：** `catch { }` 吞没所有异常包括 `OutOfMemoryException`、`SecurityException`、`StackOverflowException`。
- **修复建议：** 仅捕获格式相关异常。

### H6. EmlParser 原始 HTML body 作为文本输出
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Email/EmlParser.cs:50`
- **问题：** `message.TextBody ?? message.HtmlBody` — 当 TextBody 为 null 使用 HtmlBody 时，原始 HTML 标记作为字符数据输出。
- **修复建议：** 使用 HtmlBody 时通过 HtmlParser 解析或剥离标签。

### H7. RtfParser 负 Unicode 值产生错误字符
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Rtf/RtfParser.cs:109-120`
- **问题：** RTF `\u` 控制字使用有符号 16 位整数。值 >= 32768 编码为负数，`(char)code` 对负 int 产生错误输出。
- **修复建议：** 添加 `if (code < 0) code += 65536;`。

### H8. RtfParser 十六进制字符当作 Unicode 码点
- **文件：** `src/Shared/Tika/Azrng.NTika.Parsers.Rtf/RtfParser.cs:123-131`
- **问题：** RTF `\'XX` 十六进制转义代表文档代码页中的字节，非 Unicode 码点。代码假设代码页 = Unicode，非 ASCII 文本产生错误字符。
- **修复建议：** 使用编码感知转换，或至少记录 Latin-1 假设。

### H9. Markdown 注入 — ToMarkdownContentHandler
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Sax/ToMarkdownContentHandler.cs:114-117, 219-223`
- **问题：** 链接 href 值直接写入 markdown 输出，包含 `)` 的恶意 href 可突破链接语法注入任意 markdown。
- **修复建议：** 转义 href 中的括号和方括号。

### H10. HTML 属性值转义不完整
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Sax/ToHTMLContentHandler.cs:163-171`
- **问题：** `EscapeAttributeValue` 转义双引号但不转义单引号。在单引号属性上下文中可能导致 XSS。
- **修复建议：** 同时转义单引号 `'` → `&#39;`。

### H11. EmbeddedLimits 默认值允许无限资源和字节
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Config/EmbeddedLimits.cs:5-6`
- **问题：** `MaxEmbeddedResources = -1` 和 `MaxEmbeddedBytes = -1` 默认无限制。解析不受信输入时允许无限嵌入。
- **修复建议：** 设置合理默认值（如 1000 资源、100MB）。

### H12. TimeoutLimits 已定义但从未执行
- **文件：** `src/Shared/Tika/Azrng.NTika.Core/Config/TimeoutLimits.cs`
- **问题：** 类存在但解析管道中无代码读取或执行这些值，给人虚假的安全感。
- **修复建议：** 集成到解析管道或移除直到可正确实现。

---

## 四、Medium 级别问题

### M1. Tika.MaxStringLength 是可变字段，无同步
- **文件：** `Tika.cs:18, 48-52`
- **建议：** 标记 `volatile` 或使用 `Interlocked`，或构造后不可变。

### M2. ParseContext 非线程安全
- **文件：** `Model/ParseContext.cs:8`
- **建议：** 使用 `ConcurrentDictionary<Type, object>` 或文档说明非线程安全。

### M3. Metadata 非线程安全
- **文件：** `Model/Metadata.cs:12`
- **建议：** 同上。

### M4. MediaTypeRegistry 混用 ConcurrentDictionary 和 Dictionary
- **文件：** `Model/MediaTypeRegistry.cs:9-10`
- **问题：** `_registry` 是 ConcurrentDictionary 但 `_inheritance` 是普通 Dictionary，存在数据竞争。
- **建议：** `_inheritance` 也用 ConcurrentDictionary。

### M5. ContentHandlerFactory 的 maxStringLength 仅对 Text 有效
- **文件：** `Sax/ContentHandlerFactory.cs:18-27`
- **问题：** Html 和 Markdown 分支忽略 maxStringLength 参数。
- **建议：** 对所有 handler 类型一致应用限制。

### M6. XHTMLContentHandler 使用 `new` 而非 `override`
- **文件：** `Sax/XHTMLContentHandler.cs:34, 44`
- **问题：** `StartDocument()` 和 `EndDocument()` 用 `new` 隐藏基类方法。通过基类引用调用时跳过 XHTML 信封生成。
- **建议：** 改为 `virtual` + `override`，或文档说明设计意图。

### M7. OfficeParser GetCellStringValue 对 Formula Error 单元格可能抛异常
- **文件：** `OfficeParser.cs:178-180`
- **建议：** 添加 `CellType.Error` 处理。

### M8. CsvParser.CsvConfig 从 context 获取后直接修改
- **文件：** `CsvParser.cs:24-29`
- **问题：** `config.Delimiter = '\t'` 直接修改共享实例。
- **建议：** 创建防御性副本。

### M9. ArchiveParser 无条目数量限制
- **文件：** `ArchiveParser.cs:55-97`
- **建议：** 循环中检查 `entryCount` 是否超过 `limits.MaxEmbeddedCount`。

### M10. ArchiveParser 未验证 entry.Key 路径穿越
- **文件：** `ArchiveParser.cs:79`
- **建议：** 存入元数据前规范化 `entry.Key`。

### M11. OptimaizeLanguageDetector O(n*m) 词匹配性能
- **文件：** `LanguageDetect/OptimaizeLanguageDetector.cs:196`
- **建议：** 将 `words` 转为 `HashSet<string>` 再匹配。

### M12. HtmlParser / JsonParser / RtfParser 整个文件加载到内存
- **文件：** `HtmlParser.cs:33`, `JsonParser.cs:29`, `RtfParser.cs:28`
- **建议：** 大文件场景考虑流式处理。

### M13. OdfParser 可能产生重复文本
- **文件：** `OpenDocumentParser.cs:56-67`
- **问题：** 直接文本节点和 `text:span` 后代文本独立拼接，可能重复。
- **建议：** 使用单一遍历收集叶节点文本。

---

## 五、Low 级别问题

| # | 文件 | 问题 |
|---|------|------|
| L1 | `ContentHandlerDecorator.cs:8` | `Handler` 是 protected 可变字段，应为 readonly |
| L2 | `NameDetector.cs:110-113` | 裸 catch 吞没所有异常 |
| L3 | `TextDetector.cs:28-30` | 裸 catch 吞没所有异常 |
| L4 | `TemporaryResources.cs:31-52` | 资源清理吞没所有异常 |
| L5 | `EmbeddedLimits.cs:19-22` | XOR 哈希对交换值产生相同哈希 |
| L6 | `Property.cs:10` | 静态注册表单调增长，长期运行有内存泄漏风险 |
| L7 | `CompositeEncodingDetector.cs:41` | HIGH 置信度提前退出未恢复 stream.Position |

---

## 六、测试覆盖评估

### 总体情况

| 测试项目 | 测试数 | 覆盖评价 |
|---------|-------|---------|
| Core.Test | 97 | 框架类覆盖尚可，Tika.cs 入口仅 2 个测试 |
| EncodingDetectors.Test | 13 | 基本覆盖，弱断言 |
| LanguageDetect.Test | 10 | 覆盖 8 种语言，缺少边界场景 |
| Text.Test | 7 | 基本覆盖 |
| Xml.Test | 6 | 缺少畸形 XML、命名空间测试 |
| Csv.Test | 6 | 缺少不同分隔符、转义引号测试 |
| Html.Test | 6 | 缺少畸形 HTML、脚本/样式排除测试 |
| Pdf.Test | 4 | 缺少多页、加密、损坏 PDF 测试 |
| Office.Test | 4 | 缺少密码保护、多 Sheet 测试 |
| PowerPoint.Test | 3 | 仅基础测试，缺少备注/表格测试 |
| Data.Test | 8 | 缺少畸形输入、深层嵌套测试 |
| Email.Test | 3 | 无附件、无 HTML body、无头部提取测试 |
| Image.Test | 4 | 仅测试 PNG/BMP，6 种格式中 4 种未测试 |
| Media.Test | 2 | **几乎未测试** — 无实际媒体文件解析测试 |
| Archive.Test | 4 | 仅测试 ZIP，5 种格式中 4 种未测试 |
| Odf.Test | 4 | 仅测试 ODT，3 种格式中 2 种未测试 |
| Rtf.Test | 4 | 缺少特殊字符、Unicode 转义测试 |

### 关键覆盖缺口

1. **Tika.cs 入口类** — `ToText`、`ToHtml`、`ToMarkdown`、文件重载、语言检测集成均无测试
2. **SecureContentHandler** — 压缩比检测、深度限制（安全关键）零覆盖
3. **MediaParser** — 实际解析测试为零
4. **AutoDetectParser** — 零字节文件检测、安全 handler 包装无测试
5. **所有 Parser 的错误路径** — 仅 OpenDocumentParser 测试了无效文件

### 测试反模式

- **"不抛异常"反模式** — 多个测试仅用 `act.Should().NotThrow()` 作为主要断言
- **无参数化测试** — 全部使用 `[Fact]`，无 `[Theory]` 多输入测试
- **弱断言** — `result.Should().NotBeNull()` 未验证实际内容
- **无集成测试** — 无端到端 `Tika.Parse()` 真实文件测试
- **无并发测试** — Tika 常用于多线程场景但无相关测试

---

## 七、优先修复建议

### P0 — 立即修复（安全漏洞）

1. **C5 OdfParser XXE** — 使用安全 XmlReaderSettings
2. **C1/C3 解压炸弹保护** — ArchiveParser 和 EmlParser 添加大小限制
3. **C6 嵌入深度限制执行** — PushDepth 中检查 MaxEmbeddedDepth
4. **C7 ToHtml/ToMarkdown 输出限制** — 应用 maxStringLength

### P1 — 尽快修复（正确性）

5. **H7/H8 RTF Unicode 负值和十六进制处理** — 修复字符转换
6. **C4 OfficeParser 输出损坏** — 临时缓冲区或状态回滚
7. **H2 嵌套 suppress 元素 bug** — 改为 int 计数器
8. **H5 裸 catch 块** — 替换为特定异常捕获

### P2 — 计划修复（健壮性）

9. **H9/H10 Markdown/HTML 注入和转义**
10. **M1-M4 线程安全** — 文档说明或使用并发集合
11. **M5-M6 ContentHandlerFactory / XHTMLContentHandler 设计问题**
12. **补充测试** — 优先 Tika.cs、SecureContentHandler、MediaParser、EmailParser

---

## 八、架构评价

**优点：**
- 清晰的 IParser/IDetector/IContentHandler 接口抽象
- ParseContext 类型化对象袋设计灵活
- 分包架构允许按需引用
- SAX 事件流模型保持与 Java Tika 的一致性
- 编码检测和语言检测的组合策略设计合理

**改进建议：**
- 考虑引入 `IAsyncParser` 支持大文件异步流式处理
- 解析管道缺少统一的错误收集机制（当前各 parser 独立处理异常）
- 元数据传播在嵌入文档解析中可能丢失（解析器创建新 Metadata 实例）
- 考虑添加解析进度回调机制用于大文件场景
