using FluentAssertions;

namespace Azrng.Security.Test
{
    public class RandomStringGeneratorTests
    {
        [Fact]
        public void Generate_DefaultLength_Returns8Chars()
        {
            var result = RandomStringGenerator.Generate();

            result.Should().HaveLength(8);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(128)]
        [InlineData(256)]
        public void Generate_SpecifiedLength_ReturnsCorrectLength(int length)
        {
            var result = RandomStringGenerator.Generate(length);

            result.Should().HaveLength(length);
        }

        [Fact]
        public void Generate_ZeroLength_ReturnsEmptyString()
        {
            var result = RandomStringGenerator.Generate(0);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Generate_NegativeLength_ThrowsArgumentOutOfRangeException()
        {
            Action act = () => RandomStringGenerator.Generate(-1);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("length");
        }

        [Fact]
        public void Generate_TwoCalls_ReturnsDifferentResults()
        {
            var result1 = RandomStringGenerator.Generate(32);
            var result2 = RandomStringGenerator.Generate(32);

            result1.Should().NotBe(result2);
        }

        [Fact]
        public void Generate_ContainsOnlyDictionaryCharacters()
        {
            const string dictionary =
                "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

            var result = RandomStringGenerator.Generate(1000);

            result.All(c => dictionary.Contains(c)).Should().BeTrue();
        }

        [Fact]
        public void Generate_LargeLength_ReturnsCorrectLength()
        {
            var result = RandomStringGenerator.Generate(1000);

            result.Should().HaveLength(1000);
        }
    }
}
