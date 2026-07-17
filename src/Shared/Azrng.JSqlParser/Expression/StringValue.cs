using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 表示 SQL 字符串字面量。支持单引号、PostgreSQL dollar-quoted、带前缀（N'/E'/U'/R'/B'/RB'/_utf8'）
/// 以及 Oracle q'...{...}...' 自定义分隔引号形式。
/// </summary>
public sealed class StringValue : ASTNodeAccessImpl, Expression
{
    /// <summary>
    /// 允许的字符串前缀列表（与上游对齐）。N=国家字符集、E=EString、U=Unicode、R=Raw、B/RB=Bit，
    /// _utf8=MySQL utf8 字符集。
    /// </summary>
    public static readonly string[] AllowedPrefixes = { "N", "U", "E", "R", "B", "RB", "_utf8" };

    public string Value { get; set; } = "";

    /// <summary>
    /// 字符串前缀（如 N、E、U），未指定时为 null。
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// 引号字符串。默认 "'"，PostgreSQL dollar-quoted 时为 "$$" 或 "$tag$"，
    /// Oracle q'...' 时为 "q'X" + "X'"（X 为分隔符）。
    /// </summary>
    public string QuoteStr { get; set; } = "'";

    /// <summary>
    /// PostgreSQL dollar-quoted 前缀（兼容旧 API，等价于 QuoteStr 为 $$/$tag$ 时的快捷标记）。
    /// 对应上游 commit 95ebda5a。
    /// </summary>
    public string? DollarPrefix
    {
        get => QuoteStr.Length > 1 && QuoteStr.StartsWith("$") ? QuoteStr : null;
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                QuoteStr = value;
            }
        }
    }

    public StringValue() { }

    /// <summary>
    /// 从 lexer 返回的原始字面量文本构造 StringValue，
    /// 自动识别前缀（N/U/E/R/B/RB/_utf8）、Oracle q'...' 形式与 PostgreSQL $$...$$ dollar-quoted。
    /// </summary>
    public StringValue(string rawValue)
    {
        // Oracle q'...{...}...' 形式：移除 q' 前缀和对应的闭包引号
        if (rawValue.Length >= 3 &&
            (rawValue[0] == 'q' || rawValue[0] == 'Q') &&
            rawValue[1] == '\'')
        {
            var lastQuote = rawValue.LastIndexOf('\'');
            if (lastQuote > 2)
            {
                // 内容位于 rawValue[2 .. lastQuote]，可能含成对分隔符
                // 例如 q'[abc]' -> 内容为 abc（去除外层 [ ]）
                // 例如 q'XabcX' -> 内容为 abc（去除外层 X）
                var inner = rawValue.Substring(2, lastQuote - 2);
                var openDelim = inner.Length > 0 ? inner[0] : '\0';
                var closeDelim = inner.Length > 0 ? inner[^1] : '\0';
                var pairMap = new System.Collections.Generic.Dictionary<char, char>
                {
                    ['['] = ']',
                    ['('] = ')',
                    ['{'] = '}',
                    ['<'] = '>'
                };
                if (pairMap.ContainsKey(openDelim) && closeDelim == pairMap[openDelim])
                {
                    Value = inner[1..^1];
                }
                else
                {
                    // 单字符分隔符（X...X 形式）
                    Value = openDelim == closeDelim ? inner[1..^1] : inner;
                }
                QuoteStr = rawValue[..2]; // q' 保留用于 ToString 输出
                Prefix = null;
                return;
            }
        }

        // PostgreSQL dollar-quoted: $$...$$ 或 $tag$...$tag$
        if (rawValue.Length >= 4 && rawValue.StartsWith("$$") && rawValue.EndsWith("$$"))
        {
            Value = rawValue[2..^2];
            QuoteStr = "$$";
            return;
        }
        if (rawValue.Length >= 4 && rawValue.StartsWith("$") &&
            rawValue.IndexOf('$', 1) is int tagEnd && tagEnd > 1 &&
            rawValue.EndsWith(rawValue[..(tagEnd + 1)]))
        {
            var tag = rawValue[..(tagEnd + 1)];
            Value = rawValue[(tagEnd + 1)..^(tag.Length)];
            QuoteStr = tag;
            return;
        }

        // 单引号字面量 '...'
        if (rawValue.Length >= 2 && rawValue.StartsWith("'") && rawValue.EndsWith("'"))
        {
            Value = rawValue[1..^1].Replace("''", "'");
            return;
        }

        // 带前缀的字面量：N'...', U'...', E'...' 等
        if (rawValue.Length > 2)
        {
            foreach (var p in AllowedPrefixes)
            {
                if (rawValue.Length > p.Length &&
                    rawValue.StartsWith(p, System.StringComparison.OrdinalIgnoreCase) &&
                    rawValue[p.Length] == '\'')
                {
                    Prefix = p;
                    Value = rawValue[(p.Length + 1)..^1].Replace("''", "'");
                    return;
                }
            }
        }

        // 兜底：直接当字符串内容
        Value = rawValue;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        // PostgreSQL dollar-quoted 或 Oracle q'...' 形式由 QuoteStr 表达
        if (QuoteStr.Length > 1)
        {
            // dollar-quoted: $$ + value + $$（QuoteStr 已含两侧 $$）
            // Oracle q': QuoteStr 形如 "q'"，需要还原分隔符 — 这里简化为按括号形式输出
            if (QuoteStr == "$$" || QuoteStr.StartsWith("$"))
            {
                return $"{QuoteStr}{Value}{QuoteStr}";
            }
            // Oracle q' 形式按 [] 重输出
            return $"q'[{Value}]'";
        }
        return $"{Prefix ?? ""}{QuoteStr}{Value}{QuoteStr}";
    }
}
