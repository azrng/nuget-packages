using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common.HttpClients.Test.Helpers
{
    internal sealed class ScriptedHttpListenerServer : IAsyncDisposable
    {
        private readonly HttpListener _listener;
        private readonly Func<HttpListenerContext, Task> _handler;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _loopTask;

        public string BaseUrl { get; }

        public ScriptedHttpListenerServer(Func<HttpListenerContext, Task> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            var port = GetFreePort();
            BaseUrl = $"http://127.0.0.1:{port}/";

            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _listener.Start();
            _loopTask = Task.Run(ListenLoopAsync);
        }

        public static async Task WriteResponseAsync(HttpListenerContext context, HttpStatusCode statusCode, string body)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
            catch
            {
                // 请求已被客户端取消时，写响应可能抛错，测试场景下可忽略
            }
            finally
            {
                try
                {
                    context.Response.OutputStream.Close();
                    context.Response.Close();
                }
                catch
                {
                    // ignore
                }
            }
        }

        private async Task ListenLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().WaitAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (HttpListenerException)
                {
                    break;
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _handler(context).ConfigureAwait(false);
                    }
                    catch
                    {
                        try
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.OutputStream.Close();
                            context.Response.Close();
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                });
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _listener.Stop();
            _listener.Close();

            try
            {
                await _loopTask.ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }

            _cts.Dispose();
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}