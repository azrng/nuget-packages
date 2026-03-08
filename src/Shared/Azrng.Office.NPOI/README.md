# Azrng.Office.NPOI

一个基于 NPOI 再次封装的操作类库，方便进行 Excel 等 Office 文档的读写操作。

## 功能特性

### 核心功能
- ✅ **导出功能**: 支持将数据导出为 Excel 文件
- ✅ **导入功能**: 支持从 Excel 文件导入数据到对象列表
- ✅ **扩展方法**: 丰富的单元格和样式操作扩展

### 技术特性
- 基于 NPOI 封装，简化 Excel 操作
- 支持 .xls 和 .xlsx 格式
- 支持通过特性注解方式定义导出列
- 支持自定义样式和格式
- 支持合并单元格
- 支持大数据量分批导出（性能优化）
- 支持表头、标题等元素自定义

### 安全特性
- 🔒 路径遍历保护（防止恶意文件访问）
- 🔒 输入验证（文件扩展名、工作表名称验证）
- 🔒 资源管理（实现 IDisposable 模式）
- 🔒 异常处理优化（特定异常类型）

### 性能优化
- ⚡ 反射缓存机制（ConcurrentDictionary）
- ⚡ 批处理优化（减少中间集合创建）
- ⚡ 内存泄漏修复

### 跨平台支持
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.Office.NPOI
```

或通过 .NET CLI:

```
dotnet add package Azrng.Office.NPOI
```

## 使用方法

### 基本导出

定义数据模型并使用特性标记：

```csharp
public class Employee
{
    [ColumnName("员工编号")]
    public string Id { get; set; }

    [ColumnName("姓名")]
    public string Name { get; set; }

    [ColumnName("部门")]
    public string Department { get; set; }

    [ColumnName("入职日期")]
    [StringFormatter("yyyy-MM-dd")]
    public DateTime EntryDate { get; set; }

    [ColumnName("薪资")]
    public decimal Salary { get; set; }
}
```

导出数据到 Excel：

```csharp
// 准备数据
var employees = new List<Employee>
{
    new Employee { Id = "001", Name = "张三", Department = "技术部", EntryDate = DateTime.Now.AddYears(-2), Salary = 8000 },
    new Employee { Id = "002", Name = "李四", Department = "销售部", EntryDate = DateTime.Now.AddYears(-1), Salary = 7000 }
};

// 创建工作簿
var workbook = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

// 创建工作表
var sheet = workbook.CreateSheet("员工信息");

// 添加数据
sheet.AddList(employees);

// 保存到文件（自动进行路径验证和扩展名检查）
workbook.SaveToFile("employees.xlsx");
```

### 导入功能

从 Excel 文件导入数据：

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

**自动类型转换支持：**
- 数字类型：int, long, decimal, double, float（包括可空类型）
- 布尔值：bool / 1/0
- 日期：DateTime（多种格式）
- 其他：Guid, string

**特性：**
- ✅ 表头自动映射（支持中文列名）
- ✅ 自动跳过空行
- ✅ 反射缓存（高性能）
- ✅ 路径和参数验证
- ✅ 完整的错误处理

### 高级用法

#### 添加标题和自定义样式

```csharp
// 创建带标题的工作表
var title = new ExportSheetTitle("员工信息统计表")
{
    Style = new BaseStyle
    {
        FontSize = 16,
        IsBold = true,
        HorizontalAlignment = HorizontalAlignment.Center
    },
    RowHeight = 30
};

sheet.AddList(employees, title);
```

#### 单元格扩展方法

```csharp
// 获取或创建单元格
var cell = row.CreateCell(0);

// 设置值（支持多种类型）
cell.SetValue("Hello");
cell.SetValue(123);
cell.SetValue(DateTime.Now);

// 设置超链接
cell.SetHyperlink("https://example.com");

// 设置注释
cell.SetComment("这是一个重要数据");

// 设置格式
cell.SetDateFormat("yyyy-MM-dd");
cell.SetCurrencyFormat("¥");
cell.SetPercentFormat(2);

// 设置样式
cell.SetBackgroundColor(IndexedColors.LightYellow.Index);
cell.SetFontColor(IndexedColors.Red.Index);
cell.SetBold(true);
cell.SetFontSize(14);

// 检查合并区域
bool isMerged = cell.IsMerged();
var mergedRegion = cell.GetMergedRegion();

// 克隆单元格
cell.Clone(targetCell, copyStyle: true);
```

#### 使用特性控制导出行为

```csharp
public class Product
{
    [PrimaryKey]
    [ColumnName("产品编码")]
    public string Code { get; set; }

    [ColumnName("产品名称")]
    public string Name { get; set; }

    [ColumnName("分类")]
    [MergeRow] // 相同值的行将被合并
    public string Category { get; set; }

    [ColumnName("价格")]
    [ColumnStyle(typeof(CurrencyStyle))] // 使用货币样式
    public decimal Price { get; set; }

    [IgnoreColumn] // 忽略该列不导出
    public string InternalNotes { get; set; }
}
```

#### 大数据量分批导出

```csharp
// 对于大量数据，使用分批导出避免内存问题
var largeDataSet = GetLargeDataSet(); // 假设这是一个很大的数据集

sheet.AppendListBatch(largeDataSet, batchSize: 1000);
```

#### 资源管理（推荐）

```csharp
// 使用 using 语句确保资源正确释放
using var workbook = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
var sheet = workbook.CreateSheet("数据");
sheet.AddList(data);
workbook.SaveToFile("output.xlsx");
// 自动释放资源
```

## 特性说明

### 列相关特性

- `[ColumnName]`: 指定列显示名称
- `[IgnoreColumn]`: 忽略该列不导出
- `[StringFormatter]`: 指定字符串格式化方式
- `[MergeRow]`: 根据主键合并相同值的行
- `[MergeRowAlone]`: 单独合并行，与值相关
- `[PrimaryKey]`: 指定主键列

### 样式相关特性

- `[HeaderStyle]`: 设置表头样式
- `[ColumnStyle]`: 设置整列样式
- `[ColumnWidth]`: 设置列宽
- `[RowHeight]`: 设置行高

## API 参考

### ExcelHelper

- `CreateWorkbook(ExcelFileType ext)`: 创建工作簿
- `CreateSheet(this WorkbookWrapper workbook, string sheetName)`: 创建工作表（带验证）

### ExcelImporter（新增）

- `ImportFromFile<T>(string filePath, int sheetIndex, bool hasHeader, int startRow)`: 从文件导入
- `ImportFromStream<T>(Stream stream, ...)`: 从流导入
- `ImportFromBytes<T>(byte[] data, ...)`: 从字节数组导入
- `ImportFromFileAsync<T>(string filePath, ...)`: 异步导入

### WorkbookWrapper

- `CreateSheet(string sheetName)`: 创建工作表
- `SaveToFile(string filePath)`: 保存到文件（带路径验证）
- `ToStream()`: 转换为流
- `ToBytes()`: 转换为字节数组
- `Dispose()`: 释放资源

### SheetWrapper

- `AddTitle(...)`: 添加标题
- `AddList<T>(...)`: 添加数据列表
- `AppendListBatch<T>(...)`: 分批添加数据列表
- `AddCell(...)`: 添加单元格

### CellExtensions（新增）

- `SetValue<T>(this ICell cell, T value)`: 设置单元格值
- `SetHyperlink(this ICell cell, string url, ...)`: 设置超链接
- `SetComment(this ICell cell, string comment, ...)`: 设置注释
- `SetDateFormat(this ICell cell, string format)`: 设置日期格式
- `SetCurrencyFormat(this ICell cell, string currencySymbol)`: 设置货币格式
- `SetPercentFormat(this ICell cell, int decimals)`: 设置百分比格式
- `Clone(this ICell sourceCell, ICell targetCell, ...)`: 克隆单元格
- `IsMerged(this ICell cell)`: 检查是否在合并区域
- `GetMergedRegion(this ICell cell)`: 获取合并区域
- `SetBackgroundColor(this ICell cell, short colorIndex)`: 设置背景色
- `SetFontColor(this ICell cell, short colorIndex)`: 设置字体颜色
- `SetBold(this ICell cell, bool isBold)`: 设置粗体
- `SetFontSize(this ICell cell, short fontSize)`: 设置字体大小

## 安全说明

### 路径验证

`SaveToFile()` 方法会自动验证和清理文件路径：
- 规范化路径
- 检查目录是否存在
- 验证文件扩展名
- 防止路径遍历攻击

### 输入验证

`CreateSheet()` 方法会验证工作表名称：
- 不为空
- 不超过 31 个字符（Excel 限制）
- 不包含非法字符：`\`, `/`, `?`, `*`, `:`, `[`, `]`
- 不以单引号开头或结尾

## 版本更新记录

### 0.1.0
- ✅ 新增导入功能（ExcelImporter）
- ✅ 新增 CellExtensions 扩展（13个方法）
- ✅ 安全加固：路径验证、输入验证
- ✅ 性能优化：反射缓存、批处理优化
- ✅ 资源管理：实现 IDisposable 模式
- ✅ 添加 ExcelConstants 常量类
- ✅ 完善异常处理和错误提示

### 0.0.1
- ✅ 发布正式版本
- ✅ 基础导出功能
- ✅ 特性注解支持
- ✅ 自定义样式

### 0.0.1-beta2
- ✅ 处理导出样式报错问题

### 0.0.1-beta1
- ✅ NPOI 封装操作


## 许可证

MIT License
