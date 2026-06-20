using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// 类型字符串 → <see cref="ITypeEncoder"/>。与 <see cref="TypeDecoderFactory"/> 对称（写侧）。
/// 复合类型解析与 <see cref="TypeStringParser"/> 同构（递归下降）。
/// </summary>
public static class TypeEncoderFactory
{
    private static readonly Dictionary<string, ITypeEncoder> Primitives = new(StringComparer.OrdinalIgnoreCase)
    {
        { "tinyint", IntegerEncoder.Instance },
        { "smallint", IntegerEncoder.Instance },
        { "int", IntegerEncoder.Instance },
        { "int_", IntegerEncoder.Instance },
        { "integer", IntegerEncoder.Instance },
        { "bigint", IntegerEncoder.Instance },
        { "long", IntegerEncoder.Instance },
        { "float", FloatEncoder.Instance },
        { "float_", FloatEncoder.Instance },
        { "double", DoubleEncoder.Instance },
        { "boolean", BooleanEncoder.Instance },
        { "bool", BooleanEncoder.Instance },
        { "string", StringEncoder.Instance },
        { "binary", StringEncoder.Instance },
        { "varchar", StringEncoder.Instance },
        { "char", StringEncoder.Instance },
        { "json", StringEncoder.Instance },
        { "datetime", DateTimeEncoder.Instance },
        { "date", DateEncoder.Instance },
        { "timestamp", TimestampEncoder.Instance },
        { "timestamp_ntz", TimestampEncoder.Instance },
        { "decimal", DecimalEncoder.Instance },
        { "interval_day_time", IntervalDayTimeEncoder.Instance },
        { "interval_year_month", IntervalYearMonthEncoder.Instance },
    };

    public static ITypeEncoder GetEncoder(string typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            throw new ArgumentException("Type string is empty.", nameof(typeString));

        var p = new Parser(typeString.Trim());
        var enc = p.ParseType();
        p.SkipSpaces();
        if (!p.AtEnd)
            throw new ArgumentException($"Unbalanced or extra characters in type string: {typeString}");
        return enc;
    }

    private static ITypeEncoder GetPrimitive(string name)
    {
        var key = name.Trim().ToLowerInvariant();
        var parenIdx = key.IndexOf('(');
        if (parenIdx > 0)
            key = key[..parenIdx];
        if (Primitives.TryGetValue(key, out var enc))
            return enc;
        throw new NotSupportedException($"MaxCompute type '{name}' is not supported for writing yet.");
    }

    private sealed class Parser
    {
        private readonly string _s;
        private int _i;
        public Parser(string s) { _s = s; }
        public bool AtEnd => _i >= _s.Length;
        public void SkipSpaces() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }

        private char Peek()
        {
            if (_i >= _s.Length) throw new ArgumentException($"Unexpected end of type string near {_i}");
            return _s[_i];
        }
        private void Expect(char c) { SkipSpaces(); if (_i >= _s.Length || _s[_i] != c) throw new ArgumentException($"Expected '{c}' near {_i}"); _i++; }

        public ITypeEncoder ParseType()
        {
            SkipSpaces();
            var name = ReadIdent();
            var lower = name.ToLowerInvariant();
            SkipSpaces();
            if (lower == "vector")
                return ParseVector();
            if (!AtEnd && _s[_i] == '(')
                return GetPrimitive(name + ReadParens());
            if (AtEnd || _s[_i] != '<')
                return GetPrimitive(name);

            _i++; // '<'
            ITypeEncoder enc;
            switch (lower)
            {
                case "array": enc = new ArrayEncoder(ParseType()); break;
                case "map": var k = ParseType(); Expect(','); var v = ParseType(); enc = new MapEncoder(k, v); break;
                case "struct": enc = ParseStructBody(); break;
                default: throw new NotSupportedException($"Composite type '{name}' is not supported for writing.");
            }
            Expect('>');
            return enc;
        }

        private ITypeEncoder ParseStructBody()
        {
            var encs = new List<ITypeEncoder>();
            while (true)
            {
                SkipSpaces();
                if (!AtEnd && _s[_i] == '>') break;
                ReadFieldName();
                Expect(':');
                encs.Add(ParseType());
                SkipSpaces();
                if (!AtEnd && _s[_i] == ',') { _i++; continue; }
                break;
            }
            return new StructEncoder(encs.ToArray());
        }

        private ITypeEncoder ParseVector()
        {
            SkipSpaces();
            if (AtEnd || (_s[_i] != '<' && _s[_i] != '('))
                throw new ArgumentException($"Expected '<' or '(' after vector near {_i}");
            var close = _s[_i] == '<' ? '>' : ')';
            _i++;
            var elem = ParseType();
            Expect(',');
            SkipSpaces();
            ReadIdent();   // 维度（仅校验格式）
            Expect(close);
            return new VectorEncoder(elem);
        }

        private string ReadIdent()
        {
            var start = _i;
            while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_')) _i++;
            if (start == _i) throw new ArgumentException($"Expected type name near {_i}");
            return _s[start.._i];
        }
        private void ReadFieldName()
        {
            SkipSpaces();
            if (!AtEnd && _s[_i] == '`') { _i++; while (_i < _s.Length && _s[_i] != '`') _i++; if (_i < _s.Length) _i++; return; }
            ReadIdent();
        }
        private string ReadParens()
        {
            var sb = new System.Text.StringBuilder();
            Expect('('); sb.Append('(');
            var depth = 1;
            while (depth > 0)
            {
                if (_i >= _s.Length) throw new ArgumentException("Unbalanced parens");
                var c = _s[_i++]; sb.Append(c);
                if (c == '(') depth++; else if (c == ')') depth--;
            }
            return sb.ToString();
        }
    }
}
