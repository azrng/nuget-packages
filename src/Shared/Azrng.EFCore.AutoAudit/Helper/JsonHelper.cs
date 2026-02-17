using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Azrng.EFCore.AutoAudit.Helper;

internal static class JsonHelper
{
    /// <summary>
    /// 缓存的序列化选项实例，避免每次序列化都创建新对象
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        // 属性名不区分大小写
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        // 使用安全的编码器，防止 XSS 攻击
        // 允许常用 Unicode 字符范围，同时转义 HTML 特殊字符
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All),
        ReferenceHandler = ReferenceHandler.IgnoreCycles // 忽略循环引用
    };

    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, SerializerOptions);
    }

    public static T? ToObject<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}