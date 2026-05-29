using System.Collections.Generic;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Parser
{
    public abstract class ParserDecorator : IParser
    {
        protected readonly IParser WrappedParser;

        protected ParserDecorator(IParser parser)
        {
            WrappedParser = parser;
        }

        public virtual ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return WrappedParser.GetSupportedTypes(context);
        }

        public virtual void Parse(TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            WrappedParser.Parse(stream, handler, metadata, context);
        }
    }
}
