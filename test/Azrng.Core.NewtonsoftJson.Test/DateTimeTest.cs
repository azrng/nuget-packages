using Xunit;
using Xunit.Abstractions;

namespace Azrng.Core.NewtonsoftJson.Test
{
    public class DateTimeTest
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITestOutputHelper _testOutputHelper;

        public DateTimeTest(IJsonSerializer jsonSerializer, ITestOutputHelper testOutputHelper)
        {
            _jsonSerializer = jsonSerializer;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void DateTime_ReturnOk()
        {
            var info = new DateTimeInfo { Id = "10", DateTime = new DateTime(2024, 12, 23) };
            var result = _jsonSerializer.ToJson(info);
            _testOutputHelper.WriteLine(result);
            Assert.Contains("2024-12-23 00:00:00", result);
        }
    }

    file class DateTimeInfo
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }
    }
}