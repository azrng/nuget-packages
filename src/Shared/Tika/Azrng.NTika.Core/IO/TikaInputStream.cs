using System;
using System.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.IO
{
    public class TikaInputStream : Stream
    {
        private readonly Stream _stream;
        private readonly FileInfo? _file;
        private readonly TemporaryResources? _resources;

        private TikaInputStream(Stream stream, FileInfo? file, TemporaryResources? resources)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _file = file;
            _resources = resources;
        }

        public static TikaInputStream Get(Stream stream)
        {
            if (stream is TikaInputStream tis)
            {
                return tis;
            }

            if (stream is FileStream fs)
            {
                return new TikaInputStream(stream, new FileInfo(fs.Name), null);
            }

            return new TikaInputStream(stream, null, null);
        }

        public static TikaInputStream Get(byte[] data)
        {
            return new TikaInputStream(new MemoryStream(data), null, null);
        }

        public static TikaInputStream Get(byte[] data, Metadata metadata)
        {
            metadata.Set("Content-Length", data.Length.ToString());
            return Get(data);
        }

        public static TikaInputStream Get(FileInfo file)
        {
            var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new TikaInputStream(stream, file, null);
        }

        public static TikaInputStream Get(string path)
        {
            return Get(new FileInfo(path));
        }

        public FileInfo? GetFile()
        {
            return _file;
        }

        public bool HasFile()
        {
            return _file != null;
        }

        public void Rewind()
        {
            if (_stream.CanSeek)
            {
                _stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                throw new InvalidOperationException("Stream does not support seeking");
            }
        }

        public long CurrentPosition => _stream.CanSeek ? _stream.Position : 0;

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.CanSeek ? _stream.Position : 0;
            set
            {
                if (_stream.CanSeek)
                {
                    _stream.Position = value;
                }
                else
                {
                    throw new InvalidOperationException("Stream does not support seeking");
                }
            }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
                _resources?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
