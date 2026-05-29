using System;
using System.IO;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Detect
{
    public class TextDetector : IDetector
    {
        private const int NumberOfBytesToCheck = 512;

        public MediaType Detect(TikaInputStream? stream, Metadata metadata, ParseContext parseContext)
        {
            if (stream == null)
            {
                return MediaType.OctetStream;
            }

            var buffer = new byte[NumberOfBytesToCheck];
            var bytesRead = 0;

            try
            {
                stream.Rewind();
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                return MediaType.OctetStream;
            }

            if (bytesRead == 0)
            {
                return MediaType.Empty;
            }

            // Check for binary content (null bytes or high concentration of control characters)
            var nonTextBytes = 0;
            for (var i = 0; i < bytesRead; i++)
            {
                var b = buffer[i];
                if (b == 0)
                {
                    // Null byte strongly suggests binary
                    return MediaType.OctetStream;
                }

                // Count non-text control characters (except common ones like tab, newline, carriage return)
                if (b < 0x20 && b != 0x09 && b != 0x0A && b != 0x0D)
                {
                    nonTextBytes++;
                }
            }

            // If more than 10% are non-text control characters, consider it binary
            if (nonTextBytes > bytesRead * 0.1)
            {
                return MediaType.OctetStream;
            }

            return MediaType.TextPlain;
        }
    }
}
