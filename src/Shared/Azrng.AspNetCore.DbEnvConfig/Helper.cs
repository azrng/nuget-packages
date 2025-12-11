using System.Text.Json;

namespace Azrng.AspNetCore.DbEnvConfig;

internal static class Helper
{
    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static IDictionary<string, string?> Clone(this IDictionary<string, string> dict)
    {
        var newDict = new Dictionary<string, string?>();
        foreach (var kv in dict)
        {
            newDict[kv.Key] = kv.Value;
        }

        return newDict;
    }

    /// <summary>
    /// 检测是否发生变化
    /// </summary>
    /// <param name="oldDict"></param>
    /// <param name="newDict"></param>
    /// <returns></returns>
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
    /// 获取值
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static string? GetValueForConfig(this JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.String)
        {
            //remove the quotes, "ab"-->ab
            return e.GetString();
        }

        if (e.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            //remove the quotes, "null"-->null
            return null;
        }
        else
        {
            return e.GetRawText();
        }
    }
}