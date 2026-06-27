using Azrng.Office.NPOI;
using Azrng.Office.NPOI.Model;

namespace Azrng.Office.Npoi.Test;

public class ExcelHelperTests
{
    #region CreateWorkbook

    [Fact]
    public void CreateWorkbook_Xlsx_ReturnsWorkbookWrapper()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.NotNull(wrapper);
        Assert.NotNull(wrapper.Workbook);
        Assert.Equal("XSSFWorkbook", wrapper.Workbook.GetType().Name);
    }

    [Fact]
    public void CreateWorkbook_Xls_ReturnsWorkbookWrapper()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xls);

        Assert.NotNull(wrapper);
        Assert.NotNull(wrapper.Workbook);
        Assert.Equal("HSSFWorkbook", wrapper.Workbook.GetType().Name);
    }

    [Fact]
    public void CreateWorkbook_CreatesEmptyWorkbook()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Equal(0, wrapper.Workbook.NumberOfSheets);
    }

    #endregion

    #region CreateSheet

    [Fact]
    public void CreateSheet_ValidName_CreatesSheet()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var sheet = wrapper.CreateSheet("TestSheet");

        Assert.NotNull(sheet);
        Assert.NotNull(sheet.Sheet);
        Assert.Equal("TestSheet", sheet.Sheet.SheetName);
    }

    [Fact]
    public void CreateSheet_NullWorkbook_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((WorkbookWrapper)null!).CreateSheet("Test"));
    }

    [Fact]
    public void CreateSheet_NullName_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet(null!));
    }

    [Fact]
    public void CreateSheet_EmptyName_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet(""));
    }

    [Fact]
    public void CreateSheet_WhitespaceName_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet("   "));
    }

    [Fact]
    public void CreateSheet_NameExceedsMaxLength_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var longName = new string('A', 32);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet(longName));
    }

    [Fact]
    public void CreateSheet_NameAtMaxLength_Succeeds()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var name31 = new string('A', 31);

        var sheet = wrapper.CreateSheet(name31);

        Assert.NotNull(sheet);
        Assert.Equal(name31, sheet.Sheet.SheetName);
    }

    [Theory]
    [InlineData("Test\\Sheet")]
    [InlineData("Test/Sheet")]
    [InlineData("Test?Sheet")]
    [InlineData("Test*Sheet")]
    [InlineData("Test:Sheet")]
    [InlineData("Test[Sheet")]
    [InlineData("Test]Sheet")]
    public void CreateSheet_InvalidChars_ThrowsArgumentException(string invalidName)
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet(invalidName));
    }

    [Fact]
    public void CreateSheet_StartsWithSingleQuote_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet("'Test"));
    }

    [Fact]
    public void CreateSheet_EndsWithSingleQuote_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.CreateSheet("Test'"));
    }

    [Fact]
    public void CreateSheet_ChineseName_Succeeds()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        var sheet = wrapper.CreateSheet("数据表");

        Assert.NotNull(sheet);
        Assert.Equal("数据表", sheet.Sheet.SheetName);
    }

    [Fact]
    public void CreateSheet_MultipleSheets_Succeeds()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        wrapper.CreateSheet("Sheet1");
        wrapper.CreateSheet("Sheet2");
        wrapper.CreateSheet("Sheet3");

        Assert.Equal(3, wrapper.Workbook.NumberOfSheets);
    }

    #endregion

    #region WorkbookWrapper

    [Fact]
    public void WorkbookWrapper_ToBytes_ReturnsNonEmptyBytes()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        wrapper.CreateSheet("Test");

        var bytes = wrapper.ToBytes();

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void WorkbookWrapper_ToStream_ThrowsWhenWorkbookWriteClosesStream()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        wrapper.CreateSheet("Test");

        Assert.Throws<ObjectDisposedException>(() => wrapper.ToStream());
    }

    [Fact]
    public void WorkbookWrapper_Disposed_ThrowsObjectDisposed()
    {
        var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        wrapper.Dispose();

        Assert.Throws<ObjectDisposedException>(() => wrapper.ToBytes());
    }

    [Fact]
    public void WorkbookWrapper_SaveToFile_InvalidExtension_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");

        Assert.Throws<ArgumentException>(() => wrapper.SaveToFile(tempFile));
    }

    [Fact]
    public void WorkbookWrapper_SaveToFile_EmptyPath_ThrowsArgumentException()
    {
        using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);

        Assert.Throws<ArgumentException>(() => wrapper.SaveToFile(""));
    }

    [Fact]
    public void WorkbookWrapper_SaveToFile_Xlsx_CreatesFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.xlsx");
        try
        {
            using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
            wrapper.CreateSheet("Test");

            wrapper.SaveToFile(tempFile);

            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void WorkbookWrapper_SaveToFile_Xls_CreatesFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.xls");
        try
        {
            using var wrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xls);
            wrapper.CreateSheet("Test");

            wrapper.SaveToFile(tempFile);

            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    #endregion
}
