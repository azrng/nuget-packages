using System;
using System.IO;
using Azrng.NTika.Core.Config;

namespace Azrng.NTika.Core.Extractor
{
    public class LimitedMemoryStream : MemoryStream
    {
        private readonly EmbeddedLimits _limits;
        private readonly string _resourceName;

        public LimitedMemoryStream(EmbeddedLimits limits, string resourceName)
        {
            _limits = limits;
            _resourceName = resourceName;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureAllowed(count);
            base.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureAllowed(buffer.Length);
            base.Write(buffer);
        }

        public override void WriteByte(byte value)
        {
            EnsureAllowed(1);
            base.WriteByte(value);
        }

        private void EnsureAllowed(int count)
        {
            EmbeddedStreamLimiter.EnsureSizeAllowed(Position + count, _limits, _resourceName);
        }
    }
}
