using Azrng.Core.Extension;
using Azrng.Office.NPOI.Model;
using Azrng.Office.NPOI.Styles;
using CustomExcel.Exporter.Exporters;
using NPOI.SS.UserModel;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Azrng.Office.NPOI.Extensions;

/// <summary>
/// IWorkbook扩展
/// </summary>
public static class WorkbookExtension
{
    private static readonly ConditionalWeakTable<IWorkbook, ConcurrentDictionary<string, ICellStyle>> WorkbookStyleCaches =
        new ConditionalWeakTable<IWorkbook, ConcurrentDictionary<string, ICellStyle>>();

    /// <summary>
    /// 创建工作簿
    /// </summary>
    /// <param name="workBook"></param>
    /// <param name="sheetExportBookConfig">工作簿配置</param>
    /// <param name="sheetAction">工作簿 行索引</param>
    public static void CreateSheet(this IWorkbook workBook, BaseSheetExportConfig sheetExportBookConfig)
    {
        var sheet = sheetExportBookConfig.SheetName.IsNullOrWhiteSpace()
            ? workBook.CreateSheet()
            : workBook.CreateSheet(sheetExportBookConfig.SheetName);
        if (sheetExportBookConfig.DefaultRowHeight > 0)
        {
            sheet.DefaultRowHeight = (short)(sheetExportBookConfig.DefaultRowHeight * 20);
        }

        if (sheetExportBookConfig.DefaultColumnWidth > 0)
        {
            sheet.DefaultColumnWidth = (short)(sheetExportBookConfig.DefaultColumnWidth * 20);
        }
    }

    public static ISheet? GetSheetOrNull(this IWorkbook workBook, int index)
    {
        try
        {
            return workBook.GetSheetAt(index);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 填充sheet
    /// </summary>
    /// <param name="workBook"></param>
    /// <param name="sheet"></param>
    /// <param name="sheetWrapper"></param>
    public static void FillSheet(this IWorkbook workBook, ISheet sheet, ExportSheetWrapper sheetWrapper)
    {
        SetColumnWidth(sheet, sheetWrapper.Columns);

        foreach (var row in sheetWrapper.Rows)
        {
            var excelRow = sheet.CreateRow(row.RowIndex);

            foreach (var cell in row.Cells)
            {
                var excelCell = excelRow.CreateCell(cell.ColumnIndex);
                excelCell.SetCellValue(cell.Value);

                if (cell.CellStyle != null)
                    excelCell.CellStyle = workBook.CreateCellStyle(cell.CellStyle);
            }

            //如果设置了行高
            if (row.RowHeight > 0 && excelRow.Height < row.RowHeight * 20)
            {
                excelRow.Height = (short)(row.RowHeight * 20);
            }
        }

        //  sheet.Merged(sheetWrapper.Rows);

        static void SetColumnWidth(ISheet sheet, List<ExportColumnWrapper> columns)
        {
            foreach (var item in columns)
            {
                sheet.AutoSizeColumn(item.ColumnIndex);
                if (!item.MinWidth.HasValue && !item.MaxWidth.HasValue)
                    continue;

                var width = (int)(sheet.GetColumnWidth(item.ColumnIndex) * 1.2);

                if (item.MinWidth.HasValue && width < item.MinWidth)
                {
                    width = item.MinWidth.Value;
                    sheet.SetColumnWidth(item.ColumnIndex, width);
                }
                else if (item.MaxWidth.HasValue && width > item.MaxWidth)
                {
                    width = item.MaxWidth.Value;
                    sheet.SetColumnWidth(item.ColumnIndex, width);
                }
            }
        }
    }

    /// <summary>
    /// 获取/设置单元格样式
    /// </summary>
    /// <param name="workBook"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static ICellStyle CreateCellStyle(this IWorkbook workBook, BaseStyle style)
    {
        var styleCache = WorkbookStyleCaches.GetOrCreateValue(workBook);
        return styleCache.GetOrAdd(style.ToString(), (_) =>
        {
            var cellStyle = workBook.CreateCellStyle();
            var font = workBook.CreateFont();
            font.IsBold = style.IsBold;
            font.FontName = style.FontName;
            font.FontHeightInPoints = style.FontSize;
            font.Color = style.FontColor;
            cellStyle.SetFont(font);
            cellStyle.Alignment = style.HorizontalAlign;
            cellStyle.VerticalAlignment = style.VerticalAlign;
            cellStyle.Alignment = style.HorizontalAlign;
            cellStyle.WrapText = style.WrapText;
            if (style.FillForegroundColor.HasValue)
            {
                cellStyle.FillPattern = FillPattern.SolidForeground;
                cellStyle.FillForegroundColor = style.FillForegroundColor.Value;
            }

            cellStyle.BorderBottom = style.BorderBottom;
            cellStyle.BorderLeft = style.BorderLeft;
            cellStyle.BorderRight = style.BorderRight;
            cellStyle.BorderTop = style.BorderTop;
            cellStyle.BottomBorderColor = style.BottomBorderColor;
            cellStyle.LeftBorderColor = style.LeftBorderColor;
            cellStyle.RightBorderColor = style.TopBorderColor;
            cellStyle.TopBorderColor = style.TopBorderColor;

            return cellStyle;
        });
    }
}