using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.Options;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// GetStreamAsync / DownloadFileAsync 资源管理测试
    /// </summary>
    public class ResponseStreamTests
    {
        [Fact]
        public async Task GetStreamAsync_Success_ShouldReturnReadableStream()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("stream content")
            });
            var helper = CreateHelper(client);

            var result = await helper.GetStreamAsync("https://unit.test/stream");

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            using var reader = new StreamReader(result.Data);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("stream content", content);
        }

        [Fact]
        public async Task GetStreamAsync_Failure_WhenFailThrowDisabled_ShouldReturnFailedResult()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error")
            });
            var helper = CreateHelper(client, failThrowException: false);

            var result = await helper.GetStreamAsync("https://unit.test/error");

            Assert.False(result.IsSuccess);
            Assert.Equal("error", result.ErrorMessage);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetStreamAsync_Failure_WhenFailThrowEnabled_ShouldThrowHttpRequestException()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error")
            });
            var helper = CreateHelper(client, failThrowException: true);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                helper.GetStreamAsync("https://unit.test/error"));
        }

        [Fact]
        public async Task GetStreamAsync_LargeContent_ShouldStreamCorrectly()
        {
            var large = new string('A', 100_000);
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(large)
            });
            var helper = CreateHelper(client);

            var result = await helper.GetStreamAsync("https://unit.test/large");

            using var reader = new StreamReader(result.Data!);
            var content = await reader.ReadToEndAsync();
            Assert.Equal(100_000, content.Length);
        }

        [Fact]
        public async Task GetStreamAsync_DisposeStream_MultipleTimes_ShouldNotThrow()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("test")
            });
            var helper = CreateHelper(client);

            var result = await helper.GetStreamAsync("https://unit.test/multi");

            Assert.True(result.IsSuccess);
            await result.Data!.DisposeAsync();
            await result.Data.DisposeAsync();
        }

        [Fact]
        public async Task DownloadFileAsync_Success_ShouldSaveToFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"nextdl_{Guid.NewGuid():N}.txt");
            try
            {
                using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("file-content")
                });
                var helper = CreateHelper(client);

                var result = await helper.DownloadFileAsync("https://unit.test/file", tempPath);

                Assert.True(result.IsSuccess);
                Assert.Equal(tempPath, result.Data?.FilePath);
                Assert.Equal("file-content".Length, result.Data?.FileSize);
                Assert.True(File.Exists(tempPath));
                Assert.Equal("file-content", await File.ReadAllTextAsync(tempPath));
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public async Task DownloadFileAsync_Failure_ShouldNotLeavePartialFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"nextdl_{Guid.NewGuid():N}.txt");
            try
            {
                using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("error")
                });
                var helper = CreateHelper(client, failThrowException: false);

                var result = await helper.DownloadFileAsync("https://unit.test/file", tempPath);

                Assert.False(result.IsSuccess);
                Assert.False(File.Exists(tempPath));
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldCreateMissingDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"nextdl_{Guid.NewGuid():N}");
            var filePath = Path.Combine(tempDir, "sub", "file.txt");
            try
            {
                using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                });
                var helper = CreateHelper(client);

                var result = await helper.DownloadFileAsync("https://unit.test/file", filePath);

                Assert.True(result.IsSuccess);
                Assert.True(File.Exists(filePath));
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        private static HttpClient NewClient(Func<HttpRequestMessage, HttpResponseMessage> factory)
        {
            return new HttpClient(new DelegateHttpMessageHandler((r, _) => Task.FromResult(factory(r))));
        }

        private static HttpClientHelper CreateHelper(HttpClient client, bool failThrowException = false)
        {
            var logger = new ListLogger<HttpClientHelper>();
            var options = Options.Create(new HttpClientOptions
            {
                FailThrowException = failThrowException,
                Timeout = 100
            });
            return new HttpClientHelper(client, options, logger);
        }
    }
}
