# Azrng.Office.NPOI 扩展建议

## 📊 任务进度概览

| 状态 | 数量 | 占比 |
|------|------|------|
| ✅ 已完成 | 2 | 9.5% |
| 🔄 进行中 | 0 | 0% |
| ⏳ 待开发 | 19 | 90.5% |
| 📦 总计 | 21 | 100% |

### 已完成功能
1. ✅ **导入功能（Import）** - 2026-03-08 完成
2. ✅ **CellExtensions 扩展** - 2026-03-08 完成

### 下一步计划
- 🎯 **高优先级**: 模板功能、数据验证、条件格式
- 📋 **中优先级**: SheetExtension 实现、图表功能、数据透视表

---

## 📋 概述

本文档列出了 `Azrng.Office.NPOI` 库可以添加的新功能和改进方向，按优先级分类。

---

## 🔴 高优先级扩展（立即添加）

### 1. **导入功能（Import）** ⭐⭐⭐⭐⭐ ✅ **已完成**
**当前状态**: 库只支持导出，不支持导入
**实现日期**: 2026-03-08

**已实现的方法**:
```csharp
public static class ExcelImporter
{
    /// <summary>
    /// 从 Excel 文件导入数据到 List
    /// </summary>
    public static List<T> ImportFromFile<T>(string filePath, int sheetIndex = 0, bool hasHeader = true, int startRow = 0); ✅

    /// <summary>
    /// 从 Stream 导入数据
    /// </summary>
    public static List<T> ImportFromStream<T>(Stream stream, int sheetIndex = 0, bool hasHeader = true, int startRow = 0); ✅

    /// <summary>
    /// 从字节数组导入数据
    /// </summary>
    public static List<T> ImportFromBytes<T>(byte[] data, int sheetIndex = 0, bool hasHeader = true, int startRow = 0); ✅

    /// <summary>
    /// 异步导入文件
    /// </summary>
    public static Task<List<T>> ImportFromFileAsync<T>(string filePath, int sheetIndex = 0, bool hasHeader = true, int startRow = 0, CancellationToken cancellationToken = default); ✅

    // ✅ 额外添加：导入结果类（包含错误信息）
    public class ExcelImportResult<T>
    {
        public List<T> Data { get; set; }
        public List<ImportError> Errors { get; set; }
        public bool HasErrors => Errors.Any();
        public int SuccessCount => Data.Count;
        public int ErrorCount => Errors.Count;
    }

    // ✅ 额外添加：导入错误信息类
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string? ColumnName { get; set; }
        public string ErrorMessage { get; set; }
        public string? OriginalValue { get; set; }
    }
}
```

**已实现的特性**:
- ✅ 自动类型转换（字符串 → DateTime, decimal, int, long, double, float, bool, Guid 等）
- ✅ 表头自动映射（支持中文列名）
- ✅ 支持可空类型（Nullable<T>）
- ✅ 反射缓存（性能优化）
- ✅ 自动跳过空行
- ✅ 路径和参数验证
- ✅ 异步支持
- ✅ 错误处理和日志

**使用示例**:
```csharp
// 简单导入
var employees = ExcelImporter.ImportFromFile<Employee>("employees.xlsx");

// 指定工作表索引
var data = ExcelImporter.ImportFromFile<MyData>("data.xlsx", sheetIndex: 1);

// 从字节数组导入
var bytes = File.ReadAllBytes("data.xlsx");
var items = ExcelImporter.ImportFromBytes<MyData>(bytes);

// 异步导入大文件
var largeData = await ExcelImporter.ImportFromFileAsync<LargeData>("bigfile.xlsx");
```

**实现位置**: `ExcelImporter.cs` (新建)

---

### 2. **模板功能（Template）** ⭐⭐⭐⭐⭐
**当前状态**: 每次都要从头创建 Excel

**建议添加的方法**:
```csharp
public static class ExcelTemplateHelper
{
    /// <summary>
    /// 从模板文件加载工作簿
    /// </summary>
    public static WorkbookWrapper LoadTemplate(string templatePath);

    /// <summary>
    /// 从模板字节数组加载
    /// </summary>
    public static WorkbookWrapper LoadTemplate(byte[] templateBytes);

    /// <summary>
    /// 填充模板数据（保留格式和公式）
    /// </summary>
    public static WorkbookWrapper FillTemplate<T>(string templatePath, List<T> data, string sheetName = null);

    /// <summary>
    /// 基于命名区域填充数据
    /// </summary>
    public static void FillNamedRange<T>(this WorkbookWrapper workbook, string rangeName, List<T> data);
}
```

**使用场景**:
- 月度报表（固定格式）
- 财务报表（带公式）
- 发票模板
- 合同模板

---

### 3. **数据验证（Data Validation）** ⭐⭐⭐⭐
**当前状态**: 无数据验证功能

**建议添加**:
```csharp
public static class DataValidationExtensions
{
    /// <summary>
    /// 添加下拉列表验证
    /// </summary>
    public static ISheet AddDropdownList(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, string[] options);

    /// <summary>
    /// 添加数字范围验证
    /// </summary>
    public static ISheet AddNumberValidation(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, decimal min, decimal max);

    /// <summary>
    /// 添加日期范围验证
    /// </summary>
    public static ISheet AddDateValidation(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, DateTime min, DateTime max);

    /// <summary>
    /// 添加文本长度验证
    /// </summary>
    public static ISheet AddTextLengthValidation(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, int minLength, int maxLength);

    /// <summary>
    /// 添加自定义公式验证
    /// </summary>
    public static ISheet AddFormulaValidation(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, string formula);
}
```

---

### 4. **条件格式（Conditional Formatting）** ⭐⭐⭐⭐
**建议添加**:
```csharp
public static class ConditionalFormattingExtensions
{
    /// <summary>
    /// 添加数据条（颜色渐变）
    /// </summary>
    public static ISheet AddDataBar(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol);

    /// <summary>
    /// 添加色阶（热力图）
    /// </summary>
    public static ISheet AddColorScale(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol);

    /// <summary>
    /// 添加图标集（向上/向下箭头）
    /// </summary>
    public static ISheet AddIconSet(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol);

    /// <summary>
    /// 添加高亮单元格规则（大于、小于、等于等）
    /// </summary>
    public static ISheet AddHighlightCells(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, ConditionalOperator op, string formula);

    /// <summary>
    /// 添加前/后 N 项规则
    /// </summary>
    public static ISheet AddTopBottomRule(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, int rank, bool isTop = true);

    /// <summary>
    /// 添加重复值/唯一值规则
    /// </summary>
    public static ISheet AddDuplicateUniqueRule(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, bool isDuplicate = true);
}
```

---

## 🟡 中优先级扩展（短期规划）

### 5. **图表功能（Charts）** ⭐⭐⭐
**建议添加**:
```csharp
public static class ChartExtensions
{
    /// <summary>
    /// 创建柱状图
    /// </summary>
    public static ISheet AddBarChart(this ISheet sheet, string chartTitle, string dataRange, string categoryAxisRange, bool isVertical = true);

    /// <summary>
    /// 创建折线图
    /// </summary>
    public static ISheet AddLineChart(this ISheet sheet, string chartTitle, string dataRange, string categoryAxisRange);

    /// <summary>
    /// 创建饼图
    /// </summary>
    public static ISheet AddPieChart(this ISheet sheet, string chartTitle, string dataRange, string categoryRange);

    /// <summary>
    /// 创建散点图
    /// </summary>
    public static ISheet AddScatterChart(this ISheet sheet, string chartTitle, string xRange, string yRange);

    /// <summary>
    /// 创建组合图
    /// </summary>
    public static ISheet AddCombinationChart(this ISheet sheet, string chartTitle, Dictionary<string, ChartType> series);
}
```

---

### 6. **数据透视表（Pivot Table）** ⭐⭐⭐
**建议添加**:
```csharp
public static class PivotTableExtensions
{
    /// <summary>
    /// 创建数据透视表
    /// </summary>
    public static ISheet AddPivotTable(this ISheet sourceSheet, string pivotTableName, string sourceRange, string targetSheetName);

    /// <summary>
    /// 添加行字段
    /// </summary>
    public static IPivotTable AddRowField(this IPivotTable pivotTable, string columnName, int position = 0);

    /// <summary>
    /// 添加列字段
    /// </summary>
    public static IPivotTable AddColumnField(this IPivotTable pivotTable, string columnName, int position = 0);

    /// <summary>
    /// 添加数据字段（值字段）
    /// </summary>
    public static IPivotTable AddDataField(this IPivotTable pivotTable, string columnName, DataConsolidateFunction function = DataConsolidateFunction.Sum);

    /// <summary>
    /// 添加筛选字段
    /// </summary>
    public static IPivotTable AddFilterField(this IPivotTable pivotTable, string columnName);
}
```

---

### 7. **公式支持（Formula）** ⭐⭐⭐
**建议添加**:
```csharp
public static class FormulaHelper
{
    /// <summary>
    /// 设置单元格公式
    /// </summary>
    public static ICell SetFormula(this ICell cell, string formula);

    /// <summary>
    /// 创建求和公式
    /// </summary>
    public static string CreateSumFormula(string startCell, string endCell);

    /// <summary>
    /// 创建平均值公式
    /// </summary>
    public static string CreateAverageFormula(string startCell, string endCell);

    /// <summary>
    /// 创建计数公式
    /// </summary>
    public static string CreateCountFormula(string startCell, string endCell);

    /// <summary>
    /// 创建 VLOOKUP 公式
    /// </summary>
    public static string CreateVLookupFormula(string lookupValue, string tableRange, int columnIndex, bool exactMatch = true);

    /// <summary>
    /// 创建 IF 公式
    /// </summary>
    public static string CreateIfFormula(string condition, string trueValue, string falseValue);

    /// <summary>
    /// 重新计算所有公式
    /// </summary>
    public static void RecalculateFormulas(this IWorkbook workbook);
}
```

---

### 8. **SheetExtension 实现** ⭐⭐⭐⭐
**当前状态**: `SheetExtension.cs` 文件是空的

**建议添加**:
```csharp
public static class SheetExtension
{
    /// <summary>
    /// 复制工作表
    /// </summary>
    public static ISheet CopyTo(this ISheet sourceSheet, IWorkbook targetWorkbook, string newSheetName);

    /// <summary>
    /// 获取工作表使用的行数
    /// </summary>
    public static int GetPhysicalNumberOfRows(this ISheet sheet);

    /// <summary>
    /// 获取工作表使用的列数
    /// </summary>
    public static int GetPhysicalNumberOfColumns(this ISheet sheet);

    /// <summary>
    /// 删除空行
    /// </summary>
    public static ISheet RemoveEmptyRows(this ISheet sheet);

    /// <summary>
    /// 删除指定行
    /// </summary>
    public static ISheet RemoveRows(this ISheet sheet, int startIndex, int count);

    /// <summary>
    /// 插入行
    /// </summary>
    public static ISheet InsertRows(this ISheet sheet, int startIndex, int count);

    /// <summary>
    /// 冻结窗格
    /// </summary>
    public static ISheet FreezePane(this ISheet sheet, int colSplit, int rowSplit);

    /// <summary>
    /// 设置打印区域
    /// </summary>
    public static ISheet SetPrintArea(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol);

    /// <summary>
    /// 设置为适合一页打印
    /// </summary>
    public static ISheet FitToPage(this ISheet sheet);

    /// <summary>
    /// 保护工作表
    /// </summary>
    public static ISheet Protect(this ISheet sheet, string password, bool enableLocking = true);

    /// <summary>
    /// 获取所有合并区域
    /// </summary>
    public static List<CellRangeAddress> GetMergedRegions(this ISheet sheet);

    /// <summary>
    /// 清除所有合并区域
    /// </summary>
    public static ISheet ClearMergedRegions(this ISheet sheet);
}
```

---

### 9. **CellExtensions 扩展** ⭐⭐⭐ ✅ **已完成**
**实现日期**: 2026-03-08

**已实现的方法**:
```csharp
public static class CellExtensions
{
    /// <summary>
    /// 设置单元格值（支持多种类型）
    /// </summary>
    public static ICell SetValue<T>(this ICell cell, T value); ✅

    /// <summary>
    /// 设置超链接
    /// </summary>
    public static ICell SetHyperlink(this ICell cell, string url, HyperlinkType type = HyperlinkType.Url); ✅

    /// <summary>
    /// 设置单元格注释
    /// </summary>
    public static ICell SetComment(this ICell cell, string comment, string author = "System"); ✅

    /// <summary>
    /// 设置为日期格式
    /// </summary>
    public static ICell SetDateFormat(this ICell cell, string format = "yyyy-MM-dd"); ✅

    /// <summary>
    /// 设置为货币格式
    /// </summary>
    public static ICell SetCurrencyFormat(this ICell cell, string currencySymbol = "¥"); ✅

    /// <summary>
    /// 设置为百分比格式
    /// </summary>
    public static ICell SetPercentFormat(this ICell cell, int decimals = 2); ✅

    /// <summary>
    /// 克隆单元格
    /// </summary>
    public static ICell Clone(this ICell sourceCell, ICell targetCell, bool copyStyle = true); ✅

    /// <summary>
    /// 检查单元格是否在合并区域内
    /// </summary>
    public static bool IsMerged(this ICell cell); ✅

    /// <summary>
    /// 获取合并区域的范围
    /// </summary>
    public static CellRangeAddress? GetMergedRegion(this ICell cell); ✅ (额外添加)

    /// <summary>
    /// 设置单元格背景色
    /// </summary>
    public static ICell SetBackgroundColor(this ICell cell, short colorIndex); ✅ (额外添加)

    /// <summary>
    /// 设置单元格字体颜色
    /// </summary>
    public static ICell SetFontColor(this ICell cell, short colorIndex); ✅ (额外添加)

    /// <summary>
    /// 设置单元格为粗体
    /// </summary>
    public static ICell SetBold(this ICell cell, bool isBold = true); ✅ (额外添加)

    /// <summary>
    /// 设置单元格字体大小
    /// </summary>
    public static ICell SetFontSize(this ICell cell, short fontSize); ✅ (额外添加)
}
```

**使用示例**:
```csharp
// 设置超链接
cell.SetHyperlink("https://example.com");

// 设置注释
cell.SetComment("这是一个重要数据");

// 设置货币格式
cell.SetCurrencyFormat("¥");

// 设置背景色
cell.SetBackgroundColor(IndexedColors.LightYellow.Index);

// 设置粗体
cell.SetBold(true);
```

---

## 🟢 低优先级扩展（长期规划）

---

## 🟢 低优先级扩展（长期规划）

### 10. **图片处理（Images）** ⭐⭐
**建议添加**:
```csharp
public static class ImageExtensions
{
    /// <summary>
    /// 插入图片
    /// </summary>
    public static ISheet AddImage(this ISheet sheet, string imagePath, int row, int col, int width, int height);

    /// <summary>
    /// 从字节数组插入图片
    /// </summary>
    public static ISheet AddImage(this ISheet sheet, byte[] imageBytes, PictureType pictureType, int row, int col, int width, int height);

    /// <summary>
    /// 插入 Logo（固定位置）
    /// </summary>
    public static ISheet AddLogo(this ISheet sheet, string logoPath);

    /// <summary>
    /// 插入背景图片
    /// </summary>
    public static ISheet SetBackgroundImage(this ISheet sheet, string imagePath);
}
```

---


---

### 12. **加密和解密** ⭐⭐
**建议添加**:
```csharp
public static class EncryptionExtensions
{
    /// <summary>
    /// 加密工作簿
    /// </summary>
    public static void Encrypt(this IWorkbook workbook, string password);

    /// <summary>
    /// 解密工作簿
    /// </summary>
    public static void Decrypt(this IWorkbook workbook, string password);

    /// <summary>
    /// 设置修改密码（只读推荐）
    /// </summary>
    public static void SetWriteProtectionPassword(this IWorkbook workbook, string password);
}
```

---


---

### 14. **多线程支持** ⭐⭐⭐
**建议添加**:
```csharp
public static class AsyncExtensions
{
    /// <summary>
    /// 异步保存到文件
    /// </summary>
    public static async Task SaveToFileAsync(this WorkbookWrapper workbook, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步导出到字节数组
    /// </summary>
    public static async Task<byte[]> ToBytesAsync(this WorkbookWrapper workbook, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步导出到流
    /// </summary>
    public static async Task<Stream> ToStreamAsync(this WorkbookWrapper workbook, CancellationToken cancellationToken = default);
}
```

---

### 15. **样式预设库** ⭐⭐
**建议添加**:
```csharp
public static class ExcelStylePresets
{
    /// <summary>
    /// 财务报表样式（专业蓝）
    /// </summary>
    public static BaseStyle Financial => new BaseStyle(isBold: false, wrapText: true, fontColor: 8, fontSize: 11, fontName: "Calibri");

    /// <summary>
    /// 标题样式（粗体居中）
    /// </summary>
    public static BaseStyle Header => new TitleCellStyle();

    /// <summary>
    /// 警告样式（红色背景）
    /// </summary>
    public static BaseStyle Warning => new BaseStyle(fillForegroundColor: 10);

    /// <summary>
    /// 成功样式（绿色背景）
    /// </summary>
    public static BaseStyle Success => new BaseStyle(fillForegroundColor: 17);

    /// <summary>
    /// 信息样式（蓝色背景）
    /// </summary>
    public static BaseStyle Info => new BaseStyle(fillForegroundColor: 12);

    /// <summary>
    /// 边框样式（全边框）
    /// </summary>
    public static BaseStyle Bordered => new BaseStyle
    {
        BorderTop = BorderStyle.Thin,
        BorderBottom = BorderStyle.Thin,
        BorderLeft = BorderStyle.Thin,
        BorderRight = BorderStyle.Thin
    };
}
```

---

## 🎯 新增特性建议

### 16. **数据转换特性**
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ExcelFormatAttribute : Attribute
{
    public string Format { get; set; }
    public ExcelFormatAttribute(string format)
    {
        Format = format;
    }
}

// 使用示例
public class Report
{
    [ColumnName("金额")]
    [ExcelFormat("#,##0.00")]  // 千分位格式
    public decimal Amount { get; set; }

    [ColumnName("完成率")]
    [ExcelFormat("0.00%")]  // 百分比格式
    public decimal CompletionRate { get; set; }
}
```

### 17. **数据验证特性**
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ExcelRequiredAttribute : Attribute
{
    public string ErrorMessage { get; set; } = "此字段为必填项";
}

[AttributeUsage(AttributeTargets.Property)]
public class ExcelRangeAttribute : Attribute
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public string ErrorMessage { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public class ExcelEmailAttribute : Attribute
{
    public string ErrorMessage { get; set; } = "请输入有效的邮箱地址";
}
```

### 18. **批量操作特性**
```csharp
public static class BatchOperations
{
    /// <summary>
    /// 合并多个 Excel 文件
    /// </summary>
    public static WorkbookWrapper MergeFiles(string[] filePaths, string outputFilePath);

    /// <summary>
    /// 拆分 Excel 文件（按行数）
    /// </summary>
    public static void SplitFile(string inputFilePath, int rowsPerFile, string outputDirectory);

    /// <summary>
    /// 批量转换格式（xls ↔ xlsx）
    /// </summary>
    public static void ConvertFormat(string inputPath, string outputPath);
}
```

---

## 📊 配置和工具类扩展

### 19. **Excel 配置类**
```csharp
public class ExcelOptions
{
    /// <summary>
    /// 是否启用反射缓存
    /// </summary>
    public bool EnableReflectionCache { get; set; } = true;

    /// <summary>
    /// 默认批处理大小
    /// </summary>
    public int DefaultBatchSize { get; set; } = 1000;

    /// <summary>
    /// 是否自动调整列宽
    /// </summary>
    public bool AutoSizeColumns { get; set; } = true;

    /// <summary>
    /// 默认日期格式
    /// </summary>
    public string DefaultDateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 是否启用数据验证
    /// </summary>
    public bool EnableDataValidation { get; set; } = true;

    /// <summary>
    /// 最大行数限制（用于防止内存溢出）
    /// </summary>
    public int MaxRows { get; set; } = 1_000_000;
}
```

### 20. **错误处理和日志**
```csharp
public class ExcelImportResult<T>
{
    public List<T> Data { get; set; }
    public List<ImportError> Errors { get; set; }
    public bool HasErrors => Errors.Any();
    public int SuccessCount => Data.Count;
    public int ErrorCount => Errors.Count;
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string ColumnName { get; set; }
    public string ErrorMessage { get; set; }
    public object OriginalValue { get; set; }
}
```

---

## 🔧 工具方法扩展

### 21. **实用工具方法**
```csharp
public static class ExcelUtils
{
    /// <summary>
    /// 将列索引转换为列名（0 → A, 1 → B, 26 → AA）
    /// </summary>
    public static string GetColumnName(int columnIndex);

    /// <summary>
    /// 将列名转换为列索引（A → 0, B → 1, AA → 26）
    /// </summary>
    public static int GetColumnIndex(string columnName);

    /// <summary>
    /// 检查文件是否被占用
    /// </summary>
    public static bool IsFileLocked(string filePath);

    /// <summary>
    /// 获取单元格地址（如 A1）
    /// </summary>
    public static string GetCellAddress(int row, int column);

    /// <summary>
    /// 解析单元格地址（A1 → row=0, col=0）
    /// </summary>
    public static (int row, int column) ParseCellAddress(string address);

    /// <summary>
    /// 检测 Excel 文件类型
    /// </summary>
    public static ExcelFileType DetectFileType(string filePath);
}
```

---

## 📝 建议实现顺序

### 第一阶段（核心功能）
1. ✅ **导入功能** - 补全 CRUD 的 "R"（Read）✅ 已完成
2. ⏳ **SheetExtension 实现** - 完善空文件（进行中）
3. ⏳ **数据验证** - 提高数据质量（计划中）

### 第二阶段（高级功能）
4. **模板功能** - 提高开发效率
5. **条件格式** - 增强可视化
6. **公式支持** - 支持复杂计算

### 第三阶段（专业功能）
7. **图表功能** - 数据可视化
8. **数据透视表** - 数据分析
9. **加密解密** - 安全性

### 第四阶段（优化和工具）
10. **性能优化** - 多线程、异步
11. **工具方法** - 提高开发效率
12. **样式预设** - 统一样式管理

---

## 🎓 示例代码

### 导入功能示例
```csharp
// 简单导入
var employees = ExcelImporter.ImportFromFile<Employee>("employees.xlsx");

// 带验证的导入
var result = ExcelImporter.ImportFromFileWithValidation<Employee>("data.xlsx");
if (result.HasErrors)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"行 {error.RowNumber}, 列 {error.ColumnName}: {error.ErrorMessage}");
    }
}

// 异步导入大文件
var data = await ExcelImporter.ImportFromFileAsync<LargetData>("bigfile.xlsx", cancellationToken: ct);
```

### 模板功能示例
```csharp
// 使用模板生成报表
var report = ExcelTemplateHelper.FillTemplate("monthly_report_template.xlsx", monthlyData);
report.SaveToFile("2024_01_report.xlsx");
```

### 数据验证示例
```csharp
var sheet = workbook.CreateSheet("数据录入");

// 添加下拉列表
sheet.AddDropdownList(1, 100, 0, 0, new[] { "选项1", "选项2", "选项3" });

// 添加数字验证
sheet.AddNumberValidation(1, 100, 1, 1, 0, 100);
```

---

## 🔗 相关文档

- [当前 API 文档](README.md)
- [架构文档](ARCHITECTURE.md)
- [NPOI 官方文档](https://github.com/tonyqus/npoi)

---

## 💡 贡献指南

如果您想贡献这些扩展功能，请：

1. 在 GitHub Issues 中讨论您想实现的功能
2. 遵循现有的代码风格和命名规范
3. 添加完整的 XML 文档注释
4. 提供单元测试和示例代码
5. 更新此文档和 README.md

---

**最后更新**: 2026-03-08
**版本**: 0.0.1
