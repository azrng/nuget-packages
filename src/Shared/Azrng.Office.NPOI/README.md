# Azrng.Office.NPOI

一个基于 NPOI 再次封装的操作类库，方便进行 Excel 等 Office 文档的读写操作。

## 功能特性

- 基于 NPOI 封装，简化 Excel 操作
- 支持 .xls 和 .xlsx 格式
- 支持通过特性注解方式定义导出列
- 支持自定义样式和格式
- 支持合并单元格
- 支持大数据量分批导出
- 支持表头、标题等元素自定义
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

// 保存到文件
workbook.SaveToFile("employees.xlsx");
```

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

### WorkbookWrapper

- `CreateSheet(string sheetName)`: 创建工作表
- `SaveToFile(string filePath)`: 保存到文件
- `ToStream()`: 转换为流
- `ToBytes()`: 转换为字节数组

### SheetWrapper

- `AddTitle(...)`: 添加标题
- `AddList<T>(...)`: 添加数据列表
- `AppendListBatch<T>(...)`: 分批添加数据列表
- `AddCell(...)`: 添加单元格

## 版本更新记录

* 0.0.1-beta2
  * 处理导出样式报错问题
* 0.0.1-beta1
    * Npoi封装操作