using Azrng.Office.NPOI;
using Azrng.Office.NPOI.Extensions;
using Azrng.Office.NPOI.Model;
using Azrng.Office.Npoi.Test.Import.Dtos;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Azrng.Office.Npoi.Test;

public class ExcelImporterTests
{
    private static byte[] CreateXlsxBytes(Action<IWorkbook>? configure = null)
    {
        var workbook = new XSSFWorkbook();
        configure?.Invoke(workbook);
        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    private static byte[] CreateXlsBytes(Action<IWorkbook>? configure = null)
    {
        var workbook = new HSSFWorkbook();
        configure?.Invoke(workbook);
        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    private static void AddHeaderAndDataRows(ISheet sheet, string[] headers, string[][] data)
    {
        var headerRow = sheet.CreateRow(0);
        for (int i = 0; i < headers.Length; i++)
            headerRow.CreateCell(i).SetCellValue(headers[i]);

        for (int r = 0; r < data.Length; r++)
        {
            var row = sheet.CreateRow(r + 1);
            for (int c = 0; c < data[r].Length; c++)
                row.CreateCell(c).SetCellValue(data[r][c]);
        }
    }

    #region ImportFromBytes - basic

    [Fact]
    public void ImportFromBytes_WithHeader_ReturnsCorrectData()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "Email", "Salary", "IsActive"],
                [
                    ["Alice", "alice@test.com", "5000.5", "true"],
                    ["Bob", "bob@test.com", "6000", "false"]
                ]);
        });

        var result = ExcelImporter.ImportFromBytes<TestPerson>(bytes);

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("alice@test.com", result[0].Email);
        Assert.Equal(5000.5m, result[0].Salary);
        Assert.True(result[0].IsActive);
        Assert.Equal("Bob", result[1].Name);
        Assert.False(result[1].IsActive);
    }

    [Fact]
    public void ImportFromBytes_WithColumnNameAttribute_MatchesHeaderByAttributeName()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["姓名", "年龄"],
                [["张三", "28"]]);
        });

        var result = ExcelImporter.ImportFromBytes<TestPersonWithColumnName>(bytes);

        Assert.Single(result);
        Assert.Equal("张三", result[0].Name);
        Assert.Equal(28, result[0].Age);
    }

    [Fact]
    public void ImportFromBytes_WithoutHeader_MapsByIndex()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("Alice");
            row.CreateCell(1).SetCellValue("alice@test.com");
            row.CreateCell(2).SetCellValue("5000");
            row.CreateCell(3).SetCellValue("true");
        });

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes, hasHeader: false);

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Column1);
        Assert.Equal("alice@test.com", result[0].Column2);
    }

    [Fact]
    public void ImportFromBytes_EmptySheet_ReturnsEmptyList()
    {
        var bytes = CreateXlsxBytes(wb => wb.CreateSheet());

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes);

        Assert.Empty(result);
    }

    [Fact]
    public void ImportFromBytes_NullBytes_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ExcelImporter.ImportFromBytes<SimpleItem>(null!));
    }

    [Fact]
    public void ImportFromBytes_EmptyBytes_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ExcelImporter.ImportFromBytes<SimpleItem>(Array.Empty<byte>()));
    }

    #endregion

    #region ImportFromBytes - sheet index

    [Fact]
    public void ImportFromBytes_SpecificSheetIndex_ReadsCorrectSheet()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet0 = wb.CreateSheet("Sheet0");
            AddHeaderAndDataRows(sheet0, ["Column1"], [["Sheet0Data"]]);

            var sheet1 = wb.CreateSheet("Sheet1");
            AddHeaderAndDataRows(sheet1, ["Column1"], [["Sheet1Data"]]);
        });

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes, sheetIndex: 1);

        Assert.Single(result);
        Assert.Equal("Sheet1Data", result[0].Column1);
    }

    [Fact]
    public void ImportFromBytes_InvalidSheetIndex_ThrowsArgumentOutOfRange()
    {
        var bytes = CreateXlsxBytes(wb => wb.CreateSheet());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExcelImporter.ImportFromBytes<SimpleItem>(bytes, sheetIndex: 5));
    }

    #endregion

    #region ImportFromBytes - type conversion

    [Fact]
    public void ImportFromBytes_NumericTypes_ConvertsCorrectly()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "Age", "Salary"],
                [["Test", "25", "1234.56"]]);
        });

        var result = ExcelImporter.ImportFromBytes<TestPerson>(bytes);

        Assert.Single(result);
        Assert.Equal(25, result[0].Age);
        Assert.Equal(1234.56m, result[0].Salary);
    }

    [Fact]
    public void ImportFromBytes_BoolFromOneZero_ConvertsCorrectly()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("IsActive");

            var row1 = sheet.CreateRow(1);
            row1.CreateCell(0).SetCellValue("1");

            var row2 = sheet.CreateRow(2);
            row2.CreateCell(0).SetCellValue("0");
        });

        var result = ExcelImporter.ImportFromBytes<TestPerson>(bytes);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsActive);
        Assert.False(result[1].IsActive);
    }

    [Fact]
    public void ImportFromBytes_NullableTypes_ConvertsCorrectly()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "Age", "Amount", "IsDeleted", "UniqueId"],
                [
                    ["Alice", "30", "99.99", "true", "12345678-1234-1234-1234-123456789abc"],
                    ["Bob", "", "", "", ""]
                ]);
        });

        var result = ExcelImporter.ImportFromBytes<NullableItem>(bytes);

        Assert.Equal(2, result.Count);

        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(30, result[0].Age);
        Assert.Equal(99.99m, result[0].Amount);
        Assert.True(result[0].IsDeleted);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789abc"), result[0].UniqueId);

        Assert.Equal("Bob", result[1].Name);
        Assert.Null(result[1].Age);
        Assert.Null(result[1].Amount);
    }

    [Fact]
    public void ImportFromBytes_DateTime_ConvertsCorrectly()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "BirthDate"],
                [["Alice", "2000-01-15"]]);
        });

        var result = ExcelImporter.ImportFromBytes<NullableItem>(bytes);

        Assert.Single(result);
        Assert.Equal(new DateTime(2000, 1, 15), result[0].BirthDate);
    }

    #endregion

    #region ImportFromBytes - edge cases

    [Fact]
    public void ImportFromBytes_SkipsEmptyRows()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("Column1");
            header.CreateCell(1).SetCellValue("Column2");

            var row1 = sheet.CreateRow(1);
            row1.CreateCell(0).SetCellValue("Data1");
            row1.CreateCell(1).SetCellValue("Data2");

            sheet.CreateRow(2);

            var row3 = sheet.CreateRow(3);
            row3.CreateCell(0).SetCellValue("Data3");
            row3.CreateCell(1).SetCellValue("Data4");
        });

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes);

        Assert.Equal(2, result.Count);
        Assert.Equal("Data1", result[0].Column1);
        Assert.Equal("Data3", result[1].Column1);
    }

    [Fact]
    public void ImportFromBytes_SkipsIgnoredColumns()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "Age", "Email", "Salary", "IsActive", "Secret"],
                [["Alice", "30", "a@b.com", "100", "true", "hidden"]]);
        });

        var result = ExcelImporter.ImportFromBytes<TestPerson>(bytes);

        Assert.Single(result);
        Assert.Equal(string.Empty, result[0].Secret);
    }

    [Fact]
    public void ImportFromBytes_InvalidValue_SkipsGracefully()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "Age", "Email"],
                [["Alice", "notanumber", "a@b.com"]]);
        });

        var result = ExcelImporter.ImportFromBytes<TestPerson>(bytes);

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("a@b.com", result[0].Email);
        Assert.Equal(0, result[0].Age);
    }

    [Fact]
    public void ImportFromBytes_UnmatchedHeader_SkipsProperty()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet,
                ["Name", "Email"],
                [["Alice", "a@b.com"]]);
        });

        var result = ExcelImporter.ImportFromBytes<TestPerson>(bytes);

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("a@b.com", result[0].Email);
        Assert.Equal(0, result[0].Age);
        Assert.Equal(0m, result[0].Salary);
    }

    #endregion

    #region ImportFromStream

    [Fact]
    public void ImportFromStream_ValidXlsxStream_ReturnsData()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet, ["Column1", "Column2"], [["A", "B"]]);
        });

        using var ms = new MemoryStream(bytes);
        var result = ExcelImporter.ImportFromStream<SimpleItem>(ms);

        Assert.Single(result);
        Assert.Equal("A", result[0].Column1);
        Assert.Equal("B", result[0].Column2);
    }

    [Fact]
    public void ImportFromStream_NullStream_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ExcelImporter.ImportFromStream<SimpleItem>(null!));
    }

    [Fact]
    public void ImportFromStream_InvalidStream_ThrowsInvalidOperation()
    {
        var invalidBytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var ms = new MemoryStream(invalidBytes);

        Assert.Throws<InvalidOperationException>(() =>
            ExcelImporter.ImportFromStream<SimpleItem>(ms));
    }

    #endregion

    #region ImportFromFile

    [Fact]
    public void ImportFromFile_ValidXlsx_ReturnsData()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.xlsx");
        try
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet();
            AddHeaderAndDataRows(sheet, ["Column1"], [["Hello"]]);
            using (var fs = new FileStream(tempFile, FileMode.Create))
                workbook.Write(fs);
            workbook.Close();

            var result = ExcelImporter.ImportFromFile<SimpleItem>(tempFile);

            Assert.Single(result);
            Assert.Equal("Hello", result[0].Column1);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ImportFromFile_NullPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ExcelImporter.ImportFromFile<SimpleItem>(null!));
    }

    [Fact]
    public void ImportFromFile_EmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ExcelImporter.ImportFromFile<SimpleItem>(""));
    }

    [Fact]
    public void ImportFromFile_FileNotFound_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            ExcelImporter.ImportFromFile<SimpleItem>("C:\\nonexistent\\file.xlsx"));
    }

    [Fact]
    public void ImportFromFile_InvalidExtension_ThrowsArgumentException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        File.WriteAllText(tempFile, "dummy");
        try
        {
            Assert.Throws<ArgumentException>(() =>
                ExcelImporter.ImportFromFile<SimpleItem>(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region ImportFromFile - xls format

    [Fact]
    public void ImportFromBytes_XlsFormat_ReturnsData()
    {
        var bytes = CreateXlsBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            AddHeaderAndDataRows(sheet, ["Column1", "Column2"], [["A", "B"]]);
        });

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes);

        Assert.Single(result);
        Assert.Equal("A", result[0].Column1);
        Assert.Equal("B", result[0].Column2);
    }

    #endregion

    #region ImportFromFileAsync

    [Fact]
    public async Task ImportFromFileAsync_ValidXlsx_ReturnsData()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.xlsx");
        try
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet();
            AddHeaderAndDataRows(sheet, ["Column1"], [["AsyncData"]]);
            using (var fs = new FileStream(tempFile, FileMode.Create))
                workbook.Write(fs);
            workbook.Close();

            var result = await ExcelImporter.ImportFromFileAsync<SimpleItem>(tempFile);

            Assert.Single(result);
            Assert.Equal("AsyncData", result[0].Column1);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    #endregion

    #region ImportFromBytes - startRow

    [Fact]
    public void ImportFromBytes_CustomStartRow_ReadsFromCorrectRow()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet = wb.CreateSheet();
            sheet.CreateRow(0).CreateCell(0).SetCellValue("skip");
            var header = sheet.CreateRow(1);
            header.CreateCell(0).SetCellValue("Column1");
            header.CreateCell(1).SetCellValue("Column2");
            var data = sheet.CreateRow(2);
            data.CreateCell(0).SetCellValue("Value1");
            data.CreateCell(1).SetCellValue("Value2");
        });

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes, startRow: 1);

        Assert.Single(result);
        Assert.Equal("Value1", result[0].Column1);
    }

    #endregion

    #region ExcelImportResult

    [Fact]
    public void ExcelImportResult_DefaultValues_AreCorrect()
    {
        var result = new ExcelImporter.ExcelImportResult<SimpleItem>();

        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
        Assert.False(result.HasErrors);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void ImportError_DefaultValues_AreCorrect()
    {
        var error = new ExcelImporter.ImportError();

        Assert.Equal(0, error.RowNumber);
        Assert.Null(error.ColumnName);
        Assert.Equal(string.Empty, error.ErrorMessage);
        Assert.Null(error.OriginalValue);
    }

    #endregion

    #region ImportFromBytes - multiple sheets

    [Fact]
    public void ImportFromBytes_FirstSheetByDefault()
    {
        var bytes = CreateXlsxBytes(wb =>
        {
            var sheet0 = wb.CreateSheet("First");
            AddHeaderAndDataRows(sheet0, ["Column1"], [["FirstData"]]);

            var sheet1 = wb.CreateSheet("Second");
            AddHeaderAndDataRows(sheet1, ["Column1"], [["SecondData"]]);
        });

        var result = ExcelImporter.ImportFromBytes<SimpleItem>(bytes);

        Assert.Single(result);
        Assert.Equal("FirstData", result[0].Column1);
    }

    #endregion
}
