using Azrng.NMaxCompute.Core;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class CsvResultParserTest
{
    [Fact]
    public void Parse_SimpleTwoColumns()
    {
        var csv = "a,b\n1,2\n3,4";

        var result = CsvResultParser.Parse(csv);

        Assert.Equal(2, result.Columns.Length);
        Assert.Equal("a", result.Columns[0]);
        Assert.Equal("b", result.Columns[1]);
        Assert.Equal(2, result.RowCount);
        Assert.Equal("1", result.Rows[0][0]);
        Assert.Equal("2", result.Rows[0][1]);
        Assert.Equal("4", result.Rows[1][1]);
    }

    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        var result = CsvResultParser.Parse("");
        Assert.Equal(0, result.Columns.Length);
        Assert.Equal(0, result.RowCount);
    }

    [Fact]
    public void Parse_HeaderOnly_ReturnsZeroRows()
    {
        var result = CsvResultParser.Parse("a,b,c");
        Assert.Equal(3, result.Columns.Length);
        Assert.Equal(0, result.RowCount);
    }

    [Fact]
    public void Parse_QuotedFieldWithComma()
    {
        var csv = "name,age\n\"hello,world\",10";

        var result = CsvResultParser.Parse(csv);

        Assert.Equal("hello,world", result.Rows[0][0]);
        Assert.Equal("10", result.Rows[0][1]);
    }

    [Fact]
    public void Parse_EscapedQuote()
    {
        var csv = "name\n\"hello \"\"world\"\"\"";

        var result = CsvResultParser.Parse(csv);

        Assert.Equal("hello \"world\"", result.Rows[0][0]);
    }

    [Fact]
    public void Parse_MaxRows_Truncates()
    {
        var csv = "a\n1\n2\n3\n4\n5";

        var result = CsvResultParser.Parse(csv, maxRows: 3);

        Assert.Equal(3, result.RowCount);
    }

    [Fact]
    public void Parse_CrlfLineEndings_TreatedAsOneBreak()
    {
        var csv = "a,b\r\n1,2\r\n3,4";

        var result = CsvResultParser.Parse(csv);

        Assert.Equal(2, result.RowCount);
        Assert.Equal("2", result.Rows[0][1]);
    }
}
