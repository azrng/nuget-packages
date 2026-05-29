using System.IO;
using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.Exception;

namespace Azrng.NTika.Core.Extractor
{
    public static class EmbeddedStreamLimiter
    {
        private const int BufferSize = 81920;

        public static void EnsureSizeAllowed(long size, EmbeddedLimits limits, string resourceName)
        {
            if (limits.MaxEmbeddedBytes >= 0 && size > limits.MaxEmbeddedBytes)
            {
                throw new EmbeddedLimitReachedException(
                    $"Embedded resource '{resourceName}' is {size} bytes, which exceeds the configured limit of {limits.MaxEmbeddedBytes} bytes.");
            }
        }

        public static void CopyTo(Stream source, Stream destination, EmbeddedLimits limits, string resourceName)
        {
            var buffer = new byte[BufferSize];
            long totalRead = 0;

            while (true)
            {
                var bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                totalRead += bytesRead;
                EnsureSizeAllowed(totalRead, limits, resourceName);
                destination.Write(buffer, 0, bytesRead);
            }
        }
    }
}
