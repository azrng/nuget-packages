using Azrng.Core.Extension;
using Azrng.Office.NPOI.Attributes;
using Azrng.Office.NPOI.Extensions;
using Azrng.Office.NPOI.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Concurrent;
using System.Reflection;

namespace Azrng.Office.NPOI;

/// <summary>
/// Excel 导入帮助类
/// </summary>
public static class ExcelImporter
{
    private static readonly ConcurrentDictionary<Type, List<ColumnProperty>> ImportPropertyCache = new();

    /// <summary>
    /// 从 Excel 文件导入数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">Excel 文件路径</param>
    /// <param name="sheetIndex">工作表索引，默认为 0</param>
    /// <param name="hasHeader">是否有表头，默认为 true</param>
    /// <param name="startRow">起始行索引，默认为 0</param>
    /// <returns>导入的数据列表</returns>
    public static List<T> ImportFromFile<T>(string filePath, int sheetIndex = 0, bool hasHeader = true, int startRow = 0)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("文件路径不能为空", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"找不到 Excel 文件：{filePath}");

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".xls" && extension != ".xlsx")
            throw new ArgumentException("文件必须是 .xls 或 .xlsx 格式", nameof(filePath));

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ImportFromStream<T>(fileStream, sheetIndex, hasHeader, startRow);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"导入 Excel 文件失败：{filePath}", ex);
        }
    }

    /// <summary>
    /// 从 Stream 导入数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="stream">文件流</param>
    /// <param name="sheetIndex">工作表索引，默认为 0</param>
    /// <param name="hasHeader">是否有表头，默认为 true</param>
    /// <param name="startRow">起始行索引，默认为 0</param>
    /// <returns>导入的数据列表</returns>
    public static List<T> ImportFromStream<T>(Stream stream, int sheetIndex = 0, bool hasHeader = true, int startRow = 0)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        IWorkbook workbook;
        try
        {
            // 根据文件扩展名或内容检测文件类型
            workbook = WorkbookFactory.Create(stream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("从流创建工作簿失败", ex);
        }

        try
        {
            return ImportFromWorkbook<T>(workbook, sheetIndex, hasHeader, startRow);
        }
        finally
        {
            workbook.Close();
        }
    }

    /// <summary>
    /// 从字节数组导入数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="data">Excel 文件字节数组</param>
    /// <param name="sheetIndex">工作表索引，默认为 0</param>
    /// <param name="hasHeader">是否有表头，默认为 true</param>
    /// <param name="startRow">起始行索引，默认为 0</param>
    /// <returns>导入的数据列表</returns>
    public static List<T> ImportFromBytes<T>(byte[] data, int sheetIndex = 0, bool hasHeader = true, int startRow = 0)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("数据不能为空", nameof(data));

        using var stream = new MemoryStream(data);
        return ImportFromStream<T>(stream, sheetIndex, hasHeader, startRow);
    }

    /// <summary>
    /// 从 Excel 文件异步导入数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">Excel 文件路径</param>
    /// <param name="sheetIndex">工作表索引，默认为 0</param>
    /// <param name="hasHeader">是否有表头，默认为 true</param>
    /// <param name="startRow">起始行索引，默认为 0</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导入的数据列表</returns>
    public static async Task<List<T>> ImportFromFileAsync<T>(string filePath, int sheetIndex = 0, bool hasHeader = true, int startRow = 0,
                                                             CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ImportFromFile<T>(filePath, sheetIndex, hasHeader, startRow), cancellationToken);
    }

    /// <summary>
    /// 从 IWorkbook 导入数据（内部方法）
    /// </summary>
    private static List<T> ImportFromWorkbook<T>(IWorkbook workbook, int sheetIndex, bool hasHeader, int startRow)
    {
        if (workbook.NumberOfSheets == 0)
            throw new InvalidOperationException("工作簿不包含任何工作表");

        if (sheetIndex < 0 || sheetIndex >= workbook.NumberOfSheets)
            throw new ArgumentOutOfRangeException(nameof(sheetIndex),
                $"工作表索引 {sheetIndex} 超出范围 (0-{workbook.NumberOfSheets - 1})");

        var sheet = workbook.GetSheetAt(sheetIndex);
        var result = new List<T>();
        var properties = GetImportProperties<T>();

        // 确定数据起始行
        var dataStartRow = startRow;
        if (hasHeader)
        {
            dataStartRow = startRow + 1; // 跳过表头行
        }

        // 读取表头（如果有）
        var headerMappings = new Dictionary<int, string>();
        if (hasHeader)
        {
            var headerRow = sheet.GetRow(startRow);
            if (headerRow != null)
            {
                for (int colIndex = 0; colIndex <= headerRow.LastCellNum; colIndex++)
                {
                    var cell = headerRow.GetCell(colIndex);
                    if (cell != null)
                    {
                        var headerName = cell.GetValue();
                        if (!string.IsNullOrWhiteSpace(headerName))
                        {
                            headerMappings[colIndex] = headerName.Trim();
                        }
                    }
                }
            }
        }

        // 读取数据行
        for (int rowIndex = dataStartRow; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null || row.IsEmptyOrNull())
                continue;

            try
            {
                var item = CreateItemFromRow<T>(row, properties, headerMappings, hasHeader);
                if (item != null)
                {
                    result.Add(item);
                }
            }
            catch
            {
                // 跳过无法解析的行
                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// 获取类型的导入属性（带缓存）
    /// </summary>
    private static List<ColumnProperty> GetImportProperties<T>()
    {
        return ImportPropertyCache.GetOrAdd(typeof(T), t =>
        {
            return t.GetProperties()
                    .Where(p => p.CanWrite && p.GetCustomAttribute(typeof(IgnoreColumnAttribute)) is null)
                    .Select((p, i) => new ColumnProperty(p, i))
                    .ToList();
        });
    }

    /// <summary>
    /// 从行数据创建对象
    /// </summary>
    private static T CreateItemFromRow<T>(IRow row, List<ColumnProperty> properties, Dictionary<int, string> headerMappings, bool hasHeader)
    {
        var item = Activator.CreateInstance<T>();

        foreach (var property in properties)
        {
            try
            {
                // 确定列索引
                int colIndex;
                if (hasHeader)
                {
                    // 根据列名匹配
                    colIndex = headerMappings.FirstOrDefault(kvp =>
                                                 kvp.Value.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                                             .Key;

                    if (colIndex == 0 && !headerMappings.ContainsValue(property.Name))
                    {
                        // 未找到匹配的列，跳过
                        continue;
                    }
                }
                else
                {
                    // 直接使用属性索引
                    colIndex = property.ColumnIndex;
                }

                var cell = row.GetCell(colIndex);
                if (cell == null)
                    continue;

                var cellValue = cell.GetValue();
                if (string.IsNullOrWhiteSpace(cellValue))
                    continue;

                // 类型转换
                var convertedValue = ConvertValue(cellValue, property.PropertyInfo.PropertyType);
                if (convertedValue != null)
                {
                    property.PropertyInfo.SetValue(item, convertedValue);
                }
            }
            catch
            {
                // 跳过无法设置的属性
                continue;
            }
        }

        return item;
    }

    /// <summary>
    /// 转换单元格值为目标类型
    /// </summary>
    private static object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // 处理可空类型
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType == typeof(string))
            {
                return value;
            }
            else if (underlyingType == typeof(int) || underlyingType == typeof(int?))
            {
                if (int.TryParse(value, out var intResult))
                    return intResult;
            }
            else if (underlyingType == typeof(long) || underlyingType == typeof(long?))
            {
                if (long.TryParse(value, out var longResult))
                    return longResult;
            }
            else if (underlyingType == typeof(decimal) || underlyingType == typeof(decimal?))
            {
                if (decimal.TryParse(value, out var decimalResult))
                    return decimalResult;
            }
            else if (underlyingType == typeof(double) || underlyingType == typeof(double?))
            {
                if (double.TryParse(value, out var doubleResult))
                    return doubleResult;
            }
            else if (underlyingType == typeof(float) || underlyingType == typeof(float?))
            {
                if (float.TryParse(value, out var floatResult))
                    return floatResult;
            }
            else if (underlyingType == typeof(bool) || underlyingType == typeof(bool?))
            {
                if (bool.TryParse(value, out var boolResult))
                    return boolResult;

                // 也支持 1/0
                if (int.TryParse(value, out var intVal))
                    return intVal == 1;
            }
            else if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTime?))
            {
                if (DateTime.TryParse(value, out var dateResult))
                    return dateResult;
            }
            else if (underlyingType == typeof(Guid) || underlyingType == typeof(Guid?))
            {
                if (Guid.TryParse(value, out var guidResult))
                    return guidResult;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// 导入结果（包含错误信息）
    /// </summary>
    public class ExcelImportResult<T>
    {
        /// <summary>
        /// 成功导入的数据
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// 错误列表
        /// </summary>
        public List<ImportError> Errors { get; set; } = new();

        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool HasErrors => Errors.Any();

        /// <summary>
        /// 成功数量
        /// </summary>
        public int SuccessCount => Data.Count;

        /// <summary>
        /// 错误数量
        /// </summary>
        public int ErrorCount => Errors.Count;
    }

    /// <summary>
    /// 导入错误信息
    /// </summary>
    public class ImportError
    {
        /// <summary>
        /// 行号
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string? ColumnName { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 原始值
        /// </summary>
        public string? OriginalValue { get; set; }
    }
}