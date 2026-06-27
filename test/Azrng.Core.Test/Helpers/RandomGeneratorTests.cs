using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class RandomGeneratorTests
{
    #region GenerateNumber() - 无参版本

    [Fact]
    public void GenerateNumber_ReturnsString()
    {
        var result = RandomGenerator.GenerateNumber();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateNumber_ReturnsExpectedLength()
    {
        var result = RandomGenerator.GenerateNumber();
        result.Length.Should().Be(18);
    }

    [Fact]
    public void GenerateNumber_StartsWithCurrentDate()
    {
        var result = RandomGenerator.GenerateNumber();
        var expectedPrefix = DateTime.Now.ToString("yyyyMMdd");
        result.Substring(0, 8).Should().Be(expectedPrefix);
    }

    [Fact]
    public void GenerateNumber_ContainsOnlyDigits()
    {
        var result = RandomGenerator.GenerateNumber();
        result.Should().MatchRegex(@"^\d{18}$");
    }

    #endregion

    #region GenerateNumber(int codeNum)

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(20)]
    public void GenerateNumber_WithLength_ReturnsCorrectLength(int length)
    {
        var result = RandomGenerator.GenerateNumber(length);
        result.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateNumber_WithLength_ContainsOnlyDigits()
    {
        var result = RandomGenerator.GenerateNumber(100);
        result.Should().MatchRegex(@"^\d+$");
    }

    [Fact]
    public void GenerateNumber_ZeroLength_ReturnsEmpty()
    {
        var result = RandomGenerator.GenerateNumber(0);
        result.Should().BeEmpty();
    }

    #endregion

    #region GenerateDoubleNumber()

    [Fact]
    public void GenerateDoubleNumber_ReturnsBetweenZeroAndOne()
    {
        var result = RandomGenerator.GenerateDoubleNumber();
        result.Should().BeGreaterThanOrEqualTo(0.0);
        result.Should().BeLessThan(1.0);
    }

    [Fact]
    public void GenerateDoubleNumber_ReturnsDifferentValues()
    {
        var results = Enumerable.Range(0, 10)
            .Select(_ => RandomGenerator.GenerateDoubleNumber())
            .ToList();

        results.Distinct().Count().Should().BeGreaterThan(1);
    }

    #endregion

    #region GenerateNumber(double start, double end)

    [Fact]
    public void GenerateNumber_DoubleRange_ReturnsValueInRange()
    {
        var result = RandomGenerator.GenerateNumber(1.0, 10.0);
        result.Should().BeGreaterThanOrEqualTo(1.0);
        result.Should().BeLessThan(10.0);
    }

    [Fact]
    public void GenerateNumber_DoubleRange_ThrowsWhenStartEqualsEnd()
    {
        Action act = () => RandomGenerator.GenerateNumber(5.0, 5.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateNumber_DoubleRange_ThrowsWhenStartGreaterThanEnd()
    {
        Action act = () => RandomGenerator.GenerateNumber(10.0, 5.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateNumber_DoubleRange_NegativeRange()
    {
        var result = RandomGenerator.GenerateNumber(-10.0, -1.0);
        result.Should().BeGreaterThanOrEqualTo(-10.0);
        result.Should().BeLessThan(-1.0);
    }

    #endregion

    #region GenerateString(int stringLength)

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(50)]
    public void GenerateString_ReturnsCorrectLength(int length)
    {
        var result = RandomGenerator.GenerateString(length);
        result.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateString_DefaultLength_Returns6()
    {
        var result = RandomGenerator.GenerateString();
        result.Length.Should().Be(6);
    }

    [Fact]
    public void GenerateString_ContainsValidCharacters()
    {
        var validChars = "0123456789ABCDEFGHIJKMLNOPQRSTUVWXYZ";
        var result = RandomGenerator.GenerateString(100);
        result.All(c => validChars.Contains(c)).Should().BeTrue();
    }

    #endregion

    #region GenerateNumber(int start, int end)

    [Fact]
    public void GenerateNumber_IntRange_ReturnsValueInRange()
    {
        var result = RandomGenerator.GenerateNumber(1, 100);
        result.Should().BeGreaterThanOrEqualTo(1);
        result.Should().BeLessThan(100);
    }

    [Fact]
    public void GenerateNumber_IntRange_ThrowsWhenStartEqualsEnd()
    {
        Action act = () => RandomGenerator.GenerateNumber(5, 5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateNumber_IntRange_ThrowsWhenStartGreaterThanEnd()
    {
        Action act = () => RandomGenerator.GenerateNumber(10, 5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateNumber_IntRange_NegativeRange()
    {
        var result = RandomGenerator.GenerateNumber(-100, -1);
        result.Should().BeGreaterThanOrEqualTo(-100);
        result.Should().BeLessThan(-1);
    }

    #endregion

    #region GenerateDateTime(DateTime startDateTime, DateTime endDateTime)

    [Fact]
    public void GenerateDateTime_ReturnsDateInRange()
    {
        var start = new DateTime(2020, 1, 1);
        var end = new DateTime(2023, 12, 31);

        var result = RandomGenerator.GenerateDateTime(start, end);

        result.Should().BeOnOrAfter(start);
        result.Should().BeOnOrBefore(end);
    }

    [Fact]
    public void GenerateDateTime_SameDate_ReturnsSameDate()
    {
        var date = new DateTime(2023, 6, 15);
        var result = RandomGenerator.GenerateDateTime(date, date);
        result.Should().Be(date);
    }

    [Fact]
    public void GenerateDateTime_StartAfterEnd_StillReturnsInRange()
    {
        var start = new DateTime(2023, 12, 31);
        var end = new DateTime(2020, 1, 1);

        var result = RandomGenerator.GenerateDateTime(start, end);

        var min = end < start ? end : start;
        var max = end > start ? end : start;
        result.Should().BeOnOrAfter(min);
        result.Should().BeOnOrBefore(max);
    }

    #endregion

    #region GenerateName(int gender)

    [Fact]
    public void GenerateName_Default_ReturnsNonEmptyString()
    {
        var result = RandomGenerator.GenerateName();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateName_Default_ReturnsTwoCharacters()
    {
        var result = RandomGenerator.GenerateName();
        result.Length.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GenerateName_WithGender_ReturnsNonEmptyString(int gender)
    {
        var result = RandomGenerator.GenerateName(gender);
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);
    }

    #endregion

    #region GenerateNameBatch(int count, int gender)

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void GenerateNameBatch_ReturnsCorrectCount(int count)
    {
        var result = RandomGenerator.GenerateNameBatch(count).ToList();
        result.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateNameBatch_AllNamesHaveLength2()
    {
        var result = RandomGenerator.GenerateNameBatch(20).ToList();
        result.Should().AllSatisfy(name => name.Length.Should().Be(2));
    }

    [Fact]
    public void GenerateNameBatch_WithGender_ReturnsCorrectCount()
    {
        var result = RandomGenerator.GenerateNameBatch(5, 1).ToList();
        result.Should().HaveCount(5);
    }

    [Fact]
    public void GenerateNameBatch_ZeroCount_ReturnsEmpty()
    {
        var result = RandomGenerator.GenerateNameBatch(0).ToList();
        result.Should().BeEmpty();
    }

    #endregion

    #region GenerateIdCard(int gender, int minAge, int maxAge)

    [Fact]
    public void GenerateIdCard_Returns18Characters()
    {
        var result = RandomGenerator.GenerateIdCard();
        result.Length.Should().Be(18);
    }

    [Fact]
    public void GenerateIdCard_ContainsOnlyDigitsAndX()
    {
        var result = RandomGenerator.GenerateIdCard();
        result.Should().MatchRegex(@"^[\dX]{18}$");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GenerateIdCard_WithGender_ReturnsValidFormat(int gender)
    {
        var result = RandomGenerator.GenerateIdCard(gender);
        result.Length.Should().Be(18);
        result.Should().MatchRegex(@"^[\dX]{18}$");
    }

    [Fact]
    public void GenerateIdCard_Male_HasOddSequenceDigit()
    {
        var result = RandomGenerator.GenerateIdCard(1);
        var genderDigit = int.Parse(result[16].ToString());
        (genderDigit % 2).Should().Be(1);
    }

    [Fact]
    public void GenerateIdCard_Female_HasEvenSequenceDigit()
    {
        var result = RandomGenerator.GenerateIdCard(2);
        var genderDigit = int.Parse(result[16].ToString());
        (genderDigit % 2).Should().Be(0);
    }

    [Fact]
    public void GenerateIdCard_WithAgeRange_ReturnsValidFormat()
    {
        var result = RandomGenerator.GenerateIdCard(0, 20, 30);
        result.Length.Should().Be(18);
    }

    #endregion

    #region GeneratePhoneNumber()

    [Fact]
    public void GeneratePhoneNumber_Returns11Digits()
    {
        var result = RandomGenerator.GeneratePhoneNumber();
        result.Length.Should().Be(11);
    }

    [Fact]
    public void GeneratePhoneNumber_StartsWithValidPrefix()
    {
        var validPrefixes = new[]
        {
            "130", "131", "132", "133", "134", "135", "136", "137", "138", "139",
            "150", "151", "152", "153", "155", "156", "157", "158", "159", "147",
            "180", "181", "182", "183", "184", "187", "188", "178", "198",
            "145", "166", "167", "171", "175", "176", "185", "186",
            "149", "173", "177", "189", "199"
        };

        var result = RandomGenerator.GeneratePhoneNumber();
        var prefix = result.Substring(0, 3);
        validPrefixes.Should().Contain(prefix);
    }

    [Fact]
    public void GeneratePhoneNumber_ContainsOnlyDigits()
    {
        var result = RandomGenerator.GeneratePhoneNumber();
        result.Should().MatchRegex(@"^\d{11}$");
    }

    #endregion

    #region GenerateAddress()

    [Fact]
    public void GenerateAddress_ReturnsNonEmptyString()
    {
        var result = RandomGenerator.GenerateAddress();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAddress_ContainsNumberAndUnit()
    {
        var result = RandomGenerator.GenerateAddress();
        result.Should().Contain("号");
        result.Should().Contain("单元");
        result.Should().Contain("室");
    }

    #endregion

    #region GenerateBankCardNumber()

    [Fact]
    public void GenerateBankCardNumber_ReturnsNonEmptyString()
    {
        var result = RandomGenerator.GenerateBankCardNumber();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateBankCardNumber_Returns18Digits()
    {
        var result = RandomGenerator.GenerateBankCardNumber();
        result.Length.Should().Be(18);
    }

    [Fact]
    public void GenerateBankCardNumber_ContainsOnlyDigits()
    {
        var result = RandomGenerator.GenerateBankCardNumber();
        result.Should().MatchRegex(@"^\d+$");
    }

    [Fact]
    public void GenerateBankCardNumber_StartsWithValidBin()
    {
        var validBins = new[]
        {
            "622202", "622848", "622700", "622188",
            "436742", "622588", "622609", "622280",
            "622155", "622156", "622157", "622158",
            "622581", "622582", "622583", "622584",
            "622259", "622258", "622257", "622256",
            "622521", "622522", "622523", "622524",
            "622901", "622902", "622903", "622904"
        };

        var result = RandomGenerator.GenerateBankCardNumber();
        var bin = result.Substring(0, 6);
        validBins.Should().Contain(bin);
    }

    #endregion

    #region GenerateEmail(string? name)

    [Fact]
    public void GenerateEmail_WithoutName_ReturnsValidEmail()
    {
        var result = RandomGenerator.GenerateEmail();
        result.Should().Contain("@");
        result.Should().MatchRegex(@"@.+\..+");
    }

    [Fact]
    public void GenerateEmail_WithName_ContainsName()
    {
        var result = RandomGenerator.GenerateEmail("TestUser");
        result.Should().StartWith("testuser@");
    }

    [Fact]
    public void GenerateEmail_WithName_ConvertsToLowercase()
    {
        var result = RandomGenerator.GenerateEmail("ABC");
        result.Should().StartWith("abc@");
    }

    [Fact]
    public void GenerateEmail_NullName_GeneratesRandomUsername()
    {
        var result = RandomGenerator.GenerateEmail(null);
        result.Should().Contain("@");
    }

    [Fact]
    public void GenerateEmail_EmptyName_GeneratesRandomUsername()
    {
        var result = RandomGenerator.GenerateEmail("");
        result.Should().Contain("@");
    }

    [Fact]
    public void GenerateEmail_WithValidDomain()
    {
        var validDomains = new[]
        {
            "qq.com", "163.com", "126.com", "gmail.com", "sina.com",
            "sohu.com", "hotmail.com", "yahoo.com", "outlook.com", "foxmail.com"
        };

        var result = RandomGenerator.GenerateEmail();
        var domain = result.Split('@')[1];
        validDomains.Should().Contain(domain);
    }

    #endregion

    #region GenerateArray<T>(T[] arr)

    [Fact]
    public void GenerateArray_ShufflesArray()
    {
        var arr = Enumerable.Range(1, 100).ToArray();
        var original = arr.ToArray();

        RandomGenerator.GenerateArray(arr);

        arr.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void GenerateArray_SingleElement_NoChange()
    {
        var arr = new[] { 42 };
        RandomGenerator.GenerateArray(arr);
        arr[0].Should().Be(42);
    }

    [Fact]
    public void GenerateArray_EmptyArray_NoError()
    {
        var arr = Array.Empty<int>();
        Action act = () => RandomGenerator.GenerateArray(arr);
        act.Should().NotThrow();
    }

    #endregion

    #region GenerateVerifyCode(int length, bool hasNumber, bool hasUpper, bool hasLowercase, bool hasNonAlphanumeric)

    [Fact]
    public void GenerateVerifyCode_DefaultParams_ReturnsDigitsOnly()
    {
        var result = RandomGenerator.GenerateVerifyCode(6);
        result.Length.Should().Be(6);
        result.Should().MatchRegex(@"^\d+$");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    public void GenerateVerifyCode_WithLength_ReturnsCorrectLength(int length)
    {
        var result = RandomGenerator.GenerateVerifyCode(length);
        result.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateVerifyCode_LengthBelow4_ReturnsAtLeast4()
    {
        var result = RandomGenerator.GenerateVerifyCode(2);
        result.Length.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void GenerateVerifyCode_WithNumbers_ContainsDigits()
    {
        var result = RandomGenerator.GenerateVerifyCode(20, hasNumber: true);
        result.Should().MatchRegex(@"\d");
    }

    [Fact]
    public void GenerateVerifyCode_WithUpper_ContainsUppercase()
    {
        var result = RandomGenerator.GenerateVerifyCode(20, hasNumber: false, hasUpper: true);
        result.Should().MatchRegex(@"[A-Z]");
    }

    [Fact]
    public void GenerateVerifyCode_WithLower_ContainsLowercase()
    {
        var result = RandomGenerator.GenerateVerifyCode(20, hasNumber: false, hasLowercase: true);
        result.Should().MatchRegex(@"[a-z]");
    }

    [Fact]
    public void GenerateVerifyCode_WithSpecialChars_ContainsSpecialChars()
    {
        var result = RandomGenerator.GenerateVerifyCode(20, hasNumber: false, hasNonAlphanumeric: true);
        result.Should().MatchRegex(@"[!""#$%&'()*+,\-./:;<=>?@\[\\\]^_`{|}~]");
    }

    [Fact]
    public void GenerateVerifyCode_AllTypes_ContainsAllCharacterTypes()
    {
        var result = RandomGenerator.GenerateVerifyCode(40, hasNumber: true, hasUpper: true, hasLowercase: true, hasNonAlphanumeric: true);
        result.Should().MatchRegex(@"\d");
        result.Should().MatchRegex(@"[A-Z]");
        result.Should().MatchRegex(@"[a-z]");
        result.Should().MatchRegex(@"[!""#$%&'()*+,\-./:;<=>?@\[\\\]^_`{|}~]");
    }

    [Fact]
    public void GenerateVerifyCode_AllFalse_DefaultsToNumbers()
    {
        var result = RandomGenerator.GenerateVerifyCode(8, hasNumber: false, hasUpper: false, hasLowercase: false, hasNonAlphanumeric: false);
        result.Length.Should().Be(8);
        result.Should().MatchRegex(@"^\d+$");
    }

    #endregion
}
