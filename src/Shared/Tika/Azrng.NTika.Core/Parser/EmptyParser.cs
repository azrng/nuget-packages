using System.Collections.Generic;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Core.Parser
{
    public class EmptyParser : IParser
    {
        public static readonly EmptyParser Instance = new();

        private EmptyParser() { }

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType>();
        }

        public void Parse(TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();
            xhtml.EndDocument();
        }
    }
}
