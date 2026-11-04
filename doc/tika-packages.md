# Azrng.NTika 包清单

> 来源：从 [Apache Tika](https://tika.apache.org/) (Java) 移植到 .NET 的文档解析套件，位于 `src/Shared/Tika/`，共 17 个包，目标框架 `net8.0` / `net10.0`。迁移方案背景见 [tika-dotnet-migration-plan.md](tika-dotnet-migration-plan.md)，核心包完整用法见 [Azrng.NTika.Core/README.md](../src/Shared/Tika/Azrng.NTika.Core/README.md)。

## 定位

所有包做的是「文档 → 文本 / Markdown / HTML / 元数据」的**解析与提取**，不是文件格式互转（不做 docx→pdf 这类转换）。输出端只有 Text / HTML / Markdown 三种 content handler，典型场景是全文索引、RAG 语料抽取、文件预览。

## 包一览

### 核心与检测（3 个）

| 包 | 职责 | 外部依赖 |
|---|---|---|
| `Azrng.NTika.Core` | 核心框架：`Tika` 门面（`ToText` / `ToMarkdown` / `ToHtml` / `Parse` / `Detect`）、`IParser` / `AutoDetectParser` / `CompositeParser` 解析架构、`IContentHandler` SAX 事件流、`Metadata` / `ParseContext`、MIME 类型检测（`MagicDetector` / `NameDetector` / `TextDetector` / `CompositeDetector`）、嵌入文档递归解析、限额配置（`EmbeddedLimits` / `TimeoutLimits` / `OutputLimits`）与完整异常体系 | 无 |
| `Azrng.NTika.EncodingDetectors` | 字符编码检测：BOM（UTF-8/16/32）、通用编码检测（UTF.Unknown）、HTML meta charset、按置信度排序的组合检测器 | UTF.Unknown 2.5.1, System.Text.Encoding.CodePages 8.0.0 |
| `Azrng.NTika.LanguageDetect` | 语言检测：Unicode 范围 + 词频分析（Optimaize 算法），支持 en / zh / ja / ko / de / fr / es / pt / ru | 无 |

### 格式解析器（14 个）

每个包实现 Core 的 `IParser`，按 MIME 类型注册进 `AutoDetectParser`，只负责自己那类格式的正文与元数据提取。

| 包 | 提取内容 | 支持格式（MIME 实测） | 外部依赖 |
|---|---|---|---|
| `Azrng.NTika.Parsers.Text` | 正文 | txt（`text/plain`） | 无 |
| `Azrng.NTika.Parsers.Html` | 正文 + meta 元数据 | html, xhtml | AngleSharp 1.2.0 |
| `Azrng.NTika.Parsers.Xml` | 正文 | xml, svg（`application/xml`, `image/svg+xml`） | 无 |
| `Azrng.NTika.Parsers.Csv` | 正文 | csv, tsv | Sylvan.Data.Csv 1.4.0 |
| `Azrng.NTika.Parsers.Data` | 正文 | json, yaml | YamlDotNet 18.0.0 |
| `Azrng.NTika.Parsers.Pdf` | 正文 + 元数据 | pdf | PdfPig 0.1.14 |
| `Azrng.NTika.Parsers.Office` | 正文 + 元数据 | docx, xlsx, xls（不含 ppt，pptx 见下行；含 legacy xls） | NPOI 2.7.4 |
| `Azrng.NTika.Parsers.PowerPoint` | 正文 | pptx | DocumentFormat.OpenXml 3.5.1 |
| `Azrng.NTika.Parsers.Odf` | 正文 | odt, ods, odp | 无（System.IO.Compression） |
| `Azrng.NTika.Parsers.Rtf` | 正文 | rtf | System.Text.Encoding.CodePages 8.0.0 |
| `Azrng.NTika.Parsers.Email` | 正文 + 附件递归解析 | eml（`message/rfc822`） | MimeKit 4.17.0, MsgReader 6.0.11 |
| `Azrng.NTika.Parsers.Image` | 仅元数据（EXIF 等，无正文） | png, jpeg, gif, bmp, tiff, webp | MetadataExtractor 2.9.3 |
| `Azrng.NTika.Parsers.Media` | 仅元数据（时长、音轨等） | mp3, wav, mp4, m4a（`audio/mpeg`, `audio/vnd.wave`, `audio/mp4`, `video/mp4`） | MetadataExtractor 2.9.3 |
| `Azrng.NTika.Parsers.Archive` | 包内条目列表 + 成员递归解析 | zip, tar, gz, 7z, rar | SharpCompress 1.0.0 |

## 组合关系

```
Azrng.NTika.Core（唯一基座，无外部依赖）
├── Azrng.NTika.EncodingDetectors ──┐
├── Azrng.NTika.LanguageDetect      ├─ 检测增强，注入 Tika 构造函数
├── 14 × Azrng.NTika.Parsers.*      ┘   注册进 AutoDetectParser
```

按需引用：只用 PDF 就只装 `Core` + `Parsers.Pdf`，各解析器互不依赖，避免拖入无关重型依赖（NPOI、OpenXml 等）。

## 备注

- `Email` 包已引用 `MsgReader` 依赖，但当前 `EmlParser` 代码只处理 `message/rfc822`（eml），`.msg` 尚未接入解析路径，属 csproj 描述与实现的已知偏差。
- 各包公开用法统一走 `Core` 的 `Tika` 门面，示例见 Core README；本清单只回答「每个包是做什么的」，不重复用法说明。
