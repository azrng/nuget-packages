using FluentAssertions;
using System.Text;

namespace Azrng.Security.Test
{
    public class MurmurHashHelperTests
    {
        [Fact]
        public void MakeHash64BValue_SameInput_ReturnsConsistentResult()
        {
            var key = Encoding.UTF8.GetBytes("test-key");

            var result1 = MurmurHashHelper.MakeHash64BValue(key);
            var result2 = MurmurHashHelper.MakeHash64BValue(key);

            result1.Should().Be(result2);
        }

        [Fact]
        public void MakeHash64BValue_DifferentInputs_ReturnsDifferentResults()
        {
            var key1 = Encoding.UTF8.GetBytes("abc");
            var key2 = Encoding.UTF8.GetBytes("def");

            var result1 = MurmurHashHelper.MakeHash64BValue(key1);
            var result2 = MurmurHashHelper.MakeHash64BValue(key2);

            result1.Should().NotBe(result2);
        }

        [Fact]
        public void MakeHash64BValue_CustomSeed_ReturnsDifferentResult()
        {
            var key = Encoding.UTF8.GetBytes("seed-test");

            var defaultResult = MurmurHashHelper.MakeHash64BValue(key);
            var customResult = MurmurHashHelper.MakeHash64BValue(key, seed: 12345);

            defaultResult.Should().NotBe(customResult);
        }

        [Fact]
        public void MakeHash64BValue_EmptyArray_ReturnsValue()
        {
            var result = MurmurHashHelper.MakeHash64BValue(Array.Empty<byte>());

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void MakeHash64BValue_SingleByte_ReturnsValue()
        {
            var result = MurmurHashHelper.MakeHash64BValue(new byte[] { 0x42 });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void MakeHash64BValue_ExactBlockSize_ReturnsValue()
        {
            var key = new byte[8];
            Array.Fill(key, (byte)0xAA);

            var result = MurmurHashHelper.MakeHash64BValue(key);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void MakeHash64BValue_LargerThanBlockSize_ReturnsValue()
        {
            var key = new byte[16];
            Array.Fill(key, (byte)0xBB);

            var result = MurmurHashHelper.MakeHash64BValue(key);

            result.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void MakeHash64BValue_VariousLengths_ReturnsValue(int length)
        {
            var key = new byte[length];
            for (int i = 0; i < length; i++)
                key[i] = (byte)(i + 1);

            var result = MurmurHashHelper.MakeHash64BValue(key);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void StringToHashValue_SameInput_ReturnsSameResult()
        {
            var source = "murmur-hash";

            var value1 = MurmurHashHelper.StringToHashValue(source);
            var value2 = MurmurHashHelper.StringToHashValue(source);

            value1.Should().Be(value2);
        }

        [Fact]
        public void StringToHashValue_MatchesByteHash()
        {
            var source = "abcABC123";

            var stringResult = MurmurHashHelper.StringToHashValue(source);
            var byteResult = MurmurHashHelper.MakeHash64BValue(Encoding.UTF8.GetBytes(source));

            stringResult.Should().Be(byteResult);
        }

        [Fact]
        public void StringToHashValue_DifferentInputs_ReturnsDifferentResults()
        {
            var value1 = MurmurHashHelper.StringToHashValue("123456");
            var value2 = MurmurHashHelper.StringToHashValue("223456");

            value1.Should().NotBe(value2);
        }

        [Fact]
        public void StringToHashValue_EmptyString_ReturnsValue()
        {
            var result = MurmurHashHelper.StringToHashValue("");

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void StringToHashValue_UnicodeString_ReturnsValue()
        {
            var result = MurmurHashHelper.StringToHashValue("你好世界");

            result.Should().BeGreaterThan(0);
        }
    }
}
