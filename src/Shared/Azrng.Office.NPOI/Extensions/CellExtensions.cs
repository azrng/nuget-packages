using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using Azrng.Office.NPOI;
using Azrng.Office.NPOI.Model;

namespace Azrng.Office.NPOI.Extensions
{
    public static class CellExtensions
    {
        /// <summary>
        /// 根据Excel列类型获取列的值.
        /// cell不存在返回string.Empty
        /// </summary>
        /// <param name="cell">cell.</param>
        /// <returns>值.</returns>
        public static string GetValue(this ICell? cell)
        {
            if (cell == null)
                return string.Empty;

            var ret = cell.CellType switch
            {
                CellType.Blank => string.Empty,
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Error => cell.ErrorCellValue.ToString(),
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                    ? $"{cell.DateCellValue:G}"
                    : cell.NumericCellValue.ToString(ExcelCulture.InvariantCulture),
                CellType.String => cell.StringCellValue,
                CellType.Formula => GetFormulaCellValue(cell),
                _ => cell.ToString(),
            };

            return ret?.Trim() ?? string.Empty;

            static string GetFormulaCellValue(ICell cell)
            {
                try
                {
                    var workbookTypeName = cell.Sheet.Workbook.GetType().Name;
                    if (workbookTypeName == "HSSFWorkbook")
                    {
                        var evaluator = new HSSFFormulaEvaluator(cell.Sheet.Workbook);
                        evaluator.EvaluateInCell(cell);
                        return cell.ToString() ?? string.Empty;
                    }
                    else
                    {
                        var evaluator = new XSSFFormulaEvaluator(cell.Sheet.Workbook);
                        evaluator.EvaluateInCell(cell);
                        return cell.ToString() ?? string.Empty;
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
                {
                    // 公式评估失败，尝试返回数值或字符串
                    return cell.CellType == CellType.Numeric
                        ? cell.NumericCellValue.ToString(ExcelCulture.InvariantCulture)
                        : (cell.StringCellValue ?? string.Empty);
                }
                catch
                {
                    // 最后的回退选项
                    return cell.CellType == CellType.Numeric
                        ? cell.NumericCellValue.ToString(ExcelCulture.InvariantCulture)
                        : string.Empty;
                }
            }
        }

        /// <summary>
        /// 设置单元格值（支持多种类型）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="cell">单元格</param>
        /// <param name="value">值</param>
        public static ICell SetValue<T>(this ICell cell, T value)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            if (value == null)
            {
                cell.SetBlank();
                return cell;
            }

            switch (value)
            {
                case string str:
                    cell.SetCellValue(str);
                    break;
                case int i:
                    cell.SetCellValue(i);
                    break;
                case double d:
                    cell.SetCellValue(d);
                    break;
                case decimal dec:
                    cell.SetCellValue((double)dec);
                    break;
                case bool b:
                    cell.SetCellValue(b);
                    break;
                case DateTime dt:
                    cell.SetCellValue(dt);
                    break;
                case IConvertible convertible:
                    cell.SetCellValue(convertible.ToString());
                    break;
                default:
                    cell.SetCellValue(value.ToString());
                    break;
            }

            return cell;
        }

        /// <summary>
        /// 设置超链接
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="url">URL地址</param>
        /// <param name="type">超链接类型，默认为 Url</param>
        public static ICell SetHyperlink(this ICell cell, string url, HyperlinkType type = HyperlinkType.Url)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            var workbook = cell.Sheet.Workbook;
            var link = workbook.GetCreationHelper().CreateHyperlink(type);
            link.Address = url;
            cell.Hyperlink = link;

            // 设置为蓝色下划线样式
            var font = workbook.CreateFont();
            font.Underline = FontUnderlineType.Single;
            font.Color = IndexedColors.Blue.Index;
            font.IsBold = false;

            var style = workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);
            style.SetFont(font);

            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 设置单元格注释
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="comment">注释内容</param>
        /// <param name="author">注释作者，默认为 "System"</param>
        public static ICell SetComment(this ICell cell, string comment, string author = "System")
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment cannot be null or empty", nameof(comment));

            var workbook = cell.Sheet.Workbook;
            var patriarch = cell.Sheet.CreateDrawingPatriarch();
            var anchor = patriarch.CreateAnchor(0, 0, 0, 0, cell.ColumnIndex, cell.RowIndex, cell.ColumnIndex + 3, cell.RowIndex + 6);

            var commentObj = patriarch.CreateCellComment(anchor);
            commentObj.String = workbook.GetCreationHelper().CreateRichTextString(comment);
            commentObj.Author = author ?? "System";

            cell.CellComment = commentObj;

            return cell;
        }

        /// <summary>
        /// 设置为日期格式
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="format">日期格式字符串，默认为 "yyyy-MM-dd"</param>
        public static ICell SetDateFormat(this ICell cell, string format = "yyyy-MM-dd")
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);
            style.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat(format);

            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 设置为货币格式
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="currencySymbol">货币符号，默认为 "¥"</param>
        public static ICell SetCurrencyFormat(this ICell cell, string currencySymbol = "¥")
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var format = $"\"{currencySymbol}\"#,##0.00";
            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);
            style.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat(format);

            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 设置为百分比格式
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="decimals">小数位数，默认为 2</param>
        public static ICell SetPercentFormat(this ICell cell, int decimals = 2)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var format = new string('0', decimals + 2) + "." + new string('0', decimals) + "%";
            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);
            style.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat(format);

            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 克隆单元格
        /// </summary>
        /// <param name="sourceCell">源单元格</param>
        /// <param name="targetCell">目标单元格</param>
        /// <param name="copyStyle">是否复制样式，默认为 true</param>
        public static ICell Clone(this ICell sourceCell, ICell targetCell, bool copyStyle = true)
        {
            if (sourceCell == null)
                throw new ArgumentNullException(nameof(sourceCell));

            if (targetCell == null)
                throw new ArgumentNullException(nameof(targetCell));

            // 复制值
            switch (sourceCell.CellType)
            {
                case CellType.Numeric:
                    targetCell.SetCellValue(sourceCell.NumericCellValue);
                    break;
                case CellType.String:
                    targetCell.SetCellValue(sourceCell.StringCellValue);
                    break;
                case CellType.Boolean:
                    targetCell.SetCellValue(sourceCell.BooleanCellValue);
                    break;
                case CellType.Formula:
                    targetCell.SetCellFormula(sourceCell.CellFormula);
                    break;
                case CellType.Blank:
                    targetCell.SetBlank();
                    break;
                default:
                    targetCell.SetCellValue(sourceCell.ToString());
                    break;
            }

            // 复制样式
            if (copyStyle && sourceCell.CellStyle != null)
            {
                targetCell.CellStyle = sourceCell.CellStyle;
            }

            // 复制超链接
            if (sourceCell.Hyperlink != null)
            {
                targetCell.Hyperlink = sourceCell.Hyperlink;
            }

            return targetCell;
        }

        /// <summary>
        /// 检查单元格是否在合并区域内
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <returns>是否在合并区域内</returns>
        public static bool IsMerged(this ICell cell)
        {
            if (cell == null)
                return false;

            for (int i = 0; i < cell.Sheet.NumMergedRegions; i++)
            {
                var mergedRegion = cell.Sheet.GetMergedRegion(i);
                if (mergedRegion.IsInRange(cell.RowIndex, cell.ColumnIndex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取合并区域的范围（如果单元格在合并区域内）
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <returns>合并区域范围，如果不在合并区域内则返回 null</returns>
        public static CellRangeAddress? GetMergedRegion(this ICell cell)
        {
            if (cell == null)
                return null;

            for (int i = 0; i < cell.Sheet.NumMergedRegions; i++)
            {
                var mergedRegion = cell.Sheet.GetMergedRegion(i);
                if (mergedRegion.IsInRange(cell.RowIndex, cell.ColumnIndex))
                {
                    return mergedRegion;
                }
            }

            return null;
        }

        /// <summary>
        /// 设置单元格背景色
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="colorIndex">颜色索引（IndexedColors）</param>
        public static ICell SetBackgroundColor(this ICell cell, short colorIndex)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);
            style.FillForegroundColor = colorIndex;
            style.FillPattern = FillPattern.SolidForeground;

            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 设置单元格字体颜色
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="colorIndex">颜色索引（IndexedColors）</param>
        public static ICell SetFontColor(this ICell cell, short colorIndex)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);

            var font = cell.Sheet.Workbook.CreateFont();
            if (cell.CellStyle != null && cell.CellStyle.GetFont(cell.Sheet.Workbook) != null)
            {
                font.CloneStyleFrom(cell.CellStyle.GetFont(cell.Sheet.Workbook));
            }
            font.Color = colorIndex;

            style.SetFont(font);
            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 设置单元格为粗体
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="isBold">是否粗体，默认为 true</param>
        public static ICell SetBold(this ICell cell, bool isBold = true)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);

            var font = cell.Sheet.Workbook.CreateFont();
            if (cell.CellStyle != null && cell.CellStyle.GetFont(cell.Sheet.Workbook) != null)
            {
                font.CloneStyleFrom(cell.CellStyle.GetFont(cell.Sheet.Workbook));
            }
            font.IsBold = isBold;

            style.SetFont(font);
            cell.CellStyle = style;

            return cell;
        }

        /// <summary>
        /// 设置单元格字体大小
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="fontSize">字体大小</param>
        public static ICell SetFontSize(this ICell cell, short fontSize)
        {
            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);

            var font = cell.Sheet.Workbook.CreateFont();
            if (cell.CellStyle != null && cell.CellStyle.GetFont(cell.Sheet.Workbook) != null)
            {
                font.CloneStyleFrom(cell.CellStyle.GetFont(cell.Sheet.Workbook));
            }
            font.FontHeightInPoints = fontSize;

            style.SetFont(font);
            cell.CellStyle = style;

            return cell;
        }
    }
}