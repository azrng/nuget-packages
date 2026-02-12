using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Azrng.EventBus.Core.Abstractions;

/// <summary>
/// 时间总线订阅信息类
/// </summary>
public class EventBusSubscriptionInfo
{
    /// <summary>
    /// 事件类型存储  key：类型名 value：类型值
    /// </summary>
    public Dictionary<string, Type> EventTypes { get; } = [];

    /// <summary>
    /// 序列化配置
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; } = new(DefaultSerializerOptions);

    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
                                                                             {
                                                                                 TypeInfoResolver = JsonSerializer
                                                                                     .IsReflectionEnabledByDefault
                                                                                     ? CreateDefaultTypeResolver()
                                                                                     : JsonTypeInfoResolver.Combine()
                                                                             };

    [RequiresUnreferencedCode("Calls System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver.DefaultJsonTypeInfoResolver()")]
    [RequiresDynamicCode("Calls System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver.DefaultJsonTypeInfoResolver()")]
    private static IJsonTypeInfoResolver CreateDefaultTypeResolver() => new DefaultJsonTypeInfoResolver();
}