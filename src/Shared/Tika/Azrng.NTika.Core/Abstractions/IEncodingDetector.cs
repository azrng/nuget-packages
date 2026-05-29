using System.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Abstractions
{
    public interface IEncodingDetector
    {
        EncodingResult? Detect(Stream stream, Metadata metadata, ParseContext context);
    }
}
