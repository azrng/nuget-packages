using Azrng.Cache.Core;
using Common.Cache.Redis.Test.Model;

namespace Common.Cache.Redis.Test
{
    public class GetOrCreateTest
    {
        private readonly ICacheProvider _cacheProvider;

        public GetOrCreateTest(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        [Fact]
        public async Task GetOrCreate_Object()
        {
            var key = Guid.NewGuid().ToString("N");
            var isExist = await _cacheProvider.ExistAsync(key);
            Assert.False(isExist);

            var result =
                await _cacheProvider.GetOrCreateAsync(key, () => new UserInfo("王五", 2),
                    TimeSpan.FromSeconds(2));

            Assert.NotNull(result);

            Assert.Equal("王五", result.UserName);

            await Task.Delay(2 * 2000);
            result = await _cacheProvider.GetAsync<UserInfo>(key);
            Assert.Null(result);
        }

        /// <summary>
        /// 缓存空集合测试=>默认不缓存空集合
        /// </summary>
        [Fact]
        public async Task CacheEmptyCollections_Test()
        {
            var key = Guid.NewGuid().ToString("N");
            var isExist = await _cacheProvider.ExistAsync(key);
            Assert.False(isExist);

            var result = await _cacheProvider.GetOrCreateAsync(key, () => new List<UserInfo>(),
                TimeSpan.FromSeconds(2));
            Assert.NotNull(result);

            result = await _cacheProvider.GetAsync<List<UserInfo>>(key);
            Assert.Null(result);
        }

        /// <summary>
        /// 缓存空集合测试=>默认不缓存空集合
        /// </summary>
        [Fact]
        public async Task CacheEmptyCollections_Async_Test()
        {
            var key = Guid.NewGuid().ToString("N");
            var isExist = await _cacheProvider.ExistAsync(key);
            Assert.False(isExist);

            var result = await _cacheProvider.GetOrCreateAsync(key, () => Task.FromResult(new List<UserInfo>()),
                TimeSpan.FromSeconds(2));
            Assert.NotNull(result);

            result = await _cacheProvider.GetAsync<List<UserInfo>>(key);
            Assert.Null(result);
        }

        /// <summary>
        /// 缓存字符串测试=>不缓存
        /// </summary>
        [Fact]
        public async Task CacheEmptyString_Test()
        {
            var key = Guid.NewGuid().ToString("N");
            var isExist = await _cacheProvider.ExistAsync(key);
            Assert.False(isExist);

            var result = await _cacheProvider.GetOrCreateAsync(key, () => string.Empty,
                TimeSpan.FromSeconds(2));
            Assert.NotNull(result);

            result = await _cacheProvider.GetAsync<string>(key);
            Assert.Null(result);
        }

        /// <summary>
        /// 缓存字符串测试=>不缓存
        /// </summary>
        [Fact]
        public async Task CacheEmptyString_Async_Test()
        {
            var key = Guid.NewGuid().ToString("N");
            var isExist = await _cacheProvider.ExistAsync(key);
            Assert.False(isExist);

            var result = await _cacheProvider.GetOrCreateAsync(key, () => string.Empty,
                TimeSpan.FromSeconds(2));
            Assert.NotNull(result);

            result = await _cacheProvider.GetAsync<string>(key);
            Assert.Null(result);
        }
    }
}