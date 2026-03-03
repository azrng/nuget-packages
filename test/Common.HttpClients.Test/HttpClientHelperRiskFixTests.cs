using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.Options;
using System.Net;
using Xunit;

namespace Common.HttpClients.Test
{
    public class HttpClientHelperRiskFixTests
    {
        [Fact]
        public async Task SendAsync_GetWithNullContent_ShouldReturnResponse()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content = new StringContent("ok")
                                })));

            var helper = CreateHelper(client);
            var result = await helper.SendAsync(HttpRequestEnum.Get, "https://unit.test/health", null);

            Assert.Equal("ok", result);
        }

        [Fact]
        public async Task PostSoapAsync_ShouldUseSoapContentType()
        {
            string mediaType = null;

            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                mediaType = request.Content?.Headers.ContentType?.MediaType;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("soap-ok")
                                       });
            }));

            var helper = CreateHelper(client);
            var result = await helper.PostSoapAsync<string>("https://unit.test/soap", "<xml/>");

            Assert.Equal("soap-ok", result);
            Assert.Equal("application/soap+xml", mediaType);
        }

        [Fact]
        public async Task GetAsync_WithCancellationToken_ShouldCancel()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler(async (_, cancellation) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellation);
                return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StringContent("slow")
                       };
            }));

            var helper = CreateHelper(client);
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await helper.GetAsync("https://unit.test/slow", cancellation: cts.Token));
        }

        [Fact]
        public async Task GetAsync_WithTimeoutArgument_ShouldNotApplyLocalTimeout()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler(async (_, cancellation) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1200), cancellation);
                return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StringContent("slow-but-ok")
                       };
            }));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync("https://unit.test/slow", timeout: 1, cancellation: CancellationToken.None);

            Assert.Equal("slow-but-ok", result);
        }

        private static HttpClientHelper CreateHelper(HttpClient client)
        {
            var logger = new ListLogger<HttpClientHelper>();
            var options = Options.Create(new HttpClientOptions
                                         {
                                             FailThrowException = true,
                                             Timeout = 100
                                         });
            return new HttpClientHelper(client, options, logger);
        }
    }
}
