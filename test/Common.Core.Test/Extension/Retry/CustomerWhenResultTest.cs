using Xunit.Abstractions;

namespace Common.Core.Test.Extension.Retry
{
    /// <summary>
    /// 指定结果条件重试测试
    /// </summary>
    public class CustomerWhenResultTest
    {
        private readonly ITestOutputHelper _logger;
        private static int _attemptCount = 0;

        public CustomerWhenResultTest(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        #region WhenResult<TResult>(Func<TResult, bool> predicate) 同步条件判断版本

        /// <summary>
        /// 测试 WhenResult 同步条件判断版本 - 验证条件为 true 时重试成功
        /// </summary>
        [Fact]
        public async Task WhenResult_WithPredicate_RetryWhenConditionIsTrue()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(1, expected), 3)
                                          .WhenResult(r =>
                                          {
                                              _logger.WriteLine($"检查结果: {r}");
                                              return r.Contains("invalid"); // 当结果包含"invalid"时重试
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount); // 第一次返回无效结果，第二次返回成功结果
        }

        /// <summary>
        /// 测试 WhenResult 同步条件判断版本 - 验证条件为 false 时不重试
        /// </summary>
        [Fact]
        public async Task WhenResult_WithPredicate_NoRetryWhenConditionIsFalse()
        {
            // Arrange
            _attemptCount = 0;
            var resultValue = "valid-result";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(0, resultValue), 3)
                                          .WhenResult(r =>
                                          {
                                              return r.Contains("invalid"); // 条件为 false，不重试
                                          });

            // Assert
            Assert.Equal(resultValue, result);
            Assert.Equal(1, _attemptCount); // 只执行一次，不重试
        }

        /// <summary>
        /// 测试 WhenResult 同步条件判断版本 - 验证基于结果属性的条件判断
        /// </summary>
        [Fact]
        public async Task WhenResult_WithPredicate_PredicateBasedOnResultProperty()
        {
            // Arrange
            _attemptCount = 0;
            var expectedData = new TestResult(100, "completed");

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithProgressData(2, expectedData), 3)
                                          .WhenResult(r =>
                                          {
                                              return r.Status == "processing"; // 当状态为"processing"时重试
                                          });

            // Assert
            Assert.Equal(10, result.Value);
            Assert.Equal("pending", result.Status);

            // Assert.Equal(3, _attemptCount); // 两次 processing，一次 completed
        }

        /// <summary>
        /// 测试 WhenResult 同步条件判断版本 - 验证多次重试
        /// </summary>
        [Fact]
        public async Task WhenResult_WithPredicate_MultipleRetries()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "final-success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(3, expected), 5)
                                          .WhenResult(r =>
                                          {
                                              return r.StartsWith("temp-"); // 当结果以"temp-"开头时重试
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(4, _attemptCount); // 三次临时结果，一次成功结果
        }

        #endregion

        #region WhenResultAsync<TResult>(Func<TResult, Task<bool>> predicate) 异步条件判断版本

        /// <summary>
        /// 测试 WhenResultAsync 异步条件判断版本 - 验证异步条件为 true 时重试成功
        /// </summary>
        [Fact]
        public async Task WhenResultAsync_WithAsyncPredicate_RetryWhenAsyncConditionIsTrue()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(1, expected), 3)
                                          .WhenResultAsync(async r =>
                                          {
                                              await Task.Delay(10); // 模拟异步操作
                                              _logger.WriteLine($"异步检查结果: {r}");
                                              return r.Contains("invalid");
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount);
        }

        /// <summary>
        /// 测试 WhenResultAsync 异步条件判断版本 - 验证异步条件为 false 时不重试
        /// </summary>
        [Fact]
        public async Task WhenResultAsync_WithAsyncPredicate_NoRetryWhenAsyncConditionIsFalse()
        {
            // Arrange
            _attemptCount = 0;
            var resultValue = "valid-result";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(0, resultValue), 3)
                                          .WhenResultAsync(async r =>
                                          {
                                              await Task.Delay(10);
                                              return r.Contains("invalid"); // 条件为 false
                                          });

            // Assert
            Assert.Equal(resultValue, result);
            Assert.Equal(1, _attemptCount);
        }

        /// <summary>
        /// 测试 WhenResultAsync 异步条件判断版本 - 验证异步条件基于结果属性
        /// </summary>
        [Fact]
        public async Task WhenResultAsync_WithAsyncPredicate_PredicateBasedOnResultProperty()
        {
            // Arrange
            _attemptCount = 0;
            var expectedData = new TestResult(200, "ready");

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithProgressData(2, expectedData), 3)
                                          .WhenResultAsync(async r =>
                                          {
                                              await Task.Delay(10);
                                              return r.Status == "pending" || r.Status == "processing"; // 当状态为"pending"时重试
                                          });

            // Assert
            Assert.Equal(200, result.Value);
            Assert.Equal(expectedData.Status, result.Status);
        }

        #endregion

        #region 综合测试

        /// <summary>
        /// 综合测试 - 验证结果条件判断与延时组合使用
        /// </summary>
        [Fact]
        public async Task WhenResult_CombinedWithDelay_Success()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var delay = TimeSpan.FromMilliseconds(50);

            // Act
            var startTime = DateTime.Now;
            var result = await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(1, expected), 3, delay)
                                          .WhenResult(r => r.Contains("invalid"));
            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.Equal(expected, result);

            // 执行时间应该包含一次延时
            Assert.True(elapsed >= TimeSpan.FromMilliseconds(50));
            Assert.True(elapsed < TimeSpan.FromMilliseconds(300));
        }

        /// <summary>
        /// 综合测试 - 验证空值处理
        /// </summary>
        [Fact]
        public async Task WhenResult_WithNullPredicate_ThrowsArgumentNullException()
        {
            // Arrange
            _attemptCount = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateTaskWithTemporaryInvalidResult(1, "success"), 3)
                                 .WhenResult((Func<string, bool>)null));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建返回临时无效结果的任务
        /// </summary>
        /// <param name="invalidCount">需要返回无效结果的次数</param>
        /// <param name="finalResult">最终返回的正确结果</param>
        /// <returns>异步任务</returns>
        private async Task<string> CreateTaskWithTemporaryInvalidResult(int invalidCount, string finalResult)
        {
            _attemptCount++;
            await Task.Delay(50);

            if (_attemptCount <= invalidCount)
            {
                var invalidResult = $"temp-invalid-{_attemptCount}";
                _logger.WriteLine($"CreateTaskWithTemporaryInvalidResult 第 {_attemptCount} 次返回临时结果: {invalidResult}");
                return invalidResult;
            }

            _logger.WriteLine($"CreateTaskWithTemporaryInvalidResult 第 {_attemptCount} 次返回最终结果: {finalResult}");
            return finalResult;
        }

        /// <summary>
        /// 创建返回进度数据的任务
        /// </summary>
        /// <param name="processingCount">需要返回处理中状态的次数</param>
        /// <param name="finalData">最终返回的完成数据</param>
        /// <returns>异步任务</returns>
        private async Task<TestResult> CreateTaskWithProgressData(int processingCount, TestResult finalData)
        {
            _attemptCount++;
            await Task.Delay(50);

            if (_attemptCount <= processingCount)
            {
                var status = _attemptCount == 1 ? "processing" : "pending";
                _logger.WriteLine($"CreateTaskWithProgressData 第 {_attemptCount} 次返回状态: {status}");
                return new TestResult(10, status);
            }

            _logger.WriteLine($"CreateTaskWithProgressData 第 {_attemptCount} 次返回完成状态: {finalData.Status}");
            return finalData;
        }

        /// <summary>
        /// 创建始终返回无效结果的任务
        /// </summary>
        /// <returns>异步任务</returns>
        private async Task<string> CreateTaskAlwaysReturningInvalid()
        {
            _attemptCount++;
            await Task.Delay(50);

            var invalidResult = $"always-invalid-{_attemptCount}";
            _logger.WriteLine($"CreateTaskAlwaysReturningInvalid 第 {_attemptCount} 次返回无效结果: {invalidResult}");
            return invalidResult;
        }

        #endregion
    }

    /// <summary>
    /// 测试结果类
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// 数值
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; }

        public TestResult(int value, string status)
        {
            Value = value;
            Status = status;
        }
    }
}