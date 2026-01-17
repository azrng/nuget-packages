using Azrng.Core.Exceptions;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension.Retry
{
    /// <summary>
    /// 指定异常类型重试测试
    /// </summary>
    public class CustomerExceptionRetryTest
    {
        private readonly ITestOutputHelper _logger;
        private static int _attemptCount = 0;

        public CustomerExceptionRetryTest(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        #region WhenCatch<TException>() 无参数版本

        /// <summary>
        /// 测试 WhenCatch 无参数版本 - 验证捕获指定异常类型时重试成功
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithNoParams_RetryOnSpecifiedException()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(1, expected), 3)
                                          .WhenCatch<ParameterException>();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试 WhenCatch 无参数版本 - 验证非指定异常类型不重试
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithNoParams_NoRetryOnDifferentException()
        {
            // Arrange
            _attemptCount = 0;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithInvalidOperationException(1, ""), 3)
                                 .WhenCatch<ParameterException>());

            Assert.Equal(1, _attemptCount); // 只执行一次，不重试
        }

        #endregion

        #region WhenCatch<TException>(Action<TException> handler) 带回调版本

        /// <summary>
        /// 测试 WhenCatch 带回调版本 - 验证捕获异常并执行回调后重试成功
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithActionHandler_RetryWithCallback()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var callbackCount = 0;

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(1, expected), 3)
                                          .WhenCatch<ParameterException>(ex =>
                                          {
                                              callbackCount++;
                                              _logger.WriteLine($"回调被执行，异常消息: {ex.Message}");
                                          });

            // Assert
            Assert.Equal(expected, result);
            //Assert.Equal(2, _attemptCount);
            Assert.Equal(1, callbackCount); // 回调被调用一次
        }

        /// <summary>
        /// 测试 WhenCatch 带回调版本 - 验证多次重试时回调执行次数
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithActionHandler_CallbackExecutedMultipleTimes()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var callbackCount = 0;

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(2, expected), 3)
                                          .WhenCatch<ParameterException>(ex =>
                                          {
                                              callbackCount++;
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(3, _attemptCount);
            Assert.Equal(2, callbackCount); // 回调被调用两次
        }

        #endregion

        #region WhenCatch<TException>(Func<TException, bool> predicate) 带条件判断版本

        /// <summary>
        /// 测试 WhenCatch 带条件判断版本 - 验证条件为 true 时重试成功
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithPredicate_RetryWhenConditionIsTrue()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(1, expected), 3)
                                          .WhenCatch<ParameterException>(ex =>
                                          {
                                              return ex.Message.Contains("测试");
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount);
        }

        /// <summary>
        /// 测试 WhenCatch 带条件判断版本 - 验证条件为 false 时不重试
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithPredicate_NoRetryWhenConditionIsFalse()
        {
            // Arrange
            _attemptCount = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ParameterException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(4, "不匹配"), 3)
                                 .WhenCatch<ParameterException>(ex =>
                                 {
                                     return ex.Message.Contains("不存在的内容");
                                 }));

            Assert.Equal(1, _attemptCount); // 只执行一次，不重试
        }

        /// <summary>
        /// 测试 WhenCatch 带条件判断版本 - 验证基于异常属性的条件判断
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithPredicate_PredicateBasedOnExceptionProperty()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var allowedFailCount = 2;

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithCount(allowedFailCount, expected), 3)
                                          .WhenCatch<CountedException>(ex =>
                                          {
                                              return ex.Count < 3; // 只重试当失败次数小于3时
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(3, _attemptCount);
        }

        /// <summary>
        /// 测试 WhenCatch 带条件判断版本 - 验证条件为 false 时不再重试并抛出原始异常
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithPredicate_ThrowOriginalExceptionWhenConditionFalse()
        {
            // Arrange
            _attemptCount = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CountedException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithCount(1, ""), 3)
                                 .WhenCatch<CountedException>(ex =>
                                 {
                                     return ex.Message.Length > 100; // 条件为 false
                                 }));

            Assert.Equal(1, _attemptCount); // 只执行一次
            Assert.Contains("测试异常", exception.Message);
        }

        #endregion

        #region WhenCatchAsync<TException>(Func<TException, Task> handler) 异步回调版本

        /// <summary>
        /// 测试 WhenCatchAsync 异步回调版本 - 验证异步回调执行后重试成功
        /// </summary>
        [Fact]
        public async Task WhenCatchAsync_WithAsyncHandler_RetryWithAsyncCallback()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var callbackCount = 0;

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(1, expected), 3)
                                          .WhenCatchAsync<ParameterException>(async ex =>
                                          {
                                              await Task.Delay(10); // 模拟异步操作
                                              callbackCount++;
                                              _logger.WriteLine($"异步回调被执行");
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount);
            Assert.Equal(1, callbackCount);
        }

        #endregion

        #region WhenCatchAsync<TException>(Func<TException, Task<bool>> predicate) 异步条件判断版本

        /// <summary>
        /// 测试 WhenCatchAsync 异步条件判断版本 - 验证异步条件为 true 时重试成功
        /// </summary>
        [Fact]
        public async Task WhenCatchAsync_WithAsyncPredicate_RetryWhenAsyncConditionIsTrue()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";

            // Act
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(1, expected), 3)
                                          .WhenCatchAsync<ParameterException>(async ex =>
                                          {
                                              await Task.Delay(10);
                                              return ex.Message.Contains("测试");
                                          });

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount);
        }

        /// <summary>
        /// 测试 WhenCatchAsync 异步条件判断版本 - 验证异步条件为 false 时不重试
        /// </summary>
        [Fact]
        public async Task WhenCatchAsync_WithAsyncPredicate_NoRetryWhenAsyncConditionIsFalse()
        {
            // Arrange
            _attemptCount = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ParameterException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(5, ""), 3)
                                 .WhenCatchAsync<ParameterException>(async ex =>
                                 {
                                     await Task.Delay(10);
                                     return false;
                                 }));

            Assert.Equal(1, _attemptCount);
        }

        #endregion

        #region 综合测试

        /// <summary>
        /// 综合测试 - 验证指定异常类型重试、回调和条件判断组合使用
        /// </summary>
        [Fact]
        public async Task WhenCatch_CombinedWithMaxCountAndDelay_Success()
        {
            // Arrange
            _attemptCount = 0;
            var expected = "success";
            var callbackCount = 0;
            var delay = TimeSpan.FromMilliseconds(50);

            // Act
            var startTime = DateTime.Now;
            var result = await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(1, expected), 3, delay)
                                          .WhenCatch<RetryMarkException>(ex =>
                                          {
                                              callbackCount++;
                                          });
            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(2, _attemptCount);
            Assert.Equal(1, callbackCount);

            // 执行时间应该包含一次延时
            Assert.True(elapsed >= TimeSpan.FromMilliseconds(50));
            Assert.True(elapsed < TimeSpan.FromMilliseconds(300));
        }

        /// <summary>
        /// 综合测试 - 验证超过最大重试次数时抛出异常
        /// </summary>
        [Fact]
        public async Task WhenCatch_ThrowWhenExceedMaxRetry()
        {
            // Arrange
            _attemptCount = 0;
            var callbackCount = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InternalServerException>(async () =>
                await RetryHelper.ExecuteAsync(() => CreateFailingTaskWithRetryMark(3, ""), 2)
                                 .WhenCatch<RetryMarkException>(ex =>
                                 {
                                     callbackCount++;
                                 }));

            Assert.Contains("超出最大重试2", exception.Message);
            Assert.Equal(3, _attemptCount); // 初始1次 + 2次重试
            Assert.Equal(2, callbackCount);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建抛出 RetryMarkException 的失败任务
        /// </summary>
        /// <param name="failCount">失败次数</param>
        /// <param name="result">成功时的返回值</param>
        /// <returns>异步任务</returns>
        private async Task<string> CreateFailingTaskWithRetryMark(int failCount, string result)
        {
            _attemptCount++;
            await Task.Delay(50);

            if (_attemptCount <= failCount)
            {
                _logger.WriteLine($"CreateFailingTaskWithRetryMark 第 {_attemptCount} 次执行失败");
                throw new ParameterException("测试异常");
            }

            _logger.WriteLine($"CreateFailingTaskWithRetryMark 第 {_attemptCount} 次执行成功，返回: {result}");
            return result;
        }

        /// <summary>
        /// 创建抛出 InvalidOperationException 的失败任务
        /// </summary>
        /// <param name="failCount">失败次数</param>
        /// <param name="result">成功时的返回值</param>
        /// <returns>异步任务</returns>
        private async Task<string> CreateFailingTaskWithInvalidOperationException(int failCount, string result)
        {
            _attemptCount++;
            await Task.Delay(50);

            if (_attemptCount <= failCount)
            {
                _logger.WriteLine($"CreateFailingTaskWithInvalidOperationException 第 {_attemptCount} 次执行失败");
                throw new InvalidOperationException("无效操作异常");
            }

            _logger.WriteLine($"CreateFailingTaskWithInvalidOperationException 第 {_attemptCount} 次执行成功，返回: {result}");
            return result;
        }

        /// <summary>
        /// 创建带计数的异常任务
        /// </summary>
        /// <param name="failCount">失败次数</param>
        /// <param name="result">成功时的返回值</param>
        /// <returns>异步任务</returns>
        private async Task<string> CreateFailingTaskWithCount(int failCount, string result)
        {
            _attemptCount++;
            await Task.Delay(50);

            if (_attemptCount <= failCount)
            {
                _logger.WriteLine($"CreateFailingTaskWithCount 第 {_attemptCount} 次执行失败");
                throw new CountedException(_attemptCount, "测试异常");
            }

            _logger.WriteLine($"CreateFailingTaskWithCount 第 {_attemptCount} 次执行成功，返回: {result}");
            return result;
        }

        #endregion
    }

    /// <summary>
    /// 带计数的自定义异常
    /// </summary>
    public class CountedException : Exception
    {
        public int Count { get; }

        public CountedException(int count, string message) : base(message)
        {
            Count = count;
        }
    }
}