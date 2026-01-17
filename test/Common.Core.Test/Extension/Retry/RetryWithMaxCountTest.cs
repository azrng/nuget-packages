// using Azrng.Core.Exceptions;
// using Azrng.Core.RetryTask;
// using Xunit.Abstractions;
//
// namespace Common.Core.Test.Extension.Retry
// {
//     public class RetryWithMaxCountTest
//     {
//         private readonly ITestOutputHelper _testOutputHelper;
//
//         public RetryWithMaxCountTest(ITestOutputHelper testOutputHelper)
//         {
//             _testOutputHelper = testOutputHelper;
//         }
//
//         [Fact]
//         public async Task Retry_WithMaxCount_ShouldWork()
//         {
//             // Arrange
//             var task = CreateSuccessfulTaskAsync("42");
//             const int maxCount = 3;
//
//             // Act
//             var retryTask = task.Retry(maxCount);
//
//             // Assert
//             Assert.NotNull(retryTask);
//             var result = await retryTask;
//             Assert.Equal("42", result.Name);
//         }
//
//         /// <summary>
//         /// 无效的最大重试次数 抛出异常参数溢出
//         /// </summary>
//         [Fact]
//         public void Retry_WithInvalidMaxCount_ThrowsArgumentOutOfRangeException()
//         {
//             // Arrange
//             var task = CreateSuccessfulTaskAsync("error");
//             const int invalidMaxCount = 0;
//
//             // Act & Assert
//             var ex = Assert.Throws<ArgumentOutOfRangeException>(() => task.Retry(invalidMaxCount));
//             _testOutputHelper.WriteLine(ex.ParamName);
//             Assert.Equal("maxCount", ex.ParamName);
//         }
//
//         [Fact]
//         public void Retry_WithNullTask_ThrowsArgumentNullException()
//         {
//             // Arrange
//             Task<int> task = null;
//
//             // Act & Assert
//             var ex = Assert.Throws<ArgumentNullException>(() => task.Retry(3));
//             Assert.Equal("task", ex.ParamName);
//         }
//
//         #region Helper Methods
//
//         private async Task<ReplayUserInfo> CreateSuccessfulTaskAsync(string str)
//         {
//             var num = RandomGenerator.GenerateNumber(1, 100);
//             _testOutputHelper.WriteLine($"输出当前的随机数  {num} {str}");
//             if (str == "error")
//             {
//                 throw new ParameterException("参数异常");
//             }
//
//             await Task.Delay(100);
//
//             return new ReplayUserInfo { UserId = "1", Name = str };
//         }
//
//         #endregion
//     }
//
//     public class ReplayUserInfo
//     {
//         public string UserId { get; set; }
//
//         public string Name { get; set; }
//     }
// }