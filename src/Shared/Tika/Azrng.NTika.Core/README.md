# Azrng.NTika — Apache Tika .NET 移植版

### 迁移来源

本项目从 [Apache Tika](https://tika.apache.org/) (Java) 移植而来，目标是在 .NET 生态中提供类似的文档解析和内容提取能力。

### 已迁移功能

**核心框架：**
- `IParser` / `AutoDetectParser` / `CompositeParser` — 解析器架构
- `IContentHandler` / SAX 事件流模型
- `ParseContext` — 类型化上下文对象
- `Metadata` — 多值元数据字典
- `IDetector` / `CompositeDetector` / `MagicDetector` — MIME 类型检测
- `IEncodingDetector` / `ILanguageDetector` — 编码和语言检测
- `IEmbeddedDocumentExtractor` — 递归嵌入文档解析
- `ContentHandlerFactory` — 支持 Text / HTML / Markdown 输出

**支持格式（30+ 种）：**

| 类别 | 格式 | 包名 |
|------|------|------|
| 纯文本 | txt, log | `Azrng.NTika.Parsers.Text` |
| 标记语言 | html, xhtml, xml | `Azrng.NTika.Parsers.Html`, `Azrng.NTika.Parsers.Xml` |
| 表格数据 | csv, tsv | `Azrng.NTika.Parsers.Csv` |
| 数据格式 | json, yaml | `Azrng.NTika.Parsers.Data` |
| PDF | pdf | `Azrng.NTika.Parsers.Pdf` |
| Office | docx, xlsx, xls | `Azrng.NTika.Parsers.Office` |
| 演示文稿 | pptx | `Azrng.NTika.Parsers.PowerPoint` |
| ODF | odt, ods, odp | `Azrng.NTika.Parsers.Odf` |
| 富文本 | rtf | `Azrng.NTika.Parsers.Rtf` |
| 邮件 | eml | `Azrng.NTika.Parsers.Email` |
| 图片 | png, jpeg, gif, bmp, tiff | `Azrng.NTika.Parsers.Image` |
| 音视频 | mp3, wav, mp4, m4a | `Azrng.NTika.Parsers.Media` |
| 压缩包 | zip, tar, gz, 7z, rar | `Azrng.NTika.Parsers.Archive` |

**编码检测（`Azrng.NTika.EncodingDetectors`）：**
- BOM 检测（UTF-8/16/32）
- 通用编码检测（基于 UTF.Unknown）
- HTML meta charset 检测
- 组合检测器（按置信度排序）

**语言检测（`Azrng.NTika.LanguageDetect`）：**
- 基于 Unicode 范围 + 词频分析
- 支持：en, zh, ja, ko, de, fr, es, pt, ru

### 使用方式

**基本用法：**

```csharp
using Azrng.NTika.Core;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

// 提取纯文本
var tika = new Tika();
string text = tika.ToText("document.pdf");

// 提取 Markdown
string markdown = tika.ToMarkdown("document.docx");

// 提取 HTML
string html = tika.ToHtml("page.html");

// 获取元数据
Metadata metadata = tika.Parse(new FileInfo("report.xlsx"));
string title = metadata.Get(TikaCoreProperties.TITLE);
string creator = metadata.Get(TikaCoreProperties.CREATOR);
string content = metadata.Get("X-TIKA:content");
```

**带编码和语言检测：**

```csharp
using Azrng.NTika.EncodingDetectors;
using Azrng.NTika.LanguageDetect;

var encodingDetector = new CompositeEncodingDetector();
var languageDetector = new OptimaizeLanguageDetector();

var tika = new Tika(new DefaultDetector(), new AutoDetectParser(),
    encodingDetector, languageDetector);

Metadata metadata = tika.Parse("legacy-file.txt");
string detectedLang = metadata.Get(TikaCoreProperties.LANGUAGE);
```

**自定义解析器组合：**

```csharp
using Azrng.NTika.Parsers.Text;
using Azrng.NTika.Parsers.Html;
using Azrng.NTika.Parsers.Pdf;

var tika = new Tika(
    new TextParser(),
    new HtmlParser(),
    new PdfParser()
);
string text = tika.ToText("document.pdf");
```

**启用嵌入文档递归解析：**

```csharp
using Azrng.NTika.Core.Config;

var context = new ParseContext();
context.Set(new EmbeddedLimits
{
    MaxEmbeddedDepth = 5,
    MaxEmbeddedCount = 100
});
```

**检测文件类型：**

```csharp
var tika = new Tika();
string mimeType = tika.Detect("unknown-file"); // 返回 MIME 类型字符串
```

### 包依赖关系

```
Azrng.NTika.Core (核心框架，无外部依赖)
├── Azrng.NTika.EncodingDetectors (依赖: UTF.Unknown, System.Text.Encoding.CodePages)
├── Azrng.NTika.LanguageDetect (无外部依赖)
├── Azrng.NTika.Parsers.Text
├── Azrng.NTika.Parsers.Html
├── Azrng.NTika.Parsers.Xml
├── Azrng.NTika.Parsers.Csv
├── Azrng.NTika.Parsers.Data (依赖: YamlDotNet)
├── Azrng.NTika.Parsers.Pdf (依赖: PdfPig)
├── Azrng.NTika.Parsers.Office (依赖: NPOI)
├── Azrng.NTika.Parsers.PowerPoint (依赖: DocumentFormat.OpenXml)
├── Azrng.NTika.Parsers.Odf (无外部依赖，使用 System.IO.Compression)
├── Azrng.NTika.Parsers.Rtf (无外部依赖)
├── Azrng.NTika.Parsers.Email (依赖: MimeKit)
├── Azrng.NTika.Parsers.Image (依赖: MetadataExtractor)
├── Azrng.NTika.Parsers.Media (依赖: MetadataExtractor)
└── Azrng.NTika.Parsers.Archive (依赖: SharpCompress)
```

### 目标框架

- `net8.0`
- `net10.0`