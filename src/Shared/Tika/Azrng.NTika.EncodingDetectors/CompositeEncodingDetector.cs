using System.Collections.Generic;
using System.IO;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.EncodingDetectors
{
    public class CompositeEncodingDetector : IEncodingDetector
    {
        private readonly List<IEncodingDetector> _detectors;

        public CompositeEncodingDetector(params IEncodingDetector[] detectors)
        {
            _detectors = new List<IEncodingDetector>(detectors);
        }

        public CompositeEncodingDetector(IEnumerable<IEncodingDetector> detectors)
        {
            _detectors = new List<IEncodingDetector>(detectors);
        }

        public EncodingResult? Detect(Stream stream, Metadata metadata, ParseContext context)
        {
            EncodingResult? bestResult = null;
            var bestConfidence = EncodingConfidence.NONE;

            foreach (var detector in _detectors)
            {
                if (!stream.CanSeek)
                    break;

                var savedPosition = stream.Position;
                var result = detector.Detect(stream, metadata, context);

                if (result != null && result.Confidence > bestConfidence)
                {
                    bestResult = result;
                    bestConfidence = result.Confidence;

                    if (bestConfidence == EncodingConfidence.HIGH)
                        break;
                }

                stream.Position = savedPosition;
            }

            return bestResult;
        }
    }
}
