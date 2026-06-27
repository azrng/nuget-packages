using Azrng.Office.NPOI;
using System.Globalization;

namespace Azrng.Office.Npoi.Test;

public class ExcelConstantsTests
{
    [Fact]
    public void RowHeightMultiplier_Is20()
    {
        Assert.Equal(20, ExcelConstants.RowHeightMultiplier);
    }

    [Fact]
    public void MaxSheetNameLength_Is31()
    {
        Assert.Equal(31, ExcelConstants.MaxSheetNameLength);
    }

    [Fact]
    public void DefaultBatchSize_Is1000()
    {
        Assert.Equal(1000, ExcelConstants.DefaultBatchSize);
    }

    [Fact]
    public void DefaultFontSize_Is11()
    {
        Assert.Equal(11, ExcelConstants.DefaultFontSize);
    }

    [Fact]
    public void InvalidSheetNameChars_ContainsExpectedChars()
    {
        var expected = new[] { '\\', '/', '?', '*', ':', '[', ']' };

        Assert.Equal(expected, ExcelConstants.InvalidSheetNameChars);
    }

    [Fact]
    public void InvalidSheetNameChars_Has7Entries()
    {
        Assert.Equal(7, ExcelConstants.InvalidSheetNameChars.Length);
    }

    [Fact]
    public void ExcelCulture_InvariantCulture_IsInvariantCulture()
    {
        Assert.Equal(CultureInfo.InvariantCulture, ExcelCulture.InvariantCulture);
    }

    [Fact]
    public void InvalidSheetNameChars_IsReadOnly()
    {
        var chars1 = ExcelConstants.InvalidSheetNameChars;
        var chars2 = ExcelConstants.InvalidSheetNameChars;

        Assert.Same(chars1, chars2);
    }
}
