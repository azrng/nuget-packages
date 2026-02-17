# Azrng.Office.NPOI 项目架构与原理说明

## 项目概述

Azrng.Office.NPOI 是一个基于 NPOI 库的二次封装，旨在简化 Excel 文档的读写操作。该项目通过特性注解（Attributes）、包装器模式（Wrapper Pattern）和扩展方法等技术，提供了简洁易用的 API 接口。

## 项目目录结构

```
Azrng.Office.NPOI/
├── Attributes/              # 特性定义目录
│   ├── Styles/             # 样式相关特性
│   │   ├── ColumnStyleAttribute.cs
│   │   ├── ColumnWidthAttribute.cs
│   │   ├── HeaderStyleAttribute.cs
│   │   ├── RowHeightAttribute.cs
│   │   └── StyleAttribute.cs
│   ├── ColumnNameAttribute.cs
│   ├── ColumnProperty.cs
│   ├── IgnoreColumnAttribute.cs
│   ├── MergeRowAloneAttribute.cs
│   ├── MergeRowAttribute.cs
│   ├── PrimaryKeyAttribute.cs
│   └── StringFormatterAttribute.cs
├── Extensions/              # 扩展方法目录
│   ├── CellExtensions.cs
│   ├── RowExtensions.cs
│   ├── SheetExtension.cs
│   └── WorkbookExtension.cs
├── Model/                   # 核心模型目录
│   ├── BaseSheetExportConfig.cs
│   ├── ExcelFileType.cs
│   ├── ExportCellWrapper.cs
│   ├── ExportColumnWrapper.cs
│   ├── ExportRowWrapper.cs
│   ├── ExportSheetTitle.cs
│   ├── ExportSheetWrapper.cs
│   ├── SheetWrapper.cs
│   └── WorkbookWrapper.cs
├── Styles/                  # 样式定义目录
│   ├── BaseDetailCellStyle.cs
│   ├── BaseStyle.cs
│   ├── DynamicCellStyle.cs
│   └── TitleCellStyle.cs
├── ExcelHelper.cs          # 核心帮助类
├── ExportConfig.cs         # 导出配置
└── README.md              # 项目说明文档
```

## 核心架构设计

### 1. 分层架构

项目采用经典的分层架构设计：

```
┌─────────────────────────────────────┐
│      API 层 (用户调用接口)           │
│  ExcelHelper.CreateWorkbook()       │
│  SheetWrapper.AddList<T>()          │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      包装器层 (Wrapper Layer)        │
│  WorkbookWrapper / SheetWrapper     │
│  ExportRowWrapper / ExportCellWrapper│
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│     扩展层 (Extension Layer)         │
│  WorkbookExtension / CellExtensions  │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│    NPOI 核心层 (NPOI Core)           │
│  IWorkbook / ISheet / IRow / ICell  │
└─────────────────────────────────────┘
```

### 2. 核心组件说明

#### 2.1 API 入口层 - ExcelHelper

[ExcelHelper.cs](src/Shared/Azrng.Office.NPOI/ExcelHelper.cs)

```csharp
public static class ExcelHelper
{
    // 创建工作簿
    public static WorkbookWrapper CreateWorkbook(ExcelFileType ext)

    // 创建工作表（扩展方法）
    public static SheetWrapper CreateSheet(this WorkbookWrapper workbook, string sheetName)
}
```

**设计职责：**
- 作为项目的统一入口点
- 提供简洁的静态方法用于创建工作簿和工作表
- 封装 NPOI 复杂的初始化逻辑

#### 2.2 包装器层 (Wrapper Layer)

##### WorkbookWrapper

[WorkbookWrapper.cs](src/Shared/Azrng.Office.NPOI/Model/WorkbookWrapper.cs)

包装 NPOI 的 `IWorkbook` 接口，提供以下功能：

```csharp
public class WorkbookWrapper
{
    // 构造函数
    public WorkbookWrapper(IWorkbook workbook)
    public WorkbookWrapper(ExcelFileType ext)

    // 导出方法
    public Stream ToStream()              // 转换为流
    public void WriteStream(FileStream)   // 写入流
    public void SaveToFile(string)        // 保存到文件
    public byte[] ToBytes()               // 转换为字节数组
}
```

**设计原理：**
- 根据 `ExcelFileType` 枚举创建对应的工作簿实例：
  - `Xls` → `HSSFWorkbook` (Excel 97-2003)
  - `Xlsx` → `XSSFWorkbook` (Excel 2007+)
- 封装资源释放逻辑（try-finally 模式）

##### SheetWrapper

[SheetWrapper.cs](src/Shared/Azrng.Office.NPOI/Model/SheetWrapper.cs)

包装 NPOI 的 `ISheet` 接口，是整个项目的核心组件：

```csharp
public class SheetWrapper
{
    // 核心方法
    public SheetWrapper AddTitle(...)                    // 添加标题
    public SheetWrapper AddCell(...)                     // 添加单元格
    public SheetWrapper AddList<T>(...)                  // 添加数据列表
    public ISheet AppendListBatch<T>(...)                // 分批添加数据

    // 内部方法
    private void Merged(...)                             // 合并单元格
    private static List<ExportCellWrapper> CreateHeaderCells(...)  // 创建表头
    private static List<ColumnProperty> CreateColumnProperties(...) // 解析列属性
}
```

**核心工作流程：**

1. **反射解析列属性**
   ```csharp
   private static List<ColumnProperty> CreateColumnProperties(Type type)
   {
       // 获取类型属性，忽略标记了 [IgnoreColumn] 的属性
       // 为每个属性创建 ColumnProperty 对象
       // 自动识别并设置主键列
   }
   ```

2. **创建导出数据结构**
   - `ExportSheetWrapper`: 包含列和行的完整数据结构
   - `ExportRowWrapper`: 表示一行数据，包含多个单元格
   - `ExportCellWrapper`: 表示单个单元格

3. **数据填充到 Sheet**
   ```csharp
   Sheet.Workbook.FillSheet(Sheet, wrapper);  // 扩展方法
   ```

4. **单元格合并处理**
   - 根据主键合并行（`[MergeRow]`）
   - 独立值合并行（`[MergeRowAlone]`）
   - 相邻列合并

##### ExportRowWrapper

[ExportRowWrapper.cs](src/Shared/Azrng.Office.NPOI/Model/ExportRowWrapper.cs)

```csharp
public class ExportRowWrapper
{
    public int RowIndex { get; }              // 行索引
    public int RowHeight { get; set; }        // 行高
    public List<ExportCellWrapper> Cells { get; set; }  // 单元格列表
    public string? PrimaryKey { get; }        // 主键（用于合并判断）
}
```

##### ExportCellWrapper

[ExportCellWrapper.cs](src/Shared/Azrng.Office.NPOI/Model/ExportCellWrapper.cs)

```csharp
public class ExportCellWrapper
{
    public int ColumnIndex { get; set; }      // 列索引
    public string Value { get; set; }         // 单元格值
    public bool IsPrimaryKeyColumn { get; set; }  // 是否主键列
    public bool MergeColumn { get; set; }     // 是否参与列合并
    public bool MergedRowByPrimaryKey { get; set; }  // 是否按主键合并行
    public bool MergedRowAlone { get; set; }  // 是否独立合并行
    public BaseStyle? CellStyle { get; set; } // 单元格样式
}
```

#### 2.3 特性系统 (Attributes)

##### 列属性特性

| 特性 | 用途 | 示例 |
|------|------|------|
| `[ColumnName]` | 指定列显示名称 | `[ColumnName("员工姓名")]` |
| `[IgnoreColumn]` | 忽略该列不导出 | `[IgnoreColumn]` |
| `[PrimaryKey]` | 标记主键列 | `[PrimaryKey]` |
| `[StringFormatter]` | 格式化字符串 | `[StringFormatter("yyyy-MM-dd")]` |

##### 合并特性

| 特性 | 合并规则 | 说明 |
|------|----------|------|
| `[MergeRow]` | 按主键值合并 | 相同主键的行会合并 |
| `[MergeRowAlone]` | 按当前列值合并 | 相同值的行会合并，优先级更高 |

##### 样式特性

| 特性 | 作用域 | 说明 |
|------|--------|------|
| `[HeaderStyle]` | 表头 | 设置表头行的样式 |
| `[ColumnStyle]` | 数据列 | 设置整个数据列的样式 |
| `[ColumnWidth]` | 列宽 | 设置列的最小/最大宽度 |
| `[RowHeight]` | 行高 | 设置行的高度 |

##### ColumnProperty

[ColumnProperty.cs](src/Shared/Azrng.Office.NPOI/Attributes/ColumnProperty.cs)

核心属性解析类，通过反射读取类型上的特性信息：

```csharp
public class ColumnProperty
{
    public int ColumnIndex { get; set; }           // 列索引
    public string Name { get; set; }               // 列名
    public BaseStyle? HeaderStyle { get; set; }    // 表头样式
    public BaseStyle? ColumnStyle { get; set; }    // 列样式
    public string? StringFormat { get; set; }      // 格式化字符串
    public bool MergedRowByPrimaryKey { get; set; } // 是否按主键合并
    public bool MergedRowAlone { get; set; }       // 是否独立合并
    public bool IsPrimaryColumn { get; set; }      // 是否主键列
    public PropertyInfo PropertyInfo { get; set; } // 属性信息

    // 获取单元格值
    public string GetCellValue(object target)
}
```

**值获取逻辑：**
```csharp
public string GetCellValue(object target)
{
    var objectValue = PropertyInfo.GetValue(target);

    if (objectValue != null)
    {
        // DateTime 特殊处理
        if (PropertyInfo.PropertyType == typeof(DateTime) || ...)
        {
            var format = StringFormat.IsNullOrWhiteSpace()
                ? ExportConfig.DefaultDateFormat  // "yyyy-MM-dd HH:mm:ss"
                : StringFormat;
            return ((DateTime)objectValue).ToString(format);
        }
        else
        {
            return objectValue.ToString();
        }
    }

    return string.Empty;
}
```

#### 2.4 样式系统 (Styles)

##### BaseStyle

[BaseStyle.cs](src/Shared/Azrng.Office.NPOI/Styles/BaseStyle.cs)

所有样式的基础类：

```csharp
public class BaseStyle
{
    // 字体属性
    public bool IsBold { get; set; }              // 加粗
    public int FontSize { get; set; }             // 字号
    public string FontName { get; set; }          // 字体名称
    public short FontColor { get; set; }          // 字体颜色

    // 对齐属性
    public HorizontalAlignment HorizontalAlign { get; set; }  // 水平对齐
    public VerticalAlignment VerticalAlign { get; set; }      // 垂直对齐

    // 边框属性
    public BorderStyle BorderBottom { get; set; }
    public BorderStyle BorderLeft { get; set; }
    public BorderStyle BorderRight { get; set; }
    public BorderStyle BorderTop { get; set; }

    // 其他属性
    public bool WrapText { get; set; }            // 自动换行
    public short? FillForegroundColor { get; set; } // 背景色
}
```

**样式缓存机制：**
样式通过 `ToString()` 方法生成唯一键，用于样式缓存：

```csharp
public override string ToString()
{
    // 将所有属性序列化为字符串作为缓存键
}
```

##### DynamicCellStyle

支持动态样式设置，数据对象可实现此接口：

```csharp
public class DynamicCellStyle
{
    public List<KeyValuePair<string, BaseStyle>> PropertyNameStylePair { get; set; }
}
```

#### 2.5 扩展方法层 (Extensions)

##### WorkbookExtension

[WorkbookExtension.cs](src/Shared/Azrng.Office.NPOI/Extensions/WorkbookExtension.cs)

```csharp
public static class WorkbookExtension
{
    // 样式缓存：使用 ConditionalWeakTable 实现弱引用缓存
    private static readonly ConditionalWeakTable<IWorkbook,
        ConcurrentDictionary<string, ICellStyle>> WorkbookStyleCaches = ...;

    // 核心方法
    public static void FillSheet(this IWorkbook workBook, ISheet sheet, ExportSheetWrapper sheetWrapper)
    public static ICellStyle CreateCellStyle(this IWorkbook workBook, BaseStyle style)
}
```

**FillSheet 方法流程：**

1. 设置列宽（自动调整 + 限制范围）
2. 遍历所有行
3. 创建 Excel 行和单元格
4. 设置单元格值和样式
5. 设置行高

**样式缓存机制：**

使用 `ConditionalWeakTable` 实现样式缓存，避免重复创建相同样式：

```csharp
public static ICellStyle CreateCellStyle(this IWorkbook workBook, BaseStyle style)
{
    var styleCache = WorkbookStyleCaches.GetOrCreateValue(workBook);
    return styleCache.GetOrAdd(style.ToString(), (_) => {
        // 创建新样式
        var cellStyle = workBook.CreateCellStyle();
        // ... 设置样式属性
        return cellStyle;
    });
}
```

## 核心功能原理

### 1. 数据导出流程

```
用户代码调用
    ↓
SheetWrapper.AddList<T>(List<T> data)
    ↓
1. 反射解析类型特性 → List<ColumnProperty>
    ↓
2. 创建表头行 → ExportRowWrapper (header)
    ↓
3. 遍历数据创建数据行 → List<ExportRowWrapper>
    ↓
4. 创建导出包装器 → ExportSheetWrapper(columns, rows)
    ↓
5. 填充到 Sheet → WorkbookExtension.FillSheet()
    ↓
6. 处理单元格合并 → Merged()
    ↓
完成导出
```

### 2. 单元格合并算法

[Merged 方法](src/Shared/Azrng.Office.NPOI/Model/SheetWrapper.cs#L323-L402)

#### 行合并（纵向）

```csharp
// 用于记录需要合并的列信息
Dictionary<int, List<(int BeginRow, int EndRow, string CompareValue)>> rowMerges

foreach (var row in listData)
{
    foreach (var cell in row.Cells)
    {
        if (cell.MergedRowByPrimaryKey || cell.MergedRowAlone)
        {
            // 确定比较值（主键或当前单元格值）
            var compareValue = cell.MergedRowByPrimaryKey
                ? row.PrimaryKey
                : cell.Value;

            // 检查上一行的值
            if (lastCompareValue == compareValue)
            {
                // 值相同，更新结束行
                UpdateEndRow();
            }
            else
            {
                // 值不同，开始新的合并组
                StartNewMergeGroup();
            }
        }
    }
}

// 执行合并
sheet.AddMergedRegion(new CellRangeAddress(beginRow, endRow, columnIndex, columnIndex));
```

#### 列合并（横向）

```csharp
// 用于记录需要合并的列信息
List<(int RowIndex, List<List<int>> ColumnIndexs)> columnMerges

// 逻辑：
// 1. 找出标记了 MergeColumn=true 的相邻列
// 2. 将相邻的列索引分组
// 3. 对每组进行合并
sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, firstColumn, lastColumn));
```

### 3. 大数据量分批处理

[AppendListBatch 方法](src/Shared/Azrng.Office.NPOI/Model/SheetWrapper.cs#L246-L305)

**设计思路：**

1. **分批处理**：将大数据集按批次大小（默认 1000）分割
2. **逐批写入**：每批数据独立处理，避免内存堆积
3. **可选合并**：默认关闭单元格合并（大数据场景下合并计算开销大）

```csharp
for (int i = 0; i < totalRows; i += batchSize)
{
    var batchRows = rows.Skip(i).Take(batchSize).ToList();

    // 创建当前批次的 ExportRowWrapper
    var exportRows = new List<ExportRowWrapper>();
    foreach (var row in batchRows)
    {
        exportRows.Add(new ExportRowWrapper(row, currentRowIndex++, columnProperties));
    }

    // 填充当前批次
    var batchWrapper = new ExportSheetWrapper(CreateColumnWrappers(columnProperties), exportRows);
    Sheet.Workbook.FillSheet(Sheet, batchWrapper);

    // 处理合并（如果启用）
    if (enableMerge && exportRows.Count > 0)
    {
        Merged(Sheet, exportRows);
    }
}
```

### 4. 主键识别机制

[CreateColumnProperties 方法](src/Shared/Azrng.Office.NPOI/Model/SheetWrapper.cs#L469-L491)

**自动主键识别逻辑：**

```csharp
// 1. 获取需要按主键合并的列
var mergeColumns = columnProperties.Where(x => x.MergedRowByPrimaryKey).ToList();

// 2. 如果存在需要按主键合并的列，但没有明确的主键列
if (mergeColumns.Any() && columnProperties.All(x => !x.IsPrimaryColumn))
{
    // 自动将需要合并的列设置为主键列
    foreach (var item in mergeColumns)
        item.IsPrimaryColumn = true;
}
```

**主键作用：**
- 用于 `MergedRowByPrimaryKey` 合并判断
- 在 `ExportRowWrapper` 中组合成 `PrimaryKey` 属性（支持多列组合）

## 设计模式与最佳实践

### 1. 包装器模式 (Wrapper Pattern)

将 NPOI 原生接口包装为更易用的 API：

```csharp
IWorkbook → WorkbookWrapper
ISheet → SheetWrapper
```

### 2. 特性驱动设计 (Attribute-Driven Design)

通过特性声明式定义导出行为：

```csharp
public class Employee
{
    [ColumnName("员工编号")]
    [PrimaryKey]
    public string Id { get; set; }

    [ColumnName("入职日期")]
    [StringFormatter("yyyy-MM-dd")]
    public DateTime EntryDate { get; set; }
}
```

### 3. 扩展方法模式 (Extension Method Pattern)

为 NPOI 接口添加扩展方法：

```csharp
public static ICellStyle CreateCellStyle(this IWorkbook workBook, BaseStyle style)
public static string GetValue(this ICell cell)
```

### 4. 缓存优化 (Caching Strategy)

使用 `ConditionalWeakTable` 实现样式缓存：

- **弱引用**：不会阻止垃圾回收
- **键关联**：与 Workbook 生命周期绑定
- **线程安全**：使用 `ConcurrentDictionary`

### 5. 流式处理 (Stream Processing)

支持多种输出方式：

```csharp
workbook.SaveToFile(path);      // 文件
workbook.ToStream();            // 流
workbook.ToBytes();             // 字节数组
workbook.WriteStream(stream);   // 自定义流
```

## 扩展性设计

### 1. 自定义样式

通过继承 `BaseStyle` 创建自定义样式：

```csharp
public class CurrencyStyle : BaseStyle
{
    public CurrencyStyle()
    {
        HorizontalAlign = HorizontalAlignment.Right;
    }
}
```

### 2. 动态样式

实现 `DynamicCellStyle` 接口：

```csharp
public class Employee : DynamicCellStyle
{
    public decimal Salary { get; set; }

    public Employee()
    {
        PropertyNameStylePair = new List<KeyValuePair<string, BaseStyle>>
        {
            new(nameof(Salary), new SalaryStyle())
        };
    }
}
```

### 3. 自定义格式化

通过 `[StringFormatter]` 特性自定义格式：

```csharp
[StringFormatter("0.00")]  // 保留两位小数
[StringFormatter("yyyy年MM月dd日")]  // 中文日期格式
```

## 性能优化建议

### 1. 大数据量场景

```csharp
// 使用分批导出，避免内存溢出
sheet.AppendListBatch(largeData, batchSize: 1000, enableMerge: false);
```

### 2. 样式复用

```csharp
// 预定义样式并复用，减少样式对象创建
var commonStyle = new BaseStyle { FontSize = 12 };
```

### 3. 避免过度反射

- 在循环外预先解析 `ColumnProperty`
- 项目已通过缓存机制优化

## 依赖项

- **NPOI**: Excel 文档操作核心库
- **Azrng.Core**: 内部核心库（扩展方法等）

## 版本支持

支持 .NET 多版本：
- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 9.0
- .NET 10.0

## 项目文件对应关系

| 功能模块 | 主要文件 | 说明 |
|---------|---------|------|
| 入口 API | [ExcelHelper.cs](src/Shared/Azrng.Office.NPOI/ExcelHelper.cs) | 统一入口点 |
| 工作簿包装 | [WorkbookWrapper.cs](src/Shared/Azrng.Office.NPOI/Model/WorkbookWrapper.cs) | 工作簿封装 |
| 工作表包装 | [SheetWrapper.cs](src/Shared/Azrng.Office.NPOI/Model/SheetWrapper.cs) | 核心功能实现 |
| 列属性解析 | [ColumnProperty.cs](src/Shared/Azrng.Office.NPOI/Attributes/ColumnProperty.cs) | 特性解析 |
| 样式系统 | [BaseStyle.cs](src/Shared/Azrng.Office.NPOI/Styles/BaseStyle.cs) | 样式基础类 |
| 数据填充 | [WorkbookExtension.cs](src/Shared/Azrng.Office.NPOI/Extensions/WorkbookExtension.cs) | Sheet 填充 |
| 单元格扩展 | [CellExtensions.cs](src/Shared/Azrng.Office.NPOI/Extensions/CellExtensions.cs) | 值获取 |

---

**文档版本**: 1.0
**最后更新**: 2026-02-17
**维护者**: Azrng Team