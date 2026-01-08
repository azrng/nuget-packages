namespace Common.Core.Test.Helper.StringHelperTest
{
    public class ReplaceWithDictionaryTest
    {
        [Fact]
        public void ReplaceWithDictionary_ReplacesAllOccurrences()
        {
            var input = "Hello NAME, welcome to CITY!";
            var replacements = new Dictionary<string, string>
            {
                ["NAME"] = "Alice",
                ["CITY"] = "Paris"
            };

            var result = StringHelper.ReplaceWithDictionary(input, replacements);
            Assert.Equal("Hello Alice, welcome to Paris!", result);
        }

        [Fact]
        public void ReplaceWithDictionary_ReturnsInputWhenItIsNull()
        {
            var replacements = new Dictionary<string, string>
            {
                ["NAME"] = "Alice"
            };

            var result = StringHelper.ReplaceWithDictionary(null, replacements);
            Assert.Null(result);
        }

        [Fact]
        public void ReplaceWithDictionary_ReturnsInputWhenReplacementsAreEmpty()
        {
            const string input = "Hello world";
            var result = StringHelper.ReplaceWithDictionary(input, new Dictionary<string, string>());
            Assert.Equal(input, result);
        }

        [Fact]
        public void ReplaceWithDictionary_IgnoresEmptyKeys()
        {
            var input = "token placeholder";
            var replacements = new Dictionary<string, string>
            {
                [string.Empty] = "ignored",
                ["placeholder"] = "value"
            };

            var result = StringHelper.ReplaceWithDictionary(input, replacements);
            Assert.Equal("token value", result);
        }
    }
}
