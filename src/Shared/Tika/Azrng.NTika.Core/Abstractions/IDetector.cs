using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Abstractions
{
    public interface IDetector
    {
        MediaType Detect(IO.TikaInputStream? stream, Metadata metadata, ParseContext parseContext);
    }
}
