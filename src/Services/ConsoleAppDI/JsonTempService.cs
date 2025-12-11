using Azrng.ConsoleApp.DependencyInjection;
using Azrng.Core;
using Azrng.Core.Helpers;
using ConsoleAppDI.Dto;
using Microsoft.Extensions.Logging;

namespace ConsoleAppDI
{
    public class JsonTempService : IServiceStart
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<JsonTempService> _logger;

        public JsonTempService(IJsonSerializer jsonSerializer, ILogger<JsonTempService> logger)
        {
            _jsonSerializer = jsonSerializer;
            _logger = logger;
        }

        public string Title => "json 示例";

        public Task RunAsync()
        {
            var list = new List<IndicatorBaseInfoDto>
                       {
                           new IndicatorBaseInfoDto
                           {
                               IndicatorId = Guid.Parse("0c671a16-9cf8-4ebe-be0e-c7c88eb9b384"), IndicatorName = "住院复诊率"
                           },
                           new IndicatorBaseInfoDto
                           {
                               IndicatorId = Guid.Parse("777d2b47-c00e-48a7-b6b4-6d5716d7a39e"), IndicatorName = "门诊复诊率"
                           },
                       };

            var result = _jsonSerializer.ToJson(list);
            _logger.LogInformation(result);
            // LocalLogHelper.LogInformation(result);

            return Task.CompletedTask;
        }
    }
}