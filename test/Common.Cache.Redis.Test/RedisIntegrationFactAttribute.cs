using System;
using Xunit;

namespace Common.Cache.Redis.Test
{
    internal sealed class RedisIntegrationFactAttribute : FactAttribute
    {
        private const string ConnectionStringEnvName = "COMMON_CACHE_REDIS_TEST_CONNECTION";

        public RedisIntegrationFactAttribute()
        {
            // if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionStringEnvName)))
            // {
            //     Skip = $"Set {ConnectionStringEnvName} to run Redis integration tests.";
            // }
        }
    }
}
