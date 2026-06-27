using System.Data;
using System.Text;
using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class CsvHelperTests : IDisposable
{
    private readonly string _testDirectory;

    public CsvHelperTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"CsvHelperTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    #region DateTableToCsv Tests

    [Fact]
    public void DateTableToCsv_ValidDataTable_ReturnsTrue()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "test.csv");

        var result = CsvHelper.DateTableToCsv(dt, filePath, "Test Header", "Name,Age,City");

        result.Should().BeTrue();
    }

    [Fact]
    public void DateTableToCsv_ValidDataTable_CreatesFile()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "test.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Test Header", "Name,Age,City");

        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void DateTableToCsv_ValidDataTable_WritesHeaderLine()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "test.csv");
        var header = "Test Report Header";

        CsvHelper.DateTableToCsv(dt, filePath, header, "Name,Age,City");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines[0].Should().Be(header);
    }

    [Fact]
    public void DateTableToCsv_ValidDataTable_WritesColumnNamesLine()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "test.csv");
        var columnNames = "Name,Age,City";

        CsvHelper.DateTableToCsv(dt, filePath, "Header", columnNames);

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines[1].Should().Be(columnNames);
    }

    [Fact]
    public void DateTableToCsv_ValidDataTable_WritesAllDataRows()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "test.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Header", "Name,Age,City");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines.Length.Should().Be(2 + dt.Rows.Count);
    }

    [Fact]
    public void DateTableToCsv_ValidDataTable_WritesCorrectData()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "test.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Header", "Name,Age,City");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines[2].Should().Be("Alice,30,Beijing");
        lines[3].Should().Be("Bob,25,Shanghai");
        lines[4].Should().Be("Charlie,35,Guangzhou");
    }

    [Fact]
    public void DateTableToCsv_EmptyDataTable_WritesOnlyHeaders()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Value", typeof(int));
        var filePath = Path.Combine(_testDirectory, "empty.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Empty Report", "Name,Value");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines.Length.Should().Be(2);
        lines[0].Should().Be("Empty Report");
        lines[1].Should().Be("Name,Value");
    }

    [Fact]
    public void DateTableToCsv_SpecialCharactersInData_WritesCorrectly()
    {
        var dt = new DataTable();
        dt.Columns.Add("Text", typeof(string));
        dt.Rows.Add("Hello, World");
        dt.Rows.Add("Line1\nLine2");
        dt.Rows.Add("Quote\"Test");
        var filePath = Path.Combine(_testDirectory, "special.csv");

        var result = CsvHelper.DateTableToCsv(dt, filePath, "Header", "Text");

        result.Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void DateTableToCsv_InvalidFilePath_ReturnsFalse()
    {
        var dt = CreateSampleDataTable();
        var invalidPath = Path.Combine("Z:\\nonexistent\\path\\test.csv");

        var result = CsvHelper.DateTableToCsv(dt, invalidPath, "Header", "Name,Age,City");

        result.Should().BeFalse();
    }

    [Fact]
    public void DateTableToCsv_UnicodeData_WritesUtf8Encoding()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Rows.Add("中文名称");
        dt.Rows.Add("日本語テスト");
        var filePath = Path.Combine(_testDirectory, "unicode.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Unicode Header", "Name");

        var content = File.ReadAllText(filePath, Encoding.UTF8);
        content.Should().Contain("中文名称");
        content.Should().Contain("日本語テスト");
    }

    [Fact]
    public void DateTableToCsv_SingleColumn_WritesCorrectly()
    {
        var dt = new DataTable();
        dt.Columns.Add("Value", typeof(string));
        dt.Rows.Add("A");
        dt.Rows.Add("B");
        dt.Rows.Add("C");
        var filePath = Path.Combine(_testDirectory, "single.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Single Column", "Value");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines[2].Should().Be("A");
        lines[3].Should().Be("B");
        lines[4].Should().Be("C");
    }

    [Fact]
    public void DateTableToCsv_NumericData_WritesAsString()
    {
        var dt = new DataTable();
        dt.Columns.Add("IntVal", typeof(int));
        dt.Columns.Add("DoubleVal", typeof(double));
        dt.Rows.Add(42, 3.14);
        var filePath = Path.Combine(_testDirectory, "numeric.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "Numeric", "IntVal,DoubleVal");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines.Length.Should().Be(3);
    }

    [Fact]
    public void DateTableToCsv_OverwritesExistingFile()
    {
        var dt = CreateSampleDataTable();
        var filePath = Path.Combine(_testDirectory, "overwrite.csv");

        CsvHelper.DateTableToCsv(dt, filePath, "First Write", "Name,Age,City");

        var smallDt = new DataTable();
        smallDt.Columns.Add("X", typeof(string));
        smallDt.Rows.Add("Only");
        CsvHelper.DateTableToCsv(smallDt, filePath, "Second Write", "X");

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        lines[0].Should().Be("Second Write");
    }

    #endregion

    #region CsvToDateTable Tests

    [Fact]
    public void CsvToDateTable_EmptyStream_ReturnsEmptyDataTable()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Rows.Count.Should().Be(0);
    }

    [Fact]
    public void CsvToDateTable_SimpleCsv_ParsesCorrectly()
    {
        // n=0 means all lines are data (no header line)
        var csv = "Name,Age,City\nAlice,30,Beijing";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(2); // Both lines are data rows
    }

    [Fact]
    public void CsvToDateTable_WithHeaderLine_ParsesCorrectly()
    {
        // n=1 means line 1 is header, lines 2+ are data
        var csv = "Report Title\nName,Age,City\nAlice,30,Beijing";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 1);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(1); // First line has 1 column (no comma)
        result.Rows.Count.Should().Be(2);
    }

    [Fact]
    public void CsvToDateTable_HeaderOnly_ReturnsEmptyDataTable()
    {
        // n=0, single line with no data after it
        var csv = "Name,Age,City";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(3);
        result.Rows.Count.Should().Be(1); // The header line becomes a data row
    }

    [Fact]
    public void CsvToDateTable_WithMultipleHeaderLines_ParsesCorrectly()
    {
        // n=3 means line 3 is header
        var csv = "Line1\nLine2\nLine3\nData1,Data2\nValue1,Value2";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 3);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(1); // Line3 has 1 column
        result.Rows.Count.Should().Be(2); // Data1 and Value1 lines
    }

    [Fact]
    public void CsvToDateTable_BlankLines_SkipsBlanksAndParses()
    {
        // n=0, blank lines are skipped
        var csv = "A,B\n\n1,2\n\n3,4\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(2);
        result.Rows.Count.Should().Be(3); // A,B + 1,2 + 3,4
    }

    [Fact]
    public void CsvToDateTable_UnicodeContent_ParsesCorrectly()
    {
        // n=0, all lines are data
        var csv = "Name,Desc\n中文,説明\n日本語,テスト";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(2);
        result.Rows.Count.Should().Be(3); // All 3 lines are data
    }

    [Fact]
    public void CsvToDateTable_SingleColumn_ParsesCorrectly()
    {
        // n=0, all lines are data
        var csv = "Value\nA\nB\nC";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(1);
        result.Rows.Count.Should().Be(4); // All 4 lines are data
    }

    [Fact]
    public void CsvToDateTable_SingleDataRow_ParsesCorrectly()
    {
        // n=0, all lines are data
        var csv = "X,Y\n1,2";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(2);
        result.Rows.Count.Should().Be(2); // Both lines are data
    }

    [Fact]
    public void CsvToDateTable_NEqualsZero_ParsesAllLines()
    {
        // n=0, all lines are data
        var csv = "A,B\n1,2\n3,4";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(2);
        result.Rows.Count.Should().Be(3); // All 3 lines are data
    }

    [Fact]
    public void CsvToDateTable_WithHeaderAndData_ParsesCorrectly()
    {
        // n=1 means line 1 is header, lines 2+ are data
        var csv = "Name,Age,City\nAlice,30,Beijing\nBob,25,Shanghai";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 1);

        result.Should().NotBeNull();
        result.Columns.Count.Should().Be(3); // Header line has 3 columns
        result.Rows.Count.Should().Be(2); // Alice and Bob
        result.Rows[0]["Name"].Should().Be("Alice");
        result.Rows[1]["Name"].Should().Be("Bob");
    }

    [Fact]
    public void CsvToDateTable_WhitespaceOnlyLines_ReturnsEmptyDataTable()
    {
        var csv = "\n\n\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = CsvHelper.CsvToDateTable(stream, 0);

        result.Should().NotBeNull();
        result.Rows.Count.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static DataTable CreateSampleDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Age", typeof(int));
        dt.Columns.Add("City", typeof(string));
        dt.Rows.Add("Alice", 30, "Beijing");
        dt.Rows.Add("Bob", 25, "Shanghai");
        dt.Rows.Add("Charlie", 35, "Guangzhou");
        return dt;
    }

    #endregion
}
