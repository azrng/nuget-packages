using System;
using Azrng.Security;
using Xunit;

namespace Azrng.Security.Test
{
    public class RandomStringGeneratorTest
    {
        [Fact]
        public void Generate_ReturnsRequestedLength()
        {
            Assert.Equal(16, RandomStringGenerator.Generate(16).Length);
        }

        [Fact]
        public void Generate_DefaultLengthIs8()
        {
            Assert.Equal(8, RandomStringGenerator.Generate().Length);
        }

        [Fact]
        public void Generate_NegativeLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringGenerator.Generate(-1));
        }

        [Fact]
        public void Generate_TwoCallsAreDifferent()
        {
            Assert.NotEqual(RandomStringGenerator.Generate(32), RandomStringGenerator.Generate(32));
        }
    }
}
