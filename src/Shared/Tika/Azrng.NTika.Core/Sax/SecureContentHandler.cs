using System;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.IO;

namespace Azrng.NTika.Core.Sax
{
    public class SecureContentHandler : ContentHandlerDecorator
    {
        private const long MaxCompressionRatio = 100;
        private const int MaxDepth = 100;
        private const int MaxPackageEntryNestingDepth = 100;

        private readonly TikaInputStream _stream;
        private long _characterCount;
        private int _depth;
        private long _startByte;

        public SecureContentHandler(IContentHandler handler, TikaInputStream stream)
            : base(handler)
        {
            _stream = stream;
        }

        public override void StartDocument()
        {
            _startByte = _stream.CurrentPosition;
            base.StartDocument();
        }

        public override void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            _depth++;
            if (_depth > MaxPackageEntryNestingDepth)
            {
                throw new TikaException(
                    $"Depth ({_depth}) exceeds maximum package entry nesting depth ({MaxPackageEntryNestingDepth}). " +
                    "This may indicate a zip bomb.");
            }
            base.StartElement(uri, localName, qName, atts);
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _depth--;
            base.EndElement(uri, localName, qName);
        }

        public override void Characters(char[] ch, int start, int length)
        {
            _characterCount += length;
            if (_stream.CurrentPosition > _startByte)
            {
                var ratio = (double)_characterCount / (_stream.CurrentPosition - _startByte);
                if (ratio > MaxCompressionRatio)
                {
                    throw new TikaException(
                        $"Compression ratio ({ratio}) exceeds maximum ({MaxCompressionRatio}). " +
                        "This may indicate a zip bomb.");
                }
            }
            base.Characters(ch, start, length);
        }
    }
}
