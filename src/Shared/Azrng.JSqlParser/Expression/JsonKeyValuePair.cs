using System.Text;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// JSON_OBJECT 的键值对：[KEY] key (VALUE | : | ,) value [FORMAT JSON [ENCODING x]]。
/// 与上游 JsonKeyValuePair 对齐。key/value 用 object 承载（上游同样用 Object），
/// 通常 key 为 StringValue/Column，value 为 Expression。
/// </summary>
public class JsonKeyValuePair
{
    public enum SeparatorKind
    {
        /// <summary>" VALUE " 分隔（带前后空格）。</summary>
        VALUE,
        /// <summary>":" 分隔（jjt 由 :: 触发但输出单冒号）。</summary>
        COLON,
        /// <summary>"," 分隔（MySQL 方言）。</summary>
        COMMA,
        /// <summary>无分隔（仅有 key 无 value）。</summary>
        NOT_USED
    }

    public object? Key { get; set; }

    public object? Value { get; set; }

    public bool UsingKeyKeyword { get; set; }

    public SeparatorKind Separator { get; set; } = SeparatorKind.NOT_USED;

    public bool UsingFormatJson { get; set; }

    public string? Encoding { get; set; }

    public JsonKeyValuePair() { }

    public JsonKeyValuePair(object? key, object? value, bool usingKeyKeyword, SeparatorKind separator)
    {
        Key = key;
        Value = value;
        UsingKeyKeyword = usingKeyKeyword;
        Separator = separator;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (UsingKeyKeyword && Separator == SeparatorKind.VALUE)
        {
            sb.Append("KEY ");
        }
        sb.Append(Key);

        if (Value != null)
        {
            sb.Append(GetSeparatorString());
            sb.Append(Value);
        }

        if (UsingFormatJson)
        {
            sb.Append(" FORMAT JSON");
            if (Encoding != null)
            {
                sb.Append(" ENCODING ").Append(Encoding);
            }
        }

        return sb.ToString();
    }

    private string GetSeparatorString() => Separator switch
    {
        SeparatorKind.VALUE => " VALUE ",
        SeparatorKind.COLON => ":",
        SeparatorKind.COMMA => ",",
        _ => ""
    };
}
