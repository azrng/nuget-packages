namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 原样保留参数体的函数，用于 deparsing 时把原始参数文本原样输出。
/// 与上游 RawFunction 对齐。注意：上游解析器不会产出此节点，本类仅用于 API 对齐，
/// 由调用方手动构造（如保留方言专属函数的原始文本）。
/// </summary>
public class RawFunction : Function
{
    public string? RawArguments { get; set; }

    public RawFunction() { }

    public RawFunction(string name, string? rawArguments)
    {
        Name = name;
        RawArguments = rawArguments;
    }

    public override string ToString()
        => RawArguments == null ? $"{Name}()" : $"{Name}({RawArguments})";
}
