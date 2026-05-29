using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Azrng.NTika.Core.Model
{
    public sealed class MediaType : IEquatable<MediaType>, IComparable<MediaType>
    {
        private static readonly ConcurrentDictionary<string, MediaType> SimpleTypes = new();

        private static readonly Regex TypePattern = new(
            @"(?s)^\s*([^\s\(\)<>@,;:\\""/\[\]\?=]+)\s*/\s*([^\s\(\)<>@,;:\\""/\[\]\?=]+)\s*($|;.*)",
            RegexOptions.Compiled);

        private static readonly Regex CharsetFirstPattern = new(
            @"(?is)^\s*(charset\s*=\s*[^\s;]+)\s*;\s*([^\s\(\)<>@,;:\\""/\[\]\?=]+)\s*/\s*([^\s\(\)<>@,;:\\""/\[\]\?=]+)\s*$",
            RegexOptions.Compiled);

        public static readonly MediaType OctetStream = Parse("application/octet-stream")!;
        public static readonly MediaType Empty = Parse("application/x-empty")!;
        public static readonly MediaType TextPlain = Parse("text/plain")!;
        public static readonly MediaType TextHtml = Parse("text/html")!;
        public static readonly MediaType ApplicationXml = Parse("application/xml")!;
        public static readonly MediaType ApplicationZip = Parse("application/zip")!;

        private readonly string _string;
        private readonly int _slash;
        private readonly int _semicolon;
        private readonly IReadOnlyDictionary<string, string> _parameters;

        public MediaType(string type, string subtype, IDictionary<string, string>? parameters)
        {
            type = type.Trim().ToLowerInvariant();
            subtype = subtype.Trim().ToLowerInvariant();

            _slash = type.Length;
            _semicolon = _slash + 1 + subtype.Length;

            if (parameters == null || parameters.Count == 0)
            {
                _parameters = new Dictionary<string, string>();
                _string = type + '/' + subtype;
            }
            else
            {
                var sorted = new SortedDictionary<string, string>();
                foreach (var entry in parameters)
                {
                    sorted[entry.Key.Trim().ToLowerInvariant()] = entry.Value;
                }

                _parameters = sorted;

                var builder = new System.Text.StringBuilder();
                builder.Append(type);
                builder.Append('/');
                builder.Append(subtype);

                foreach (var entry in sorted)
                {
                    builder.Append("; ");
                    builder.Append(entry.Key);
                    builder.Append('=');
                    var value = entry.Value;
                    if (Regex.IsMatch(value, @"[\(\)<>@,;:\\""/\[\]\?=\s]"))
                    {
                        builder.Append('"');
                        builder.Append(Regex.Replace(value, @"[\(\)<>@,;:\\""/\[\]\?=]", @"\$0"));
                        builder.Append('"');
                    }
                    else
                    {
                        builder.Append(value);
                    }
                }

                _string = builder.ToString();
            }
        }

        public MediaType(string type, string subtype)
            : this(type, subtype, null)
        {
        }

        private MediaType(string value, int slash)
        {
            _string = value;
            _slash = slash;
            _semicolon = value.Length;
            _parameters = new Dictionary<string, string>();
        }

        public MediaType(MediaType type, IDictionary<string, string> parameters)
            : this(type.Type, type.Subtype, Union(type._parameters, parameters))
        {
        }

        public MediaType(MediaType type, string name, string value)
            : this(type, new Dictionary<string, string> { { name, value } })
        {
        }

        public string Type => _string[.._slash];
        public string Subtype => _string[(_slash + 1).._semicolon];
        public IReadOnlyDictionary<string, string> Parameters => _parameters;
        public bool HasParameters => _parameters.Count > 0;

        public MediaType BaseType
        {
            get
            {
                if (!HasParameters)
                {
                    return this;
                }
                return Parse(_string[.._semicolon])!;
            }
        }

        public static MediaType Application(string type) => Parse("application/" + type)!;
        public static MediaType Audio(string type) => Parse("audio/" + type)!;
        public static MediaType Image(string type) => Parse("image/" + type)!;
        public static MediaType Text(string type) => Parse("text/" + type)!;
        public static MediaType Video(string type) => Parse("video/" + type)!;

        public static ISet<MediaType> Set(params MediaType[] types)
        {
            var set = new HashSet<MediaType>();
            foreach (var type in types)
            {
                if (type != null)
                {
                    set.Add(type);
                }
            }
            return set;
        }

        public static ISet<MediaType> Set(params string[] types)
        {
            var set = new HashSet<MediaType>();
            foreach (var type in types)
            {
                var mt = Parse(type);
                if (mt != null)
                {
                    set.Add(mt);
                }
            }
            return set;
        }

        public static MediaType? Parse(string? value)
        {
            if (value == null)
            {
                return null;
            }

            // Optimization for common cases
            if (SimpleTypes.TryGetValue(value, out var cached))
            {
                return cached;
            }

            var slash = value.IndexOf('/');
            if (slash == -1)
            {
                return null;
            }

            if (SimpleTypes.Count < 10000 &&
                IsSimpleName(value[..slash]) &&
                IsSimpleName(value[(slash + 1)..]))
            {
                // Check if there are parameters - if so, don't cache as simple
                var semicolon = value.IndexOf(';', slash);
                if (semicolon == -1)
                {
                    var simple = new MediaType(value, slash);
                    return SimpleTypes.GetOrAdd(value, simple);
                }
            }

            // Try standard pattern
            var match = TypePattern.Match(value);
            if (match.Success)
            {
                return new MediaType(match.Groups[1].Value, match.Groups[2].Value,
                    ParseParameters(match.Groups[3].Value));
            }

            // Try charset-first pattern
            match = CharsetFirstPattern.Match(value);
            if (match.Success)
            {
                return new MediaType(match.Groups[2].Value, match.Groups[3].Value,
                    ParseParameters(match.Groups[1].Value));
            }

            return null;
        }

        private static bool IsSimpleName(string name)
        {
            foreach (var c in name)
            {
                if (c != '-' && c != '+' && c != '.' && c != '_' &&
                    !(c >= '0' && c <= '9') &&
                    !(c >= 'a' && c <= 'z'))
                {
                    return false;
                }
            }
            return name.Length > 0;
        }

        private static Dictionary<string, string> ParseParameters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new Dictionary<string, string>();
            }

            var parameters = new Dictionary<string, string>();
            while (value.Length > 0)
            {
                var key = value;
                var val = "";

                var semicolon = value.IndexOf(';');
                if (semicolon != -1)
                {
                    key = value[..semicolon];
                    value = value[(semicolon + 1)..];
                }
                else
                {
                    value = "";
                }

                var equals = key.IndexOf('=');
                if (equals != -1)
                {
                    val = key[(equals + 1)..];
                    key = key[..equals];
                }

                key = key.Trim();
                if (key.Length > 0)
                {
                    parameters[key] = Unquote(val.Trim());
                }
            }

            return parameters;
        }

        private static string Unquote(string s)
        {
            while (s.StartsWith("\"") || s.StartsWith("'"))
            {
                s = s[1..];
            }
            while (s.EndsWith("\"") || s.EndsWith("'"))
            {
                s = s[..^1];
            }
            return s;
        }

        private static IDictionary<string, string> Union(
            IReadOnlyDictionary<string, string> a,
            IDictionary<string, string> b)
        {
            if (a.Count == 0) return b;
            if (b.Count == 0) return new Dictionary<string, string>(a);

            var result = new Dictionary<string, string>(a);
            foreach (var entry in b)
            {
                result[entry.Key] = entry.Value;
            }
            return result;
        }

        public override string ToString() => _string;

        public bool Equals(MediaType? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _string == other._string;
        }

        public override bool Equals(object? obj) => Equals(obj as MediaType);

        public override int GetHashCode() => _string.GetHashCode();

        public int CompareTo(MediaType? other)
        {
            if (other is null) return 1;
            return string.Compare(_string, other._string, StringComparison.Ordinal);
        }
    }
}
