using System;
using System.Collections.Generic;
using System.Linq;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Parser
{
    public class CompositeParser : IParser
    {
        private readonly List<IParser> _parsers;
        private readonly MediaTypeRegistry _registry;
        private IParser _fallback;

        public CompositeParser(MediaTypeRegistry registry, params IParser[] parsers)
        {
            _registry = registry;
            _parsers = new List<IParser>(parsers);
            _fallback = EmptyParser.Instance;
        }

        public CompositeParser(MediaTypeRegistry registry, IEnumerable<IParser> parsers)
        {
            _registry = registry;
            _parsers = new List<IParser>(parsers);
            _fallback = EmptyParser.Instance;
        }

        public IParser Fallback
        {
            get => _fallback;
            set => _fallback = value ?? EmptyParser.Instance;
        }

        public IReadOnlyList<IParser> Parsers => _parsers;

        public virtual ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            var types = new HashSet<MediaType>();
            foreach (var parser in _parsers)
            {
                types.UnionWith(parser.GetSupportedTypes(context));
            }
            return types;
        }

        public virtual void Parse(TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var parser = GetParser(metadata, context);
            try
            {
                parser.Parse(stream, handler, metadata, context);
            }
            catch (TikaException)
            {
                throw;
            }
            catch (System.Exception e)
            {
                throw new TikaException("Unexpected error parsing document", e);
            }
        }

        public IParser GetParser(Metadata metadata, ParseContext context)
        {
            var typeName = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            if (!string.IsNullOrEmpty(typeName))
            {
                var type = MediaType.Parse(typeName);
                if (type != null)
                {
                    type = _registry.Normalize(type) ?? type;
                    return FindParser(type, context);
                }
            }

            return _fallback;
        }

        private IParser FindParser(MediaType type, ParseContext context)
        {
            // Direct match
            foreach (var parser in _parsers)
            {
                foreach (var supported in parser.GetSupportedTypes(context))
                {
                    if (supported.Equals(type))
                    {
                        return parser;
                    }
                }
            }

            // Walk up the type hierarchy
            var current = type;
            while (current != null)
            {
                foreach (var parser in _parsers)
                {
                    foreach (var supported in parser.GetSupportedTypes(context))
                    {
                        if (supported.Equals(current))
                        {
                            return parser;
                        }
                    }
                }
                current = _registry.GetSupertype(current);
            }

            return _fallback;
        }
    }
}
