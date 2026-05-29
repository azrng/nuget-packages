using System;
using System.Collections.Generic;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Detect;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Core.Parser
{
    public class AutoDetectParser : CompositeParser
    {
        private readonly IDetector _detector;

        public AutoDetectParser()
            : this(new DefaultDetector(), new MediaTypeRegistry())
        {
        }

        public AutoDetectParser(IDetector detector, MediaTypeRegistry registry, params IParser[] parsers)
            : base(registry, parsers)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        public AutoDetectParser(MediaTypeRegistry registry, params IParser[] parsers)
            : this(new DefaultDetector(registry), registry, parsers)
        {
        }

        public IDetector Detector => _detector;

        public override void Parse(TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            // Detect the media type
            var mediaType = _detector.Detect(stream, metadata, context);
            if (mediaType != null)
            {
                metadata.Set(TikaCoreProperties.CONTENT_TYPE, mediaType.ToString());
            }

            // Check for zero-byte files
            if (stream.Length == 0)
            {
                throw new ZeroByteFileException();
            }

            // Wrap handler with secure handler for zip bomb protection
            var secureHandler = new SecureContentHandler(handler, stream);

            // Delegate to composite parser
            base.Parse(stream, secureHandler, metadata, context);
        }
    }
}
