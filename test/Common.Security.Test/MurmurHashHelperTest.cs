using System.Text;

namespace Common.Security.Test
{
    public class MurmurHashHelperTest
    {
        [Fact]
        public void StringToHashValue_SameInput_ReturnSameResult()
        {
            var source = "murmur-hash";
            var value1 = MurmurHashHelper.StringToHashValue(source);
            var value2 = MurmurHashHelper.StringToHashValue(source);

            Assert.Equal(value1, value2);
        }

        [Fact]
        public void StringToHashValue_AndByteHash_ReturnSameResult()
        {
            var source = "abcABC123";
            var value1 = MurmurHashHelper.StringToHashValue(source);
            var value2 = MurmurHashHelper.MakeHash64BValue(Encoding.UTF8.GetBytes(source));

            Assert.Equal(value1, value2);
        }

        [Fact]
        public void StringToHashValue_DifferentInput_ReturnDifferentResult()
        {
            var value1 = MurmurHashHelper.StringToHashValue("123456");
            var value2 = MurmurHashHelper.StringToHashValue("223456");

            Assert.NotEqual(value1, value2);
        }
    }
}
