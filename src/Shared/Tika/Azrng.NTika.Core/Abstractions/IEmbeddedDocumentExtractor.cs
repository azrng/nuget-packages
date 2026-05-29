using System.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Abstractions
{
    public interface IEmbeddedDocumentExtractor
    {
        bool ShouldParseEmbedded(Metadata metadata);
        void ParseEmbedded(Stream stream, IContentHandler handler, Metadata metadata, ParseContext context);
    }
}
