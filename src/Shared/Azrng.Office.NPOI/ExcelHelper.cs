using Azrng.Core.Extension;
using Azrng.Office.NPOI.Model;
using Azrng.Office.NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Azrng.Office.NPOI;

/// <summary>
/// Excel操作帮助类
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// 创建工作簿
    /// </summary>
    /// <param name="ext"></param>
    /// <returns></returns>
    public static WorkbookWrapper CreateWorkbook(ExcelFileType ext)
    {
        return new WorkbookWrapper(ext);
    }

    /// <summary>
    /// 创建sheet
    /// </summary>
    /// <param name="workbook">工作簿</param>
    /// <param name="sheetName">工作表名称</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static SheetWrapper CreateSheet(this WorkbookWrapper workbook, string sheetName)
    {
        if (workbook == null)
            throw new ArgumentNullException(nameof(workbook));

        if (string.IsNullOrWhiteSpace(sheetName))
            throw new ArgumentException("Sheet name cannot be null or empty", nameof(sheetName));

        if (sheetName.Length > ExcelConstants.MaxSheetNameLength)
            throw new ArgumentException($"Sheet name cannot exceed {ExcelConstants.MaxSheetNameLength} characters", nameof(sheetName));

        if (sheetName.IndexOfAny(ExcelConstants.InvalidSheetNameChars) >= 0)
            throw new ArgumentException("Sheet name contains invalid characters", nameof(sheetName));

        // 验证工作表名称不以单引号开头或结尾
        if (sheetName.StartsWith("'") || sheetName.EndsWith("'"))
            throw new ArgumentException("Sheet name cannot start or end with a single quote", nameof(sheetName));

        return new SheetWrapper(workbook.Workbook.CreateSheet(sheetName));
    }


}