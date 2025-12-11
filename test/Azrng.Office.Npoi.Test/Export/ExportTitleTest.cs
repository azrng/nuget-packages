using Azrng.Core.Extension;
using Azrng.Office.NPOI;
using Azrng.Office.NPOI.Model;
using Azrng.Office.NPOI.Styles;
using Azrng.Office.Npoi.Test.Export.Dtos;
using Xunit.Abstractions;

namespace Azrng.Office.Npoi.Test.Export;

public class ExportTitleTest
{
    private readonly ITestOutputHelper _output;
    private readonly List<User> _users;

    public ExportTitleTest(ITestOutputHelper output)
    {
        _output = output;
        _users = Enumerable.Range(0, 10)
                           .Select(x => new User { Age = x, Hobby = "Hobby" + x, Name = "Name" + x, Sex = "Sex" + x })
                           .ToList();
    }

    /// <summary>
    /// 导出没有样式标题
    /// </summary>
    [Theory]
    [InlineData("sheet1", "标题", 0, 10, 0,
        14)]
    [InlineData("sheet2", "标题1", 5, 10, 2,
        18)]
    [InlineData("sheet3", "标题2", 10, 20, 4,
        20)]
    public void Export_NoneStyle_Title(string sheetName, string title, int startIndex, int endIndex, int rowIndex,
                                       int rowHeight)
    {
        var workbookWrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var sheet = workbookWrapper.CreateSheet(sheetName);

        sheet.AddTitle(title, startIndex, endIndex, rowHeight: rowHeight, rowIndex: rowIndex);

        var fileName = $"{DateTime.Now.GetTimestamp()}.xlsx";
        using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        workbookWrapper.WriteStream(outStream);
        _output.WriteLine(fileName);
    }

    /// <summary>
    /// 导出有样式标题
    /// </summary>
    [Theory]
    [InlineData("sheet1", "标题", 7, 0, 10,
        0, 14)]
    [InlineData("sheet2", "标题1", 8, 5, 10,
        2, 16)]
    [InlineData("sheet3", "标题2", 8, 10, 20,
        4, 18)]
    public void Export_Title(string sheetName, string title, short fontColor, int startIndex, int endIndex,
                             int rowIndex,
                             int rowHeight)
    {
        var workbookWrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var sheet = workbookWrapper.CreateSheet(sheetName);

        sheet.AddTitle(new ExportSheetTitle(title, rowHeight, new TitleCellStyle(fontColor: fontColor)), startIndex, endIndex,
            rowIndex: rowIndex);

        var fileName = $"{DateTime.Now.GetTimestamp()}.xlsx";
        using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        workbookWrapper.WriteStream(outStream);
        _output.WriteLine(fileName);
    }

    /// <summary>
    /// 导出标准标题
    /// </summary>
    [Fact]
    public void Export_Standard_Title()
    {
        var workbookWrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var sheet = workbookWrapper.CreateSheet("测试");

        sheet.AddTitle(new ExportSheetTitle("我是标题信息", 30, new TitleCellStyle(fontSize: 26, showAllBorder: true)), 0, 10,
            rowIndex: 0);

        var fileName = $"{DateTime.Now.GetTimestamp()}.xlsx";
        using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        workbookWrapper.WriteStream(outStream);
        _output.WriteLine(fileName);
    }

    /// <summary>
    /// 导出标准标题
    /// </summary>
    [Fact]
    public void Export_Multi_Title()
    {
        var workbookWrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var sheet = workbookWrapper.CreateSheet("测试");

        sheet.AddTitle(new ExportSheetTitle("我是标题信息", 30, new TitleCellStyle(fontSize: 26, showAllBorder: true)));
        sheet.AddTitle(new ExportSheetTitle("我是标题信息", 30, new TitleCellStyle(fontSize: 26, showAllBorder: true)), endIndex: 1);
        sheet.AddTitle(new ExportSheetTitle("我是标题信息", 30, new TitleCellStyle(fontSize: 26, showAllBorder: true)), endIndex: 2);

        _output.WriteLine($"下一行 行索引：{sheet.NextY}");

        var fileName = $"{DateTime.Now.GetTimestamp()}.xlsx";
        using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        workbookWrapper.WriteStream(outStream);
        _output.WriteLine(fileName);
    }
}