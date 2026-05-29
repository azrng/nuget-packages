# Apache Tika -> .NET 类库 转换方案

## 一、项目概况

| 维度 | 数据 |
|---|---|
| Java 源文件 | 2,049 个 |
| 类 | 2,319 个 |
| 接口 | 136 个 |
| 方法 | 15,766 个 |
| Maven 子模块 | ~30 个 |
| 核心架构 | `Parser` 接口 + `AutoDetectParser` + `CompositeParser` + SAX 事件流 |

## 二、核心技术映射

| Java 概念 | .NET 对应方案 |
|---|---|
| `Parser` 接口 (`SAX ContentHandler`) | `IParser` 接口，输出改为 `XDocument` / `XmlReader` / 流式回调 |
| `Metadata` (多值 Map) | `Metadata : Dictionary<string, string[]>` |
| `ParseContext` (类型容器) | `ParseContext` 用 `Dictionary<Type, object>` 实现 |
| `MediaType` / `MediaTypeRegistry` | `MediaType` 类 + 静态注册表 |
| `Detector` (MIME 检测) | `IDetector` 接口，可用 [MimeDetective](https://github.com/MediatedCommunications/MimeDetective) 等 NuGet 包替代 |
| `TikaInputStream` (可回溯流) | 封装 `Stream`，用 `MemoryStream` 或临时文件实现 Seek |
| Java SPI (`ServiceLoader`) | .NET `IServiceCollection` + DI 或运行时程序集扫描 |
| Maven 多模块 | .NET Solution 多项目 (`*.csproj`) |
| JUnit 测试 | xUnit / NUnit |
| SAX `ContentHandler` | 自定义 `IContentHandler` 或直接输出 `XDocument` |

## 三、建议的 .NET 项目结构

```
Tika.Net.sln
|
|-- src/
|   |-- Tika.Net.Core/                    <-- 对应 tika-core (312 文件)
|   |   |-- Abstractions/
|   |   |   |-- IParser.cs
|   |   |   |-- IDetector.cs
|   |   |   |-- IContentHandler.cs
|   |   |   +-- ITranslator.cs
|   |   |-- Model/
|   |   |   |-- Metadata.cs
|   |   |   |-- MediaType.cs
|   |   |   |-- ParseContext.cs
|   |   |   +-- TikaInputStream.cs
|   |   |-- Parser/
|   |   |   |-- AutoDetectParser.cs
|   |   |   |-- CompositeParser.cs
|   |   |   +-- AbstractParser.cs
|   |   |-- Detector/
|   |   |   |-- DefaultDetector.cs
|   |   |   +-- MimeTypes.cs
|   |   |-- Config/
|   |   |   +-- TikaConfig.cs
|   |   +-- Tika.cs                       <-- 门面类
|   |
|   |-- Tika.Net.Parsers.Standard/        <-- 对应 tika-parsers-standard (743 文件)
|   |   |-- Office/
|   |   |-- Pdf/
|   |   |-- Html/
|   |   |-- Xml/
|   |   |-- Image/
|   |   |-- Audio/
|   |   |-- Video/
|   |   |-- Archive/
|   |   |-- Text/
|   |   +-- ...
|   |
|   |-- Tika.Net.Detectors/               <-- 对应 tika-detectors
|   |-- Tika.Net.EncodingDetectors/       <-- 对应 tika-encoding-detectors
|   |-- Tika.Net.LanguageDetect/          <-- 对应 tika-langdetect
|   |-- Tika.Net.Translate/               <-- 对应 tika-translate
|   +-- Tika.Net.Serialization/           <-- 对应 tika-serialization
|
+-- test/
    |-- Tika.Net.Core.Tests/
    |-- Tika.Net.Parsers.Standard.Tests/
    +-- Tika.Net.IntegrationTests/
```

## 四、核心接口设计（示例）

```csharp
// 核心解析接口 - 替代 Java 的 Parser
public interface IParser
{
    ISet<MediaType> GetSupportedTypes(ParseContext context);
    void Parse(Stream stream, IContentHandler handler,
               Metadata metadata, ParseContext context);
}

// 简化门面类
public class Tika
{
    public string Detect(Stream stream) { ... }
    public string Detect(string filePath) { ... }
    public Metadata Parse(Stream stream) { ... }
    public string ToText(Stream stream) { ... }
}

// 用法示例
var tika = new Tika();
string text = tika.ToText(File.OpenRead("report.pdf"));
Metadata meta = tika.Parse(File.OpenRead("report.pdf"));
```

## 五、分阶段实施路线

### 第 1 阶段：Core 基础（2-3 周）

- 搭建 .NET Solution 骨架
- 移植 `Tika.Net.Core`：`IParser`、`IDetector`、`Metadata`、`MediaType`、`ParseContext`、`TikaConfig`
- 实现 `AutoDetectParser` 框架（MIME 检测 + 解析器路由）
- 基于 NuGet 包实现基础 MIME 检测

### 第 2 阶段：核心 Parser 移植（4-6 周）

按优先级移植常用 Parser：

1. **PDF** --> 使用 [PdfPig](https://github.com/UglyToad/PdfPig) 或 [iTextSharp](https://github.com/itext/itext-dotnet)
2. **Office (docx/xlsx/pptx)** --> 使用 [Open XML SDK](https://github.com/dotnet/Open-XML-SDK) 或 [NPOI](https://github.com/nissl-lab/npoi)
3. **HTML** --> 使用 [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack) 或 [AngleSharp](https://github.com/AngleSharp/AngleSharp)
4. **XML** --> 使用 `System.Xml`
5. **纯文本/CSV** --> 纯 .NET 实现
6. **图片 (OCR)** --> 使用 [Tesseract OCR .NET](https://github.com/charlesw/tesseract)

### 第 3 阶段：扩展功能（3-4 周）

- 语言检测 --> 使用 [fastText](https://github.com/facebookresearch/fastText) .NET 绑定或自行实现
- 编码检测 --> 使用 `System.Text.Encoding.CodePages`
- 提取元数据增强
- 流式解析优化

### 第 4 阶段：高级特性（2-3 周）

- 嵌入文档递归解析
- 自定义 Parser 插件机制
- 性能优化 & 基准测试
- NuGet 包发布

## 六、关键风险与应对

| 风险 | 影响 | 应对 |
|---|---|---|
| SAX 事件模型在 .NET 中无直接对应 | 所有 Parser 的输出层 | 定义 `IContentHandler` 适配层，或将输出统一为 `XDocument` |
| Java 三方库无 .NET 对应 (如 Apache POI) | Office 文档解析 | 优先选择 Open XML SDK + NPOI，部分格式可能需放弃或降级 |
| 89+ Parser 实现工作量巨大 | 时间成本 | 分优先级，先覆盖常用格式 (PDF/Office/HTML/图片)，其余逐步补充 |
| Java SPI 动态加载 vs .NET DI | 插件扩展性 | 用程序集扫描 + DI 容器实现自动注册 |
| Tika 的 MIME 魔数检测数据库 | 文件类型识别 | 移植 `tika-core/src/main/resources` 中的 MIME 规则文件，或用 NuGet 替代 |

## 七、已有的 .NET 替代方案参考

在动手之前值得评估：

| 项目 | 说明 |
|---|---|
| **[TikaOnDotnet](https://github.com/KevM/tikaondotnet)** | 通过 IKVM 把 Tika JAR 转成 .NET 直接调用，最省力但体积大 |
| **[TikaCore](https://github.com/nickvdyck/tika-core)** | 轻量级 Tika .NET 移植，功能有限 |
| **[Apache Tika REST Server](https://tika.apache.org/)** | 不移植代码，部署 Tika Server，通过 HTTP API 调用 |

## 八、建议

- **如果目标是完全自主的 .NET 原生库**：按上述分阶段方案执行，优先 Core + Top 10 Parser。
- **如果目标是快速可用**：优先考虑 TikaOnDotnet（IKVM 封装）或部署 Tika Server + .NET Client 的方案，工作量可降低 80%。
- **推荐混合策略**：先用 Tika Server 方案满足短期需求，同时并行推进原生 .NET 移植作为长期方案。
