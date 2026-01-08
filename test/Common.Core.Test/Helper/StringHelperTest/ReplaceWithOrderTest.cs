namespace Common.Core.Test.Helper.StringHelperTest
{
    public class ReplaceWithOrderTest
    {
        [Fact]
        public void ReplaceWithOrder_UsesLongestKeysFirst()
        {
            var input = "abcabc";
            var replacements = new Dictionary<string, string>
            {
                ["ab"] = "Y",
                ["abc"] = "X"
            };

            var result = StringHelper.ReplaceWithOrder(input, replacements);
            Assert.Equal("XX", result);
        }

        [Fact]
        public void ReplaceWithOrder_ReturnsOriginalWhenDictionaryIsNull()
        {
            const string input = "demo";
            var result = StringHelper.ReplaceWithOrder(input, null);
            Assert.Equal(input, result);
        }

        [Fact]
        public void ReplaceWithOrder_ReturnsOriginalWhenDictionaryIsEmpty()
        {
            const string input = "demo";
            var result = StringHelper.ReplaceWithOrder(input, new Dictionary<string, string>());
            Assert.Equal(input, result);
        }
    }
}
