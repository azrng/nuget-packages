using System.Collections.Generic;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Abstractions
{
    public interface IParser
    {
        ISet<MediaType> GetSupportedTypes(ParseContext context);
        void Parse(IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context);
    }
}
