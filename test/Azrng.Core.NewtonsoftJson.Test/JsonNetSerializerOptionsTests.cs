using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Azrng.Core.NewtonsoftJson.Test
{
    public class JsonNetSerializerOptionsTests
    {
        [Fact]
        public void DefaultOptions_SerializeSettings_NotNull()
        {
            var options = new JsonNetSerializerOptions();

            Assert.NotNull(options.JsonSerializeOptions);
        }

        [Fact]
        public void DefaultOptions_DeserializeSettings_NotNull()
        {
            var options = new JsonNetSerializerOptions();

            Assert.NotNull(options.JsonDeserializeOptions);
        }

        [Fact]
        public void DefaultOptions_SerializeSettings_UsesCamelCaseContractResolver()
        {
            var options = new JsonNetSerializerOptions();

            Assert.IsType<CamelCasePropertyNamesContractResolver>(options.JsonSerializeOptions.ContractResolver);
        }

        [Fact]
        public void DefaultOptions_DeserializeSettings_UsesCamelCaseContractResolver()
        {
            var options = new JsonNetSerializerOptions();

            Assert.IsType<CamelCasePropertyNamesContractResolver>(options.JsonDeserializeOptions.ContractResolver);
        }

        [Fact]
        public void DefaultOptions_SerializeSettings_HasIsoDateTimeConverter()
        {
            var options = new JsonNetSerializerOptions();

            var hasIsoDateTimeConverter = options.JsonSerializeOptions.Converters
                .Any(c => c.GetType().Name == "IsoDateTimeConverter");

            Assert.True(hasIsoDateTimeConverter);
        }

        [Fact]
        public void DefaultOptions_DeserializeSettings_HasIsoDateTimeConverter()
        {
            var options = new JsonNetSerializerOptions();

            var hasIsoDateTimeConverter = options.JsonDeserializeOptions.Converters
                .Any(c => c.GetType().Name == "IsoDateTimeConverter");

            Assert.True(hasIsoDateTimeConverter);
        }

        [Fact]
        public void SerializeAndDeserializeSettings_AreIndependentInstances()
        {
            var options = new JsonNetSerializerOptions();

            Assert.NotSame(options.JsonSerializeOptions, options.JsonDeserializeOptions);
        }

        [Fact]
        public void Options_CanOverrideSerializeSettings()
        {
            var options = new JsonNetSerializerOptions();
            var customSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            options.JsonSerializeOptions = customSettings;

            Assert.Same(customSettings, options.JsonSerializeOptions);
        }

        [Fact]
        public void Options_CanOverrideDeserializeSettings()
        {
            var options = new JsonNetSerializerOptions();
            var customSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            options.JsonDeserializeOptions = customSettings;

            Assert.Same(customSettings, options.JsonDeserializeOptions);
        }

        [Fact]
        public void DateTimeFormat_DefaultOptions_FormatsCorrectly()
        {
            var options = new JsonNetSerializerOptions();
            var date = new DateTime(2025, 3, 10, 14, 30, 0);
            var obj = new { Date = date };

            var json = JsonConvert.SerializeObject(obj, options.JsonSerializeOptions);

            Assert.Contains("2025-03-10 14:30:00", json);
        }
    }
}
