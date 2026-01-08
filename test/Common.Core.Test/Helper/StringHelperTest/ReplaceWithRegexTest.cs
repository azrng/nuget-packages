namespace Common.Core.Test.Helper.StringHelperTest
{
    public class ReplaceWithRegexTest
    {
        [Fact]
        public void ReplaceWithRegex_ReplacesAllLiteralMatches()
        {
            var input = "Hello NAME, welcome to CITY.";
            var replacements = new Dictionary<string, string>
            {
                ["NAME"] = "Alice",
                ["CITY"] = "Paris"
            };

            var result = StringHelper.ReplaceWithRegex(input, replacements);
            Assert.Equal("Hello Alice, welcome to Paris.", result);
        }

        [Fact]
        public void ReplaceWithRegex_HandlesSpecialCharactersInKeys()
        {
            var input = "Symbols +() need escaping?";
            var replacements = new Dictionary<string, string>
            {
                ["+()"] = "plus-parentheses",
                ["?"] = "Q"
            };

            var result = StringHelper.ReplaceWithRegex(input, replacements);
            Assert.Equal("Symbols plus-parentheses need escapingQ", result);
        }

        [Fact]
        public void ReplaceWithRegex_ReturnsOriginalWhenDictionaryIsNull()
        {
            const string input = "demo";
            var result = StringHelper.ReplaceWithRegex(input, null);
            Assert.Equal(input, result);
        }

        [Fact]
        public void ReplaceWithRegex_ReturnsOriginalWhenDictionaryIsEmpty()
        {
            const string input = "demo";
            var result = StringHelper.ReplaceWithRegex(input, new Dictionary<string, string>());
            Assert.Equal(input, result);
        }
    }
}
