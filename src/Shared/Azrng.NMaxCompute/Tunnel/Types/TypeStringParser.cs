using System.Text;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// 解析 MaxCompute 类型字符串（含复合类型）为 <see cref="ITypeDecoder"/>。
/// <para>对应 PyODPS <c>validate_data_type</c> + <c>parse_composite_types</c>。</para>
/// <para>
/// 支持语法：<c>bigint</c>、<c>array&lt;T&gt;</c>、<c>map&lt;K,V&gt;</c>、
/// <c>struct&lt;name1:T1,name2:T2&gt;</c>，可任意嵌套。
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

        var tokens = Tokenize(typeString);
        if (tokens.Count == 0)
            throw new ArgumentException($"Cannot parse type string: {typeString}");

        // 简单类型：单个 token 且无子结构
        if (tokens.Count == 1 && tokens[0].IsTerminal)
            return TypeDecoderFactory.GetPrimitiveDecoder(tokens[0].Value);

        // 复合类型：用栈折叠
        var stack = new List<object>();
        foreach (var token in tokens)
        {
            if (token.IsOpen)
            {
                // 形如 "array<" / "map<" / "struct("：把类型名压栈
                stack.Add(token.Value);
            }
            else if (token.IsClose)
            {
                // 取出栈顶到上一个 open 之间的所有元素，构造复合 decoder
                stack = Reduce(stack, token.Value);
            }
            else
            {
                stack.Add(token);
            }
        }

        if (stack.Count != 1)
            throw new ArgumentException($"Unbalanced type string: {typeString}");

        return TokenToDecoder(stack[0]);
    }

    private static List<object> Reduce(List<object> stack, string closeBracket)
    {
        // 栈底向上找到第一个 string（类型名），它之前的都是参数
        var nameIdx = -1;
        for (var i = stack.Count - 1; i >= 0; i--)
        {
            if (stack[i] is string)
            {
                nameIdx = i;
                break;
            }
        }
        if (nameIdx < 0)
            throw new ArgumentException("Malformed composite type: missing type name");

        var name = (string)stack[nameIdx];
        var args = new List<object>();
        for (var i = nameIdx + 1; i < stack.Count; i++)
            args.Add(stack[i]);

        var reduced = BuildComposite(name, args, closeBracket);

        var result = new List<object>();
        for (var i = 0; i < nameIdx; i++)
            result.Add(stack[i]);
        result.Add(reduced);
        return result;
    }

    private static object BuildComposite(string name, List<object> args, string closeBracket)
    {
        var lower = name.ToLowerInvariant();
        return lower switch
        {
            "array" => new ArrayDecoder(TokenToDecoder(args[0])),
            "map" => new MapDecoder(TokenToDecoder(args[0]), TokenToDecoder(args[1])),
            "struct" => BuildStruct(args),
            _ => throw new NotSupportedException($"Composite type '{name}' is not supported.")
        };
    }

    private static object BuildStruct(List<object> args)
    {
        var names = new List<string>();
        var decoders = new List<ITypeDecoder>();
        foreach (var arg in args)
        {
            if (arg is Token t)
            {
                // "name:type"
                var kv = SplitStructKv(t.Value);
                names.Add(kv.name);
                decoders.Add(Parse(kv.type));
            }
            else
            {
                throw new ArgumentException($"Struct field must be 'name:type', got {arg}");
            }
        }
        return new StructDecoder(names.ToArray(), decoders.ToArray());
    }

    private static (string name, string type) SplitStructKv(string s)
    {
        // 支持反引号包裹的字段名
        var sb = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            if (ch == '`')
            {
                quoted = !quoted;
                continue;
            }
            if (ch == ':' && !quoted)
            {
                var name = sb.ToString().Trim().Trim('`');
                var type = s[(i + 1)..].Trim();
                return (name, type);
            }
            sb.Append(ch);
        }
        throw new ArgumentException($"Invalid struct field definition: {s}");
    }

    private static ITypeDecoder TokenToDecoder(object token)
    {
        return token switch
        {
            Token t => TypeDecoderFactory.GetPrimitiveDecoder(t.Value),
            ITypeDecoder d => d,
            _ => throw new ArgumentException($"Cannot convert token {token} to decoder")
        };
    }

    /// <summary>
    /// 词法分析：把类型字符串切成 token 序列。
    /// IsOpen 形如 "array<"（Value="array"），IsClose 形如 ">" / ")"。
    /// </summary>
    private static List<Token> Tokenize(string s)
    {
        var tokens = new List<Token>();
        var current = new StringBuilder();
        var quoted = false;

        void Flush()
        {
            if (current.Length > 0)
            {
                tokens.Add(new Token(current.ToString().Trim(), isTerminal: true));
                current.Clear();
            }
        }

        for (var i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            if (ch == '`')
            {
                quoted = !quoted;
                current.Append(ch);
                continue;
            }

            if (quoted)
            {
                current.Append(ch);
                continue;
            }

            if (ch == '<' || ch == '(')
            {
                var name = current.ToString().Trim();
                current.Clear();
                if (name.Length > 0)
                    tokens.Add(new Token(name, isOpen: true));
            }
            else if (ch == '>' || ch == ')')
            {
                Flush();
                tokens.Add(new Token(ch.ToString(), isClose: true));
            }
            else if (ch == ',')
            {
                Flush();
            }
            else
            {
                current.Append(ch);
            }
        }
        Flush();
        return tokens;
    }

    private readonly struct Token
    {
        public string Value { get; }
        public bool IsOpen { get; }
        public bool IsClose { get; }
        public bool IsTerminal { get; }

        public Token(string value, bool isOpen = false, bool isClose = false, bool isTerminal = false)
        {
            Value = value;
            IsOpen = isOpen;
            IsClose = isClose;
            IsTerminal = isTerminal;
        }

        public override string ToString() => Value;
    }
}
