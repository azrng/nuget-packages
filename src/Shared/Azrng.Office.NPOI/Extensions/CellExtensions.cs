using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

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
        public static string GetValue(this ICell cell)
        {
            var ret = cell.CellType switch
            {
                CellType.Blank => string.Empty,
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Error => cell.ErrorCellValue.ToString(),
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                    ? $"{cell.DateCellValue:G}"
                    : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.String => cell.StringCellValue,
                CellType.Formula => GetFormulaCellValue(cell),
                _ => cell.ToString(),
            };

            return ret?.Trim() ?? string.Empty;

            static string GetFormulaCellValue(ICell cell)
            {
                try
                {
                    var da = cell.Sheet.Workbook.GetType().Name;
                    if (da == "HSSFWorkbook")
                    {
                        var e = new HSSFFormulaEvaluator(cell.Sheet.Workbook);
                        e.EvaluateInCell(cell);
                        return cell.ToString();
                    }
                    else
                    {
                        var e = new XSSFFormulaEvaluator(cell.Sheet.Workbook);
                        e.EvaluateInCell(cell);
                        return cell.ToString();
                    }
                }
                catch
                {
                    return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }
}