using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using UtfUnknown;

namespace Azrng.NTika.EncodingDetectors
{
    public class UniversalEncodingDetector : IEncodingDetector
    {
        private const int SampleSize = 8192;

        public EncodingResult? Detect(Stream stream, Metadata metadata, ParseContext context)
        {
            if (!stream.CanSeek)
                return null;

            var savedPosition = stream.Position;
            stream.Position = 0;

            try
            {
                var buffer = new byte[SampleSize];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    return null;

                if (bytesRead < buffer.Length)
                {
                    var trimmed = new byte[bytesRead];
                    System.Array.Copy(buffer, trimmed, bytesRead);
                    buffer = trimmed;
                }

                var detection = CharsetDetector.DetectFromBytes(buffer);

                if (detection?.Detected?.Encoding == null)
                    return null;

                var encoding = detection.Detected.Encoding;
                var confidence = detection.Detected.Confidence switch
                {
                    > 0.9f => EncodingConfidence.HIGH,
                    > 0.7f => EncodingConfidence.MEDIUM,
                    > 0.5f => EncodingConfidence.LOW,
                    _ => EncodingConfidence.TENTATIVE
                };

                return new EncodingResult(encoding, confidence, nameof(UniversalEncodingDetector));
            }
            finally
            {
                stream.Position = savedPosition;
            }
        }
    }
}
