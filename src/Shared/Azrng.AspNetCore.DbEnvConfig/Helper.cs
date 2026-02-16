using System.Text.Json;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 辅助工具类
/// </summary>
internal static class Helper
{
    /// <summary>
    /// 深拷贝字典
    /// </summary>
    /// <param name="dict">要拷贝的字典</param>
    /// <returns>字典的深拷贝副本</returns>
    public static IDictionary<string, string?> Clone(this IDictionary<string, string?> dict)
    {
        return new Dictionary<string, string?>(dict, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检测两个字典是否发生变化
    /// </summary>
    /// <param name="oldDict">旧字典</param>
    /// <param name="newDict">新字典</param>
    /// <returns>如果字典内容发生变化返回 true，否则返回 false</returns>
    public static bool IsChanged(IDictionary<string, string?> oldDict,
                                 IDictionary<string, string?> newDict)
    {
        if (oldDict.Count != newDict.Count)
        {
            return true;
        }

        foreach (var (oldKey, oldValue) in oldDict)
        {
            if (!newDict.TryGetValue(oldKey, out var newValue))
            {
                return true;
            }

            if (oldValue != newValue)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 从 JsonElement 获取适用于配置的字符串值
    /// </summary>
    /// <param name="e">JSON 元素</param>
    /// <returns>配置值，如果元素为 null 或 undefined 则返回 null</returns>
    public static string? GetValueForConfig(this JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.String)
        {
            return e.GetString();
        }

        if (e.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return e.GetRawText();
    }
}