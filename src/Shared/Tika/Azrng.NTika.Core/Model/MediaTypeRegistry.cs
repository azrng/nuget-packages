using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Azrng.NTika.Core.Model
{
    public class MediaTypeRegistry
    {
        private readonly ConcurrentDictionary<MediaType, MediaType> _registry = new();
        private readonly ConcurrentDictionary<MediaType, MediaType> _inheritance = new();

        public ISet<MediaType> Types => new HashSet<MediaType>(_registry.Values.Distinct());

        public ISet<MediaType> GetAliases(MediaType type)
        {
            var aliases = new HashSet<MediaType>();
            foreach (var entry in _registry)
            {
                if (entry.Value.Equals(type) && !entry.Key.Equals(type))
                {
                    aliases.Add(entry.Key);
                }
            }
            return aliases;
        }

        public ISet<MediaType> GetChildTypes(MediaType type)
        {
            var children = new HashSet<MediaType>();
            foreach (var entry in _inheritance)
            {
                if (entry.Value.Equals(type))
                {
                    children.Add(entry.Key);
                }
            }
            return children;
        }

        public void AddType(MediaType type)
        {
            _registry[type] = type;
        }

        public void AddAlias(MediaType type, MediaType alias)
        {
            _registry[alias] = type;
        }

        public void AddSuperType(MediaType type, MediaType supertype)
        {
            _inheritance[type] = supertype;
        }

        public MediaType? Normalize(MediaType? type)
        {
            if (type == null)
            {
                return null;
            }

            var baseType = type.BaseType;
            if (_registry.TryGetValue(baseType, out var canonical))
            {
                if (type.HasParameters)
                {
                    return new MediaType(canonical.Type, canonical.Subtype, type.Parameters.ToDictionary(k => k.Key, v => v.Value));
                }
                return canonical;
            }

            return type;
        }

        public bool IsSpecializationOf(MediaType a, MediaType b)
        {
            return IsInstanceOf(GetSupertype(a), b);
        }

        public bool IsInstanceOf(MediaType? a, MediaType b)
        {
            return a != null && (a.Equals(b) || IsSpecializationOf(a, b));
        }

        public bool IsInstanceOf(string a, MediaType b)
        {
            var parsed = MediaType.Parse(a);
            return parsed != null && IsInstanceOf(Normalize(parsed), b);
        }

        public MediaType? GetSupertype(MediaType? type)
        {
            if (type == null)
            {
                return null;
            }

            if (_inheritance.TryGetValue(type, out var supertype))
            {
                return supertype;
            }

            if (type.HasParameters)
            {
                return type.BaseType;
            }

            if (type.Subtype.EndsWith("+xml"))
            {
                return MediaType.ApplicationXml;
            }

            if (type.Subtype.EndsWith("+zip"))
            {
                return MediaType.ApplicationZip;
            }

            if (type.Type == "text" && !type.Equals(MediaType.TextPlain))
            {
                return MediaType.TextPlain;
            }

            if (!type.Equals(MediaType.OctetStream))
            {
                return MediaType.OctetStream;
            }

            return null;
        }
    }
}
