using System.Text;

namespace Azrng.NTika.Core.Model
{
    public class EncodingResult
    {
        public EncodingResult(Encoding encoding, EncodingConfidence confidence, string detectorName)
        {
            Encoding = encoding;
            Confidence = confidence;
            DetectorName = detectorName;
        }

        public Encoding Encoding { get; }
        public EncodingConfidence Confidence { get; }
        public string DetectorName { get; }

        public override string ToString()
        {
            return $"{Encoding.WebName} ({Confidence}, {DetectorName})";
        }
    }
}
