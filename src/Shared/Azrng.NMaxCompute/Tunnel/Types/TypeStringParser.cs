using System.Text;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// 解析 MaxCompute 类型字符串（含复合类型）为 <see cref="ITypeDecoder"/>。
/// <para>对应 PyODPS <c>validate_data_type</c> + <c>parse_composite_types</c>。</para>
/// <para>
/// 支持语法：<c>bigint</c>、<c>varchar(n)</c>、<c>decimal(p,s)</c>、<c>array&lt;T&gt;</c>、
/// <c>map&lt;K,V&gt;</c>、<c>struct&lt;name1:T1,name2:T2&gt;</c>，可任意嵌套
/// （含 <c>struct&lt;f:array&lt;string&gt;&gt;</c> 这类字段为复合类型的场景）。
/// struct 字段名可用反引号 <c>`name`</c> 包裹。
/// </para>
/// </summary>
public static class TypeStringParser
{
    /// <summary>
    /// 解析类型字符串为 decoder。
    /// </summary>
    public static ITypeDecoder Parse(string typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            throw new ArgumentException("Type string is empty.", nameof(typeString));

        var parser = new Parser(typeString);
        var decoder = parser.ParseType();
        parser.SkipSpaces();
        if (!parser.AtEnd)
            throw new ArgumentException($"Unbalanced or extra characters in type string: {typeString}");
        return decoder;
    }

    private sealed class Parser
    {
        private readonly string _s;
        private int _i;

        public Parser(string s) { _s = s; }

        public bool AtEnd => _i >= _s.Length;

        private char Peek()
        {
            if (_i >= _s.Length)
                throw new ArgumentException($"Unexpected end of type string near position {_i}");
            return _s[_i];
        }

        public void SkipSpaces()
        {
            while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
        }

        private void Expect(char c)
        {
            SkipSpaces();
            if (_i >= _s.Length || _s[_i] != c)
                throw new ArgumentException($"Expected '{c}' in type string near position {_i}");
            _i++;
        }

        public ITypeDecoder ParseType()
        {
            SkipSpaces();
            var name = ReadIdent();
            var lower = name.ToLowerInvariant();
            SkipSpaces();

            // vector 特殊语法：vector<float,1536> 或 vector(float,1536)
            if (lower == "vector")
                return ParseVector();

            // 带长度/精度的基本类型：varchar(10) / char(5) / decimal(10,2) —— 消费 (..) 交 factory
            if (!AtEnd && _s[_i] == '(')
                return TypeDecoderFactory.GetPrimitiveDecoder(name + ReadParens());

            // 非复合：交 factory
            if (AtEnd || _s[_i] != '<')
                return TypeDecoderFactory.GetPrimitiveDecoder(name);

            // 复合：name<
            _i++; // consume '<'
            ITypeDecoder decoder;
            switch (lower)
            {
                case "array":
                    decoder = new ArrayDecoder(ParseType());
                    break;
                case "map":
                    var keyDecoder = ParseType();
                    Expect(',');
                    var valueDecoder = ParseType();
                    decoder = new MapDecoder(keyDecoder, valueDecoder);
                    break;
                case "struct":
                    decoder = ParseStructBody();
                    break;
                default:
                    throw new NotSupportedException($"Composite type '{name}' is not supported.");
            }
            Expect('>');
            return decoder;
        }

        private ITypeDecoder ParseStructBody()
        {
            var names = new List<string>();
            var decoders = new List<ITypeDecoder>();
            while (true)
            {
                SkipSpaces();
                if (!AtEnd && _s[_i] == '>') break; // 空 struct 或结束
                names.Add(ReadStructFieldName());
                Expect(':');
                decoders.Add(ParseType());
                SkipSpaces();
                if (!AtEnd && _s[_i] == ',') { _i++; continue; }
                break;
            }
            return new StructDecoder(names.ToArray(), decoders.ToArray());
        }

        /// <summary>
        /// 解析 vector：接受 <c>vector&lt;elem,dim&gt;</c> 或 <c>vector(elem,dim)</c>。
        /// 维度仅用于校验格式（真实维度随 wire 流传输），元素 decoder 决定解码。
        /// </summary>
        private ITypeDecoder ParseVector()
        {
            SkipSpaces();
            if (AtEnd || (_s[_i] != '<' && _s[_i] != '('))
                throw new ArgumentException($"Expected '<' or '(' after vector near position {_i}");
            var close = _s[_i] == '<' ? '>' : ')';
            _i++;

            var elementDecoder = ParseType();
            Expect(',');
            SkipSpaces();
            var dimToken = ReadIdent();          // 维度数字
            if (!int.TryParse(dimToken, out _))
                throw new ArgumentException($"Invalid vector dimension: {dimToken}");
            Expect(close);

            return new VectorDecoder(elementDecoder);
        }

        private string ReadIdent()
        {
            var start = _i;
            while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_')) _i++;
            if (start == _i)
                throw new ArgumentException($"Expected type name near position {_i}");
            return _s[start.._i];
        }

        /// <summary>
        /// 读取 struct 字段名（支持反引号包裹的任意字符，如 `my field`）。
        /// </summary>
        private string ReadStructFieldName()
        {
            SkipSpaces();
            if (!AtEnd && _s[_i] == '`')
            {
                _i++; // 跳过起始反引号
                var start = _i;
                while (_i < _s.Length && _s[_i] != '`') _i++;
                var name = _s[start.._i];
                if (_i < _s.Length) _i++; // 跳过结束反引号
                return name;
            }
            return ReadIdent();
        }

        /// <summary>
        /// 消费平衡的 (...)（含逗号），返回含括号的完整片段，如 "(10,2)"。
        /// </summary>
        private string ReadParens()
        {
            var sb = new StringBuilder();
            Expect('(');
            sb.Append('(');
            var depth = 1;
            while (depth > 0)
            {
                if (_i >= _s.Length)
                    throw new ArgumentException("Unbalanced parentheses in type string");
                var c = _s[_i++];
                sb.Append(c);
                if (c == '(') depth++;
                else if (c == ')') depth--;
            }
            return sb.ToString();
        }
    }
}
