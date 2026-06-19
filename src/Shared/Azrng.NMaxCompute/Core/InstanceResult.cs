using System.Xml.Linq;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Core;

/// <summary>
/// ODPS Result API 返回的 task 结果（解析自 <c>GET .../instances/&lt;id&gt;?instance=result</c>）
/// </summary>
public sealed class InstanceResult
{
    /// <summary>
    /// task 名称（通常 AnonymousSQLTask）
    /// </summary>
    public string TaskName { get; init; } = string.Empty;

    /// <summary>
    /// 原始内容（CSV 文本或 Base64 解码后的 CSV 文本）
    /// </summary>
    public string RawContent { get; init; } = string.Empty;

    /// <summary>
    /// 关联的 instance id
    /// </summary>
    public string? InstanceId { get; init; }

    /// <summary>
    /// 是否是空结果（DDL/INSERT 类语句）
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(RawContent);
}

internal static class InstanceResultParser
{
    public static InstanceResult Parse(string bodyXml, string taskName, string instanceId)
    {
        if (string.IsNullOrWhiteSpace(bodyXml))
            return new InstanceResult { TaskName = taskName, InstanceId = instanceId };

        try
        {
            var doc = XDocument.Parse(bodyXml);
            var root = doc.Root;
            if (root == null)
                return new InstanceResult { TaskName = taskName, InstanceId = instanceId };

            // 查找 <Tasks><Task Name="..."><Result Format="..." Transform="...">text</Result></Task></Tasks>
            var taskEl = FindTaskElement(root, taskName);
            if (taskEl == null)
                return new InstanceResult { TaskName = taskName, InstanceId = instanceId };

            var resultEl = taskEl.Element("Result");
            if (resultEl == null)
                return new InstanceResult { TaskName = taskName, InstanceId = instanceId };

            var transform = resultEl.Attribute("Transform")?.Value;
            var rawText = resultEl.Value ?? string.Empty;

            var content = transform is { Length: > 0 } && transform.Equals("Base64", StringComparison.OrdinalIgnoreCase)
                ? DecodeBase64(rawText)
                : rawText;

            return new InstanceResult
            {
                TaskName = taskName,
                RawContent = content,
                InstanceId = instanceId
            };
        }
        catch
        {
            return new InstanceResult { TaskName = taskName, InstanceId = instanceId };
        }
    }

    private static XElement? FindTaskElement(XElement root, string taskName)
    {
        var tasksEl = root.Element("Tasks") ?? root;
        foreach (var taskEl in tasksEl.Elements("Task"))
        {
            var nameAttr = taskEl.Attribute("Name")?.Value;
            if (string.Equals(nameAttr, taskName, StringComparison.Ordinal))
                return taskEl;
        }
        return tasksEl.Element("Task");
    }

    private static string DecodeBase64(string input)
    {
        try
        {
            var bytes = Convert.FromBase64String(input.Trim());
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return input;
        }
    }
}
