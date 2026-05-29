using System.IO;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Extractor
{
    public class ParsingEmbeddedDocumentExtractor : IEmbeddedDocumentExtractor
    {
        private int _embeddedCount;
        private readonly EmbeddedLimits _limits;

        public ParsingEmbeddedDocumentExtractor(EmbeddedLimits limits)
        {
            _limits = limits;
        }

        public bool ShouldParseEmbedded(Metadata metadata)
        {
            if (_limits.MaxEmbeddedResources > 0 && _embeddedCount >= _limits.MaxEmbeddedResources)
                return false;

            if (_limits.MaxEmbeddedCount > 0 && _embeddedCount >= _limits.MaxEmbeddedCount)
                return false;

            return true;
        }

        public void ParseEmbedded(Stream stream, IContentHandler handler, Metadata metadata, ParseContext context)
        {
            _embeddedCount++;

            var parser = context.Get<IParser>();
            if (parser == null)
                return;

            using var tis = TikaInputStream.Get(stream);
            parser.Parse(tis, handler, metadata, context);
        }
    }
}
