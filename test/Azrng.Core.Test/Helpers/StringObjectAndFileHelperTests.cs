using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class StringObjectAndFileHelperTests
{
    [Fact]
    public void CompressText_ShouldCollapseWhitespaceAndTrim()
    {
        var input = "  hello   \r\n  world \n ";

        StringHelper.CompressText(input).Should().Be("hello world");
    }

    [Fact]
    public void UnicodeRoundTrip_ShouldRestoreOriginalText()
    {
        const string text = "Hello世界";

        var unicode = StringHelper.TextToUnicode(text);
        var restored = StringHelper.UnicodeToText(unicode);

        restored.Should().Be(text);
    }

    [Fact]
    public void ReplaceWithOrder_ShouldPreferLongerKeysFirst()
    {
        var input = "abc ab";
        var replacements = new Dictionary<string, string>
        {
            ["ab"] = "X",
            ["abc"] = "Y"
        };

        StringHelper.ReplaceWithOrder(input, replacements).Should().Be("Y X");
    }

    [Fact]
    public void ReplaceWithRegex_ShouldReplaceAllConfiguredTokens()
    {
        var input = "cat dog";
        var replacements = new Dictionary<string, string>
        {
            ["cat"] = "lion",
            ["dog"] = "wolf"
        };

        StringHelper.ReplaceWithRegex(input, replacements).Should().Be("lion wolf");
    }

    [Fact]
    public void GetStringPropertiesWithValues_ShouldFilterEmptyStringsByDefault()
    {
        var model = new SampleModel
        {
            Name = "Alice",
            Empty = "",
            Age = 30
        };

        var result = ObjectHelper.GetStringPropertiesWithValues(model);

        result.Should().ContainKey(nameof(SampleModel.Name)).WhoseValue.Should().Be("Alice");
        result.Should().NotContainKey(nameof(SampleModel.Empty));
        result.Should().NotContainKey(nameof(SampleModel.Age));
    }

    [Fact]
    public void GetStringPropertiesWithValues_ShouldIncludeEmptyStrings_WhenRequested()
    {
        var model = new SampleModel
        {
            Name = "Alice",
            Empty = ""
        };

        var result = ObjectHelper.GetStringPropertiesWithValues(model, includeEmpty: true);

        result.Should().ContainKey(nameof(SampleModel.Empty)).WhoseValue.Should().BeEmpty();
    }

    [Fact]
    public void ConvertToTargetType_ShouldHandleEnumAndNullableTargets()
    {
        ObjectHelper.ConvertToTargetType("Green", typeof(SampleColor)).Should().Be(SampleColor.Green);
        ObjectHelper.ConvertToTargetType("", typeof(int?)).Should().BeNull();
        ObjectHelper.ConvertToTargetType("12", typeof(int)).Should().Be(12);
    }

    [Fact]
    public void FormatFileSize_ShouldUseHumanReadableUnits()
    {
        FileHelper.FormatFileSize(512).Should().Be("512 B");
        FileHelper.FormatFileSize(2048).Should().Be("2.00 KB");
    }

    private sealed class SampleModel
    {
        public string Name { get; set; } = string.Empty;

        public string Empty { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    private enum SampleColor
    {
        Red = 1,
        Green = 2
    }
}
