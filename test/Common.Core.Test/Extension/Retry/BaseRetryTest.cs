using Azrng.Core.Exceptions;
using Azrng.Core.RetryTask;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension.Retry
{
    /// <summary>
    /// 基础的测试
    /// </summary>
    public class BaseRetryTest
    {
        private readonly ITestOutputHelper _logger;
        private static int _attemptCount = 0;

        public BaseRetryTest(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 测试基本重试方法 - 验证成功任务正常执行
        /// </summary>
        [Fact]
        public async Task Retry_WithMaxCount_Success()
        {
            // Arrange
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateSuccessfulTask(expected), 3);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试基本重试方法 - 验证失败时自动重试
        /// </summary>
        [Fact]
        public async Task Retry_WithMaxCount_RetryOnFailure()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTask(_attemptCount, expected), maxRetryCount: 3);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试基本重试方法 - 验证超过最大重试次数时抛出异常
        /// </summary>
        [Fact]
        public async Task Retry_WithMaxCount_ThrowWhenExceedMaxRetry()
        {
            // Arrange
            _attemptCount = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InternalServerException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateFailingTask(3, ""), maxRetryCount: 2));
            Assert.Contains("超出最大重试2", exception.Message);
            Assert.Equal(3, _attemptCount); // 初始1次 + 2次重试
        }

        /// <summary>
        /// 测试带固定延时重试方法 - 验证重试时正确应用延时
        /// </summary>
        [Fact]
        public async Task Retry_WithFixedDelay_Success()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var delay = TimeSpan.FromMilliseconds(100);

            // Act
            var startTime = DateTime.Now;
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTask(2, expected), 3, delay);
            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(3, _attemptCount);

            // 执行时间应该包含两次延时（每次100ms）
            Assert.True(elapsed >= TimeSpan.FromMilliseconds(200));
            Assert.True(elapsed < TimeSpan.FromMilliseconds(500)); // 加上任务执行时间
        }

        /// <summary>
        /// 测试带固定延时重试方法 - 验证带延时的重试逻辑
        /// </summary>
        [Fact]
        public async Task Retry_WithFixedDelay_RetryOnFailure()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTask(1, expected), 3, TimeSpan.FromMilliseconds(50));

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount); // 第一次失败，第二次成功
        }

        /// <summary>
        /// 测试带动态延时重试方法 - 验证零延时情况
        /// </summary>
        [Fact]
        public async Task Retry_WithDynamicDelay_ZeroDelay()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            Func<int, TimeSpan> zeroDelay = retryIndex => TimeSpan.Zero;

            // Act
            var startTime = DateTime.Now;
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTask(2, expected), 3, zeroDelay);

            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(3, _attemptCount);

            // 延时应该非常短（只有任务执行时间）
            Assert.True(elapsed < TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// 创建成功的任务，用于测试正常情况
        /// </summary>
        /// <typeparam name="T">返回结果类型</typeparam>
        /// <param name="result">要返回的结果</param>
        /// <returns>异步任务</returns>
        private async Task<T> CreateSuccessfulTask<T>(T result)
        {
            _logger.WriteLine($"CreateSuccessfulTask 执行中，返回值: {result}");
            await Task.Delay(100);
            _logger.WriteLine($"CreateSuccessfulTask 执行完成");
            return result;
        }

        private async Task<string> CreateFailingTask(int failCount, string result)
        {
            _attemptCount++;
            await Task.Delay(50);

            if (_attemptCount <= failCount)
            {
                _logger.WriteLine($"CreateFailingTask 第 {_attemptCount} 次执行失败");
                throw new RetryMarkException("测试异常");
            }

            _logger.WriteLine($"CreateFailingTask 第 {_attemptCount} 次执行成功，返回: {result}");
            return result;
        }
    }
}