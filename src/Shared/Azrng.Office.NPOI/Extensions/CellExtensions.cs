using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Azrng.Office.NPOI;

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
    }
}