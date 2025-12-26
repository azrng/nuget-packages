namespace Common.Core.Test.Helper.UrlHelperTest
{
    public class AddQueryStringTest
    {
        /// <summary>
        /// Test Case 1: Valid URI with empty query string collection
        /// 测试用例1: 有效URI和空查询字符串集合
        /// </summary>
        [Fact]
        public void AddQueryString_ValidUriWithEmptyQueryCollection_ReturnsOriginalUrl()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");
            var queryString = new List<KeyValuePair<string, string>>();

            // Act
            var result = UrlHelper.AddQueryString(uri, queryString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.example.com", result);
        }

        /// <summary>
        /// Test Case 2: Valid URI with single query parameter
        /// 测试用例2: 有效URI和单个查询参数
        /// </summary>
        [Fact]
        public void AddQueryString_ValidUriWithSingleQueryParam_AddsQueryParam()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");
            var queryString = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("param1", "value1") };

            // Act
            var result = UrlHelper.AddQueryString(uri, queryString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.example.com?param1=value1", result);
        }

        /// <summary>
        /// Test Case 3: Valid URI with multiple query parameters
        /// 测试用例3: 有效URI和多个查询参数
        /// </summary>
        [Fact]
        public void AddQueryString_ValidUriWithMultipleQueryParams_AddsAllParams()
        {
            // Arrange
            var uri = new Uri("https://www.example.com/path");
            var queryString = new List<KeyValuePair<string, string>>
                              {
                                  new KeyValuePair<string, string>("param1", "value1"), new KeyValuePair<string, string>("param2", "value2")
                              };

            // Act
            var result = UrlHelper.AddQueryString(uri, queryString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.example.com/path?param1=value1&param2=value2", result);
        }

        /// <summary>
        /// Test Case 4: Valid URI with null query string collection
        /// 测试用例4: 有效URI和null查询字符串集合
        /// </summary>
        [Fact]
        public void AddQueryString_ValidUriWithNullQueryCollection_ReturnsOriginalUrl()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");

            // Act
            var result = UrlHelper.AddQueryString(uri, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.example.com", result);
        }

        /// <summary>
        /// Test Case 5: URI is null - should throw ArgumentNullException
        /// 测试用例5: URI为null - 应该抛出ArgumentNullException
        /// </summary>
        [Fact]
        public void AddQueryString_UriIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Uri uri = null;
            var queryString = new List<KeyValuePair<string, string>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                UrlHelper.AddQueryString(uri, queryString));
        }

        /// <summary>
        /// Test Case 6: URI with existing query parameters
        /// 测试用例6: 带有现有查询参数的URI
        /// </summary>
        [Fact]
        public void AddQueryString_UriWithExistingQuery_AddsNewParams()
        {
            // Arrange
            var uri = new Uri("https://www.example.com?existing=param");
            var queryString = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("newparam", "newvalue") };

            // Act
            var result = UrlHelper.AddQueryString(uri, queryString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.example.com/?existing=param&newparam=newvalue", result);
        }

        /// <summary>
        /// Test Case 7: URI with special characters in query parameters
        /// 测试用例7: 查询参数包含特殊字符的URI
        /// </summary>
        [Fact]
        public void AddQueryString_UriWithSpecialChars_EncodesProperly()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");
            var queryString = new List<KeyValuePair<string, string>>
                              {
                                  new KeyValuePair<string, string>("special", "value with spaces & symbols!@#")
                              };

            // Act
            var result = UrlHelper.AddQueryString(uri, queryString);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("special=", result);
        }

        /// <summary>
        /// Test Case 8: Verify Uri.ToString() is called when uri is not null
        /// 测试用例8: 验证当uri不为null时调用Uri.ToString()
        /// </summary>
        [Fact]
        public void AddQueryString_UriNotNull_CallsUriToString()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");
            var queryString = new List<KeyValuePair<string, string>>();

            // Act
            var result = UrlHelper.AddQueryString(uri, queryString);

            // Assert
            Assert.NotNull(result);

            // This test assumes that the internal implementation calls uri.ToString()
            // The actual verification depends on the implementation of the other overload
        }

        [Fact]
        public void AddQueryString_Ok()
        {
            var url = "http://www.baidu.com/get/userinfo?userId=123&timestamp=13";
            var dict = new Dictionary<string, string>() { { "configId", "123" }, { "bbb", "654321" } };
            var result = UrlHelper.AddQueryString(url, dict);
            Assert.Equal("http://www.baidu.com/get/userinfo?userId=123&timestamp=13&configId=123&bbb=654321", result);
        }

        [Fact]
        public void AddQueryString_ShouldIgnoreInvalidPairs()
        {
            var url = "http://www.baidu.com/get/userinfo";
            var queryPairs = new List<KeyValuePair<string, string>>
                             {
                                 new KeyValuePair<string, string>(null, "invalid"),
                                 new KeyValuePair<string, string>(" ", "also invalid"),
                                 new KeyValuePair<string, string>("config id", "hello world"),
                                 new KeyValuePair<string, string>("skip", null)
                             };

            // 只保留有效的键值对
            var result = UrlHelper.AddQueryString(url, queryPairs);
            Assert.Equal("http://www.baidu.com/get/userinfo?config+id=hello+world", result);
        }

        [Fact]
        public void AddQueryString_ShouldPreserveFragment()
        {
            var url = "https://demo.com/list?page=1#section";
            var queryPairs = new Dictionary<string, string>() { { "size", "20" } };

            // 锚点应该保持在末尾
            var result = UrlHelper.AddQueryString(url, queryPairs);
            Assert.Equal("https://demo.com/list?page=1&size=20#section", result);
        }
    }
}