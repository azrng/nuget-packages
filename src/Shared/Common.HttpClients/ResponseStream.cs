using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// 包装响应流，确保响应消息在流释放时也被正确释放
    /// </summary>
    internal sealed class ResponseStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly HttpResponseMessage _response;
        private bool _disposed;

        /// <summary>
        /// 初始化 <see cref="ResponseStream"/> 的新实例
        /// </summary>
        /// <param name="innerStream">内部响应流</param>
        /// <param name="response">HTTP响应消息</param>
        public ResponseStream(Stream innerStream, HttpResponseMessage response)
        {
            _innerStream = innerStream;
            _response = response;
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// 释放流和响应消息使用的资源
        /// </summary>
        /// <param name="disposing">是否在释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _innerStream.Dispose();
                _response.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }

        /// <summary>
        /// 异步释放流和响应消息使用的资源
        /// </summary>
        /// <returns>表示异步释放操作的值任务</returns>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                await base.DisposeAsync().ConfigureAwait(false);
                return;
            }

            _disposed = true;
            await _innerStream.DisposeAsync().ConfigureAwait(false);
            await base.DisposeAsync().ConfigureAwait(false);
            _response.Dispose();
        }
    }
}
