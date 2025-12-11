using Azrng.Core.Extension;
using Azrng.Office.NPOI.Attributes;
using Azrng.Office.NPOI.Attributes.Styles;
using Azrng.Office.NPOI.Styles;
using Azrng.Office.NPOI.Extensions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Reflection;

namespace Azrng.Office.NPOI.Model
{
    public class SheetWrapper
    {
        public SheetWrapper(ISheet sheet)
        {
            Sheet = sheet;
        }

        public ISheet Sheet { get; private set; }

        // /// <summary>
        // /// 最小横坐标
        // /// </summary>
        // public int MinX { get; private set; }
        //
        // /// <summary>
        // /// 最大横坐标
        // /// </summary>
        // public int MaxX { get; private set; }

        /// <summary>
        /// 最小纵坐标
        /// </summary>
        public int MinY { get; private set; }

        /// <summary>
        /// 最大纵坐标
        /// </summary>
        public int NextY { get; private set; }

        /// <summary>
        /// 创建sheet标题
        /// </summary>
        /// <param name="title">标题文本</param>
        /// <param name="startIndex">起始列索引，默认为0</param>
        /// <param name="endIndex">结束列索引，默认为0表示不合并，大于startIndex时进行跨列合并</param>
        /// <param name="style">标题样式</param>
        /// <param name="rowHeight">行高，单位：像素</param>
        /// <param name="rowIndex">行索引，默认为null表示使用当前最大行索引+1</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SheetWrapper AddTitle(string title, int startIndex = 0, int endIndex = 0, BaseStyle? style = null, int rowHeight = 0,
                                     int rowIndex = 0)
        {
            if (title.IsNullOrWhiteSpace())
                throw new ArgumentException("Sheet title cannot be null or empty", nameof(title));

            // 验证参数
            if (startIndex < 0)
                throw new ArgumentException("Start index cannot be negative", nameof(startIndex));
            if (endIndex < 0)
                throw new ArgumentException("End index cannot be negative", nameof(endIndex));
            if (endIndex > 0 && endIndex < startIndex)
                throw new ArgumentException("End index cannot be less than start index", nameof(endIndex));

            // 确定行索引
            var actualRowIndex = Math.Max(rowIndex, NextY);

            // 获取或创建行
            var row = GetOrCreateRow(Sheet, actualRowIndex);

            // 设置行高
            if (rowHeight > 0)
                row.Height = (short)(rowHeight * 20);

            // 创建标题单元格
            var cell = row.CreateCell(startIndex);
            cell.SetCellValue(title);

            // 应用样式
            if (style != null)
                cell.CellStyle = Sheet.Workbook.CreateCellStyle(style);

            // 如果需要跨列合并
            if (endIndex > startIndex)
            {
                var cellRangeAddress = new CellRangeAddress(actualRowIndex, actualRowIndex, startIndex, endIndex);
                Sheet.AddMergedRegion(cellRangeAddress);
            }

            // 更新坐标范围
            UpdateCoordinateRange(actualRowIndex, actualRowIndex);

            return this;
        }

        /// <summary>
        /// 创建sheet标题（使用ExportSheetTitle对象）
        /// </summary>
        /// <param name="sheetTitle">标题对象</param>
        /// <param name="startIndex">起始列索引，默认为0</param>
        /// <param name="endIndex">结束列索引，默认为0表示不合并，大于startIndex时进行跨列合并</param>
        /// <param name="rowIndex">行索引，默认为null表示使用当前最大行索引+1</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SheetWrapper AddTitle(ExportSheetTitle sheetTitle, int startIndex = 0, int endIndex = 0, int rowIndex = 0)
        {
            if (sheetTitle == null)
                throw new ArgumentNullException(nameof(sheetTitle));

            return AddTitle(sheetTitle.Title, startIndex, endIndex, sheetTitle.Style, sheetTitle.RowHeight,
                rowIndex);
        }

        /// <summary>
        /// 添加单元格
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="rowHeight"></param>
        /// <param name="cellStyle"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SheetWrapper AddCell(string value, int startIndex = 0, int endIndex = 0, int rowIndex = 0, int rowHeight = 0,
                                    CellStyle? cellStyle = null)
        {
            // 验证参数
            if (startIndex < 0)
                throw new ArgumentException("Start index cannot be negative", nameof(startIndex));
            if (endIndex < 0)
                throw new ArgumentException("End index cannot be negative", nameof(endIndex));
            if (endIndex > 0 && endIndex < startIndex)
                throw new ArgumentException("End index cannot be less than start index", nameof(endIndex));

            // 确定行索引
            var actualRowIndex = rowIndex == 0 ? NextY : rowIndex;

            // 获取或创建行
            var row = GetOrCreateRow(Sheet, actualRowIndex);

            // 设置行高
            if (rowHeight > 0)
                row.Height = (short)(rowHeight * 20);

            // 创建单元格
            var cell = row.CreateCell(startIndex);
            cell.SetCellValue(value);

            // 应用样式
            if (cellStyle != null)
                cell.CellStyle = Sheet.Workbook.CreateCellStyle(cellStyle);

            // 如果需要跨列合并
            if (endIndex > startIndex)
            {
                var cellRangeAddress = new CellRangeAddress(actualRowIndex, actualRowIndex, startIndex, endIndex);
                Sheet.AddMergedRegion(cellRangeAddress);
            }

            // 更新坐标范围
            UpdateCoordinateRange(actualRowIndex, actualRowIndex);

            return this;
        }

        /// <summary>
        /// 创建表格
        /// </summary>
        /// <param name="rows">数据行列表</param>
        /// <param name="sheetTitle">表格标题，可选</param>
        /// <param name="startRowIndex">起始行索引，默认为null表示使用当前最大行索引+1</param>
        /// <param name="enableMerge">是否启用合并单元格功能，默认为true</param>
        /// <param name="startIndex"></param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>返回当前Sheet对象</returns>
        /// <exception cref="ArgumentNullException">当rows为null时抛出</exception>
        /// <exception cref="ArgumentException">当没有可导出的列时抛出</exception>
        public SheetWrapper AddList<T>(List<T> rows, ExportSheetTitle? sheetTitle = null, int startIndex = 0,
                                       int startRowIndex = 0, bool enableMerge = true)
        {
            // 参数验证
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            var columnProperties = CreateColumnProperties(typeof(T));
            if (columnProperties.Count == 0)
                throw new ArgumentException("No exportable columns found for type " + typeof(T).Name, nameof(T));

            // 确定起始行索引
            var currentRowIndex = Math.Max(startRowIndex, NextY);

            var exportRows = new List<ExportRowWrapper>();

            // 创建标题行
            if (sheetTitle != null)
            {
                AddTitle(sheetTitle, startIndex, columnProperties.Count + startIndex + 1, currentRowIndex++);
            }

            // 创建表头行
            var headerRowHeight = columnProperties.Max(t => t.HeaderRowHeight);
            var headerCells = CreateHeaderCells(columnProperties);
            exportRows.Add(new ExportRowWrapper(headerCells, currentRowIndex++, headerRowHeight));

            // 填充数据行
            foreach (var row in rows)
            {
                exportRows.Add(new ExportRowWrapper(row, currentRowIndex++, columnProperties));
            }

            // 创建列配置
            var columns = CreateColumnWrappers(columnProperties);

            // 创建导出包装器
            var wrapper = new ExportSheetWrapper(columns, exportRows);

            // 填充到工作表
            Sheet.Workbook.FillSheet(Sheet, wrapper);

            // 处理合并单元格
            if (enableMerge)
            {
                Merged(Sheet, exportRows);
            }

            // 更新坐标范围
            UpdateCoordinateRange(currentRowIndex, currentRowIndex - 1);

            return this;
        }

        /// <summary>
        /// 批量创建表格（适用于大数据量）
        /// </summary>
        /// <param name="rows">数据行列表</param>
        /// <param name="sheetTitle">表格标题，可选</param>
        /// <param name="startRowIndex">起始行索引，默认为null表示使用当前最大行索引+1</param>
        /// <param name="batchSize">批处理大小，默认为1000</param>
        /// <param name="enableMerge">是否启用合并单元格功能，默认为false（大数据量时建议关闭）</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>返回当前Sheet对象</returns>
        /// <exception cref="ArgumentNullException">当rows为null时抛出</exception>
        /// <exception cref="ArgumentException">当没有可导出的列时抛出</exception>
        public ISheet AppendListBatch<T>(List<T> rows, ExportSheetTitle? sheetTitle = null, int? startRowIndex = null,
                                         int batchSize = 1000, bool enableMerge = false)
        {
            // 参数验证
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));
            if (batchSize <= 0)
                throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

            var columnProperties = CreateColumnProperties(typeof(T));
            if (columnProperties.Count == 0)
                throw new ArgumentException("No exportable columns found for type " + typeof(T).Name, nameof(T));

            // 确定起始行索引
            var actualStartRowIndex = startRowIndex ?? Math.Max(0, NextY + 1);
            var currentRowIndex = actualStartRowIndex;

            // 创建标题和表头
            if (sheetTitle != null)
            {
                var titleCells = CreateTitleCells(columnProperties, sheetTitle);
                var titleRow = new ExportRowWrapper(titleCells, currentRowIndex++, sheetTitle.RowHeight);
                var titleWrapper = new ExportSheetWrapper(CreateColumnWrappers(columnProperties), new List<ExportRowWrapper> { titleRow });
                Sheet.Workbook.FillSheet(Sheet, titleWrapper);
            }

            // 创建表头
            var headerRowHeight = columnProperties.Max(t => t.HeaderRowHeight);
            var headerCells = CreateHeaderCells(columnProperties);
            var headerRow = new ExportRowWrapper(headerCells, currentRowIndex++, headerRowHeight);
            var headerWrapper = new ExportSheetWrapper(CreateColumnWrappers(columnProperties), new List<ExportRowWrapper> { headerRow });
            Sheet.Workbook.FillSheet(Sheet, headerWrapper);

            // 批量处理数据行
            var totalRows = rows.Count;
            for (int i = 0; i < totalRows; i += batchSize)
            {
                var batchRows = rows.Skip(i).Take(batchSize).ToList();
                var exportRows = new List<ExportRowWrapper>();

                foreach (var row in batchRows)
                {
                    exportRows.Add(new ExportRowWrapper(row, currentRowIndex++, columnProperties));
                }

                var batchWrapper = new ExportSheetWrapper(CreateColumnWrappers(columnProperties), exportRows);
                Sheet.Workbook.FillSheet(Sheet, batchWrapper);

                // 处理合并单元格（仅对当前批次）
                if (enableMerge && exportRows.Count > 0)
                {
                    Merged(Sheet, exportRows);
                }
            }

            // 更新坐标范围
            UpdateCoordinateRange(actualStartRowIndex, currentRowIndex - 1);

            return Sheet;
        }

        /// <summary>
        /// 获取/创建 行
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public static IRow GetOrCreateRow(ISheet sheet, int rowIndex)
        {
            return sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
        }

        /// <summary>
        /// 合并列
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="sheet"></param>
        private void Merged(ISheet sheet, List<ExportRowWrapper> listData)
        {
            var rowMerges = new Dictionary<int, List<(int BeginRow, int EndRow, string? CompareValue)>>();
            var columnMerges = new List<(int RowIndex, List<List<int>> ColumnIndexs)>();

            foreach (var row in listData)
            {
                var rowMergeColumns = new List<List<int>>();
                foreach (var cell in row.Cells)
                {
                    if (cell.MergeColumn)
                    {
                        //最后一组需要合并的列
                        var lastList = rowMergeColumns.LastOrDefault();
                        if (lastList == null)
                        {
                            rowMergeColumns.Add(new List<int> { cell.ColumnIndex });
                        }
                        else
                        {
                            //最后一个需要合并的列索引
                            var lastColumnIndex = lastList.Last();

                            //如果和当前列是相邻列
                            if (lastColumnIndex == cell.ColumnIndex - 1)
                                lastList.Add(cell.ColumnIndex);

                            //如果不是相邻列，且上一组合并列只有一个列，移除
                            else if (lastList.Count == 1)
                                rowMergeColumns.RemoveAt(rowMergeColumns.Count - 1);
                        }
                    }

                    //如果该单元格不是单独合并并且不是根据主键合并
                    if (!cell.MergedRowAlone && !cell.MergedRowByPrimaryKey) continue;

                    //判断合并行对比值，主键或者当前列值
                    var compareValue = cell.MergedRowByPrimaryKey ? row.PrimaryKey : cell.Value;

                    //当前列当前行,取到最后一个合并行
                    if (!rowMerges.TryGetValue(cell.ColumnIndex, out List<(int _0, int _1, string? _2)> preDataList))
                    {
                        rowMerges.Add(cell.ColumnIndex, [(row.RowIndex, 0, compareValue)]);
                        continue;
                    }

                    var (lastBeginRow, _, lastCompareValue) = preDataList.Last();

                    //当前行与上一行相等，重置EndRow
                    if (lastCompareValue == compareValue)
                    {
                        preDataList.RemoveAt(preDataList.Count - 1);
                        preDataList.Add((lastBeginRow, row.RowIndex, lastCompareValue));
                    }
                    else
                    {
                        preDataList.Add((row.RowIndex, 0, compareValue));
                    }
                }

                //上一组合并列只有一个列，移除
                if (rowMergeColumns.Count > 0 && rowMergeColumns.Last().Count == 1)
                    rowMergeColumns.RemoveAt(rowMergeColumns.Count - 1);

                if (rowMergeColumns.IsNotNullOrEmpty())
                    columnMerges.Add((row.RowIndex, rowMergeColumns));
            }

            foreach (var merge in rowMerges)
            {
                foreach (var (beginRow, endRow, _) in merge.Value.Where(x => x.EndRow > 0))
                    sheet.AddMergedRegion(new CellRangeAddress(beginRow, endRow, merge.Key, merge.Key));
            }

            foreach (var (rowIndex, columnIndexs) in columnMerges)
            {
                foreach (var columns in columnIndexs)
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, columns[0], columns.Last()));
            }
        }

        /// <summary>
        /// 创建标题单元格
        /// </summary>
        /// <param name="columnProperties">列属性列表</param>
        /// <param name="sheetTitle">标题对象</param>
        /// <returns>标题单元格列表</returns>
        private static List<ExportCellWrapper> CreateTitleCells(List<ColumnProperty> columnProperties, ExportSheetTitle sheetTitle)
        {
            return columnProperties.ConvertAll(x =>
                new ExportCellWrapper(x.ColumnIndex, x.ColumnIndex == 0 ? sheetTitle.Title : string.Empty)
                {
                    MergeColumn = true, CellStyle = x.ColumnIndex == 0 ? sheetTitle.Style : null
                });
        }

        /// <summary>
        /// 创建表头单元格
        /// </summary>
        /// <param name="columnProperties">列属性列表</param>
        /// <returns>表头单元格列表</returns>
        private static List<ExportCellWrapper> CreateHeaderCells(List<ColumnProperty> columnProperties)
        {
            return columnProperties.ConvertAll(x =>
                new ExportCellWrapper(x.ColumnIndex, x.IsPrimaryColumn, x.Name, x.HeaderStyle));
        }

        /// <summary>
        /// 创建列包装器
        /// </summary>
        /// <param name="columnProperties">列属性列表</param>
        /// <returns>列包装器列表</returns>
        private static List<ExportColumnWrapper> CreateColumnWrappers(List<ColumnProperty> columnProperties)
        {
            return columnProperties.ConvertAll(x => new ExportColumnWrapper(x.ColumnIndex, x.Name)
                                                    {
                                                        MinWidth = x.MinWidth, MaxWidth = x.MaxWidth
                                                    });
        }

        /// <summary>
        /// 更新坐标范围
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="endRowIndex">结束行索引</param>
        private void UpdateCoordinateRange(int rowIndex, int endRowIndex)
        {
            if (MinY == 0 && NextY == 0)
            {
                // 首次设置
                MinY = rowIndex;
                NextY = endRowIndex + 1;
            }
            else
            {
                // 更新范围
                MinY = Math.Min(MinY, rowIndex);
                NextY = Math.Max(NextY, endRowIndex + 1);
            }
        }

        /// <summary>
        /// 创建列属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<ColumnProperty> CreateColumnProperties(Type type)
        {
            //查询没有被忽略的字段，并且获取该字段的特性信息
            var columnProperties = type.GetProperties()
                                       .Where(x => x.GetCustomAttribute(typeof(IgnoreColumnAttribute)) is null)
                                       .Select((m, i) => new ColumnProperty(m, i))
                                       .ToList();

            // for (var i = 0; i < columnProperties.Count; i++)
            //     columnProperties[i].ColumnIndex = i;

            //设置主键列
            var mergeColumns = columnProperties.Where(x => x.MergedRowByPrimaryKey).ToList();

            //如果标注了特性MergeRowAttribute(MergedRowByPrimaryKey为true),但是列里面没有主键列，那么就将标注该特性的列设置为主键列
            if (mergeColumns.Any() && columnProperties.All(x => !x.IsPrimaryColumn))
            {
                foreach (var item in mergeColumns)
                    item.IsPrimaryColumn = true;
            }

            return columnProperties;
        }
    }
}