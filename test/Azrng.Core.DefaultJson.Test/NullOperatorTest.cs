using Azrng.Core.DefaultJson.Test.Models;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.Core.DefaultJson.Test
{
    public class NullOperatorTest
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITestOutputHelper _testOutputHelper;

        public NullOperatorTest(IJsonSerializer jsonSerializer, ITestOutputHelper testOutputHelper)
        {
            _jsonSerializer = jsonSerializer;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ToJson_Null_ReturnOk()
        {
            UserInfo? obj = null;
            var result = _jsonSerializer.ToJson(obj);
            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void ToObject_Null_ReturnOk()
        {
            string? obj = null;
            var result = _jsonSerializer.ToObject<UserInfo>(obj);
            Assert.Null(result);
        }
    }
}