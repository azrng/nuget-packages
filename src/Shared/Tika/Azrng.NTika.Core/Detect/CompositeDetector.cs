using System.Collections.Generic;
using System.Linq;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Detect
{
    public class CompositeDetector : IDetector
    {
        private readonly List<IDetector> _detectors;
        private readonly MediaTypeRegistry _registry;

        public CompositeDetector(MediaTypeRegistry registry, params IDetector[] detectors)
        {
            _registry = registry;
            _detectors = new List<IDetector>(detectors);
        }

        public CompositeDetector(MediaTypeRegistry registry, IEnumerable<IDetector> detectors)
        {
            _registry = registry;
            _detectors = new List<IDetector>(detectors);
        }

        public MediaType Detect(TikaInputStream? stream, Metadata metadata, ParseContext parseContext)
        {
            var type = MediaType.OctetStream;

            foreach (var detector in _detectors)
            {
                var detected = detector.Detect(stream, metadata, parseContext);
                if (detected != null && !detected.Equals(MediaType.OctetStream))
                {
                    if (type.Equals(MediaType.OctetStream) ||
                        _registry.IsSpecializationOf(detected, type))
                    {
                        type = detected;
                    }
                }
            }

            return type;
        }
    }
}
