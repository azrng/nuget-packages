using Azrng.Core.Extension;
using Azrng.Office.NPOI.Model;
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
    /// <param name="workbook"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static SheetWrapper CreateSheet(this WorkbookWrapper workbook, string sheetName)
    {
        if (sheetName.IsNullOrWhiteSpace())
            throw new ArgumentException("Sheet name cannot be null or empty", nameof(sheetName));
        return new SheetWrapper(workbook.Workbook.CreateSheet(sheetName));
    }


}