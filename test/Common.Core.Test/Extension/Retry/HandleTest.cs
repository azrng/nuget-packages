using Azrng.Core.RetryTask;
using Xunit.Abstractions;

namespace Common.Core.Test.Extension.Retry
{
    /// <summary>
    /// 异常处理测试
    /// </summary>
    public class HandleTest
    {
        private readonly ITestOutputHelper _logger;

        public HandleTest(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        #region Task<TResult>.Handle() 基础测试

        /// <summary>
        /// 测试 Handle 方法 - 验证成功任务正常执行
        /// </summary>
        [Fact]
        public async Task Handle_WithSuccessfulTask_ReturnsResult()
        {
            // Arrange
            var expected = "success";
            var task = CreateSuccessfulTask(expected);

            // Act
            var handleTask = task.Handle();
            var result = await handleTask;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试 Handle 方法 - 验证 null 任务抛出异常
        /// </summary>
        [Fact]
        public async Task Handle_WithNullTask_ThrowsArgumentNullException()
        {
            // Arrange
            Task<int> task = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await task.Handle<int>());
            Assert.Equal("task", ex.ParamName);
        }

        /// <summary>
        /// 测试 Handle 方法 - 验证异常未被捕获时抛出原始异常
        /// </summary>
        [Fact]
        public async Task Handle_WithUnhandledException_ThrowsOriginalException()
        {
            // Arrange
            var errorMessage = "测试异常";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await CreateFailingTask(errorMessage).Handle());

            Assert.Equal(errorMessage, ex.Message);
        }

        #endregion

        #region Task<TResult>.Handle().WhenCatch<TException>(Func<TException, TResult> func) 指定异常处理

        /// <summary>
        /// 测试 WhenCatch 带异常参数 - 验证捕获指定异常并返回默认值
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithExceptionParam_ReturnsDefaultValue()
        {
            // Arrange
            var errorMessage = "测试异常";
            var defaultValue = "default-value";

            // Act
            var result = await CreateFailingTask(errorMessage)
                .Handle()
                .WhenCatch<InvalidOperationException>(ex => defaultValue);

            // Assert
            Assert.Equal(defaultValue, result);
        }

        /// <summary>
        /// 测试 WhenCatch 带异常参数 - 验证可访问异常信息
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithExceptionParam_CanAccessExceptionMessage()
        {
            // Arrange
            var errorMessage = "自定义错误消息";
            var exceptionCaught = false;
            string caughtMessage = null;

            // Act
            var result = await CreateFailingTask(errorMessage)
                .Handle()
                .WhenCatch<InvalidOperationException>(ex =>
                {
                    exceptionCaught = true;
                    caughtMessage = ex.Message;
                    return "fallback";
                });

            // Assert
            Assert.True(exceptionCaught);
            Assert.Equal(errorMessage, caughtMessage);
            Assert.Equal("fallback", result);
        }

        /// <summary>
        /// 测试 WhenCatch 带异常参数 - 验证非指定异常不处理
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithExceptionParam_OnlyHandlesSpecifiedException()
        {
            // Arrange
            var errorMessage = "测试异常";
            var defaultValue = "not-used";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await CreateFailingTaskWithArgumentException(errorMessage)
                    .Handle()
                    .WhenCatch<InvalidOperationException>(ex => defaultValue));

            Assert.Equal(errorMessage, ex.Message);
        }

        /// <summary>
        /// 测试 WhenCatch 带异常参数 - 验证成功任务不触发异常处理
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithExceptionParam_SuccessfulTaskNotHandled()
        {
            // Arrange
            var expected = "success";
            var handlerCalled = false;

            // Act
            var result = await CreateSuccessfulTask(expected)
                .Handle()
                .WhenCatch<InvalidOperationException>(ex =>
                {
                    handlerCalled = true;
                    return "should-not-happen";
                });

            // Assert
            Assert.Equal(expected, result);
            Assert.False(handlerCalled);
        }

        #endregion

        #region Task<TResult>.Handle().WhenCatch<TException>(Func<TResult> func) 无异常参数版本

        /// <summary>
        /// 测试 WhenCatch 无异常参数 - 验证捕获异常并返回默认值
        /// </summary>
        [Fact]
        public async Task WhenCatch_WithoutExceptionParam_ReturnsDefaultValue()
        {
            // Arrange
            var errorMessage = "测试异常";
            var defaultValue = "default-value";

            // Act
            var result = await CreateFailingTask(errorMessage)
                .Handle()
                .WhenCatch<InvalidOperationException>(() => defaultValue);

            // Assert
            Assert.Equal(defaultValue, result);
        }

        #endregion

        #region Task<TResult>.Handle().WhenCatchAsync<TException>(Func<TException, Task<TResult>> func) 异步版本

        /// <summary>
        /// 测试 WhenCatchAsync - 验证异步处理异常并返回默认值
        /// </summary>
        [Fact]
        public async Task WhenCatchAsync_WithExceptionParam_ReturnsDefaultValue()
        {
            // Arrange
            var errorMessage = "测试异常";
            var defaultValue = "default-value";

            // Act
            var result = await CreateFailingTask(errorMessage)
                .Handle()
                .WhenCatchAsync<InvalidOperationException>(async ex =>
                {
                    await Task.Delay(10); // 模拟异步操作
                    return defaultValue;
                });

            // Assert
            Assert.Equal(defaultValue, result);
        }

        #endregion

        #region Task<TResult>.HandleAsDefaultWhenException() 测试

        /// <summary>
        /// 测试 HandleAsDefaultWhenException - 验证成功任务正常执行
        /// </summary>
        [Fact]
        public async Task HandleAsDefaultWhenException_WithSuccessfulTask_ReturnsResult()
        {
            // Arrange
            var expected = "success";

            // Act
            var result = await CreateSuccessfulTask(expected).HandleAsDefaultWhenException();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试 HandleAsDefaultWhenException - 验证异常时返回默认值
        /// </summary>
        [Fact]
        public async Task HandleAsDefaultWhenException_WithException_ReturnsDefault()
        {
            // Arrange
            var errorMessage = "测试异常";

            // Act
            var result = await CreateFailingTask(errorMessage).HandleAsDefaultWhenException();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// 测试 HandleAsDefaultWhenException - 验证 int 类型返回默认值 0
        /// </summary>
        [Fact]
        public async Task HandleAsDefaultWhenException_WithIntException_ReturnsZero()
        {
            // Arrange
            var errorMessage = "测试异常";
            var expected = 42;

            // Act
            var result = await CreateIntFailingTask(errorMessage, expected)
                .HandleAsDefaultWhenException();

            // Assert
            Assert.Equal(0, result); // int 的默认值是 0
        }

        /// <summary>
        /// 测试 HandleAsDefaultWhenException - 验证引用类型返回 null
        /// </summary>
        [Fact]
        public async Task HandleAsDefaultWhenException_WithObjectException_ReturnsNull()
        {
            // Arrange
            var errorMessage = "测试异常";

            // Act
            var result = await CreateObjectFailingTask(errorMessage).HandleAsDefaultWhenException();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// 测试 HandleAsDefaultWhenException - 验证 bool 类型返回默认值 false
        /// </summary>
        [Fact]
        public async Task HandleAsDefaultWhenException_WithBoolException_ReturnsFalse()
        {
            // Arrange
            var errorMessage = "测试异常";

            // Act
            var result = await CreateBoolFailingTask(errorMessage).HandleAsDefaultWhenException();

            // Assert
            Assert.False(result); // bool 的默认值是 false
        }

        /// <summary>
        /// 测试 HandleAsDefaultWhenException - 验证 null 任务抛出异常
        /// </summary>
        [Fact]
        public async Task HandleAsDefaultWhenException_WithNullTask_ThrowsArgumentNullException()
        {
            // Arrange
            Task<string> task = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await task.HandleAsDefaultWhenException());
            Assert.Equal("task", ex.ParamName);
        }

        #endregion

        #region 综合测试

        /// <summary>
        /// 综合测试 - 验证多种异常类型的链式处理
        /// </summary>
        [Fact]
        public async Task WhenCatch_ChainedExceptionHandling_HandlesMultipleExceptionTypes()
        {
            // Arrange
            var errorMessage = "测试异常";

            // Act
            var result = await CreateFailingTask(errorMessage)
                .Handle()
                .WhenCatch<InvalidOperationException>(ex => "io-handled")
                .WhenCatch<ArgumentException>(ex => "arg-handled");

            // Assert
            Assert.Equal("io-handled", result);
        }

        /// <summary>
        /// 综合测试 - 验证异常处理逻辑中可以使用异常属性
        /// </summary>
        [Fact]
        public async Task WhenCatch_UsesExceptionProperties_ReturnsCustomResult()
        {
            // Arrange
            var errorData = new CustomExceptionData { ErrorCode = 404, Message = "Not Found" };

            // Act
            var result = await CreateCustomFailingTask(errorData)
                .Handle()
                .WhenCatch<CustomExceptionDataException>(ex =>
                {
                    return $"Error: {ex.Data.ErrorCode} - {ex.Data.Message}";
                });

            // Assert
            Assert.Equal("Error: 404 - Not Found", result);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建成功的任务
        /// </summary>
        private async Task<string> CreateSuccessfulTask(string result)
        {
            _logger.WriteLine($"CreateSuccessfulTask 返回: {result}");
            await Task.Delay(50);
            return result;
        }

        /// <summary>
        /// 创建抛出异常的任务
        /// </summary>
        private async Task<string> CreateFailingTask(string errorMessage)
        {
            _logger.WriteLine($"CreateFailingTask 抛出异常: {errorMessage}");
            await Task.Delay(50);
            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// 创建抛出 ArgumentException 的任务
        /// </summary>
        private async Task<string> CreateFailingTaskWithArgumentException(string errorMessage)
        {
            _logger.WriteLine($"CreateFailingTaskWithArgumentException 抛出异常: {errorMessage}");
            await Task.Delay(50);
            throw new ArgumentException(errorMessage);
        }

        /// <summary>
        ///创建返回 int 的失败任务
        /// </summary>
        private async Task<int> CreateIntFailingTask(string errorMessage, int valueBeforeThrow)
        {
            _logger.WriteLine($"CreateIntFailingTask 准备抛出异常，值为: {valueBeforeThrow}");
            await Task.Delay(50);
            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// 创建返回对象的失败任务
        /// </summary>
        private async Task<TestData> CreateObjectFailingTask(string errorMessage)
        {
            _logger.WriteLine($"CreateObjectFailingTask 抛出异常: {errorMessage}");
            await Task.Delay(50);
            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// 创建返回 bool 的失败任务
        /// </summary>
        private async Task<bool> CreateBoolFailingTask(string errorMessage)
        {
            _logger.WriteLine($"CreateBoolFailingTask 抛出异常: {errorMessage}");
            await Task.Delay(50);
            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// 创建抛出自定义异常的任务
        /// </summary>
        private async Task<string> CreateCustomFailingTask(CustomExceptionData data)
        {
            _logger.WriteLine($"CreateCustomFailingTask 抛出异常: {data.Message}");
            await Task.Delay(50);
            throw new CustomExceptionDataException(data);
        }

        #endregion
    }

    /// <summary>
    /// 测试数据类
    /// </summary>
    public class TestData
    {
        public string Value { get; set; }
    }

    /// <summary>
    /// 自定义异常数据
    /// </summary>
    public class CustomExceptionData
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 自定义异常数据异常
    /// </summary>
    public class CustomExceptionDataException : Exception
    {
        public CustomExceptionData Data { get; }

        public CustomExceptionDataException(CustomExceptionData data)
            : base(data.Message)
        {
            Data = data;
        }
    }
}
