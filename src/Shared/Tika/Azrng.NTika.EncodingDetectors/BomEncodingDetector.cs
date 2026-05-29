using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.EncodingDetectors
{
    public class BomEncodingDetector : IEncodingDetector
    {
        public EncodingResult? Detect(Stream stream, Metadata metadata, ParseContext context)
        {
            if (!stream.CanSeek)
                return null;

            var savedPosition = stream.Position;
            stream.Position = 0;

            try
            {
                var bom = new byte[4];
                var bytesRead = stream.Read(bom, 0, bom.Length);

                if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                    return new EncodingResult(Encoding.UTF8, EncodingConfidence.HIGH, nameof(BomEncodingDetector));

                if (bytesRead >= 4 && bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
                    return new EncodingResult(Encoding.UTF32, EncodingConfidence.HIGH, nameof(BomEncodingDetector));

                if (bytesRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                    return new EncodingResult(Encoding.Unicode, EncodingConfidence.HIGH, nameof(BomEncodingDetector));

                if (bytesRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
                    return new EncodingResult(Encoding.BigEndianUnicode, EncodingConfidence.HIGH, nameof(BomEncodingDetector));

                return null;
            }
            finally
            {
                stream.Position = savedPosition;
            }
        }
    }
}
