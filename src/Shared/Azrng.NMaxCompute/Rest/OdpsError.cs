using System.Xml.Linq;
using System.Text.Json;

namespace Azrng.NMaxCompute.Rest;

/// <summary>
/// ODPS 服务端错误
/// </summary>
public sealed class OdpsError
{
    public string? Code { get; init; }

    public string? Message { get; init; }

    public string? RequestId { get; init; }

    public string? HostId { get; init; }

    public static OdpsError? TryParse(int statusCode, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        // 优先 XML：<Error><Code>X</Code><Message>Y</Message><RequestId>Z</RequestId></Error>
        var xmlError = TryParseXml(body);
        if (xmlError != null)
            return xmlError;

        // 回退 JSON：{"Code":"X","Message":"Y","RequestId":"Z"}
        var jsonError = TryParseJson(body);
        if (jsonError != null)
            return jsonError;

        // 最末回退：用 ODPS-xxxxx 前缀提取 Code，整段作为 Message
        return TryParseFromText(body);
    }

    private static OdpsError? TryParseXml(string body)
    {
        try
        {
            var doc = XDocument.Parse(body);
            var root = doc.Root;
            if (root == null || !root.Name.LocalName.Equals("Error", StringComparison.OrdinalIgnoreCase))
                return null;

            return new OdpsError
            {
                Code = ReadElement(root, "Code"),
                Message = ReadElement(root, "Message"),
                RequestId = ReadElement(root, "RequestId"),
                HostId = ReadElement(root, "HostId")
            };
        }
        catch
        {
            return null;
        }
    }

    private static OdpsError? TryParseJson(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("Code", out _) && !doc.RootElement.TryGetProperty("code", out _))
                return null;

            string Get(string snake, string camel)
            {
                if (doc.RootElement.TryGetProperty(snake, out var s) && s.ValueKind == JsonValueKind.String)
                    return s.GetString() ?? string.Empty;
                if (doc.RootElement.TryGetProperty(camel, out var c) && c.ValueKind == JsonValueKind.String)
                    return c.GetString() ?? string.Empty;
                return string.Empty;
            }

            return new OdpsError
            {
                Code = Get("Code", "code"),
                Message = Get("Message", "message"),
                RequestId = Get("RequestId", "requestId"),
                HostId = Get("HostId", "hostId")
            };
        }
        catch
        {
            return null;
        }
    }

    private static OdpsError? TryParseFromText(string body)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            body,
            @"ODPS-\w{2,5}\d{3,5}",
            System.Text.RegularExpressions.RegexOptions.None,
            TimeSpan.FromMilliseconds(100));

        if (!match.Success)
            return null;

        return new OdpsError
        {
            Code = match.Value,
            Message = body.Trim()
        };
    }

    private static string? ReadElement(XElement root, string name)
    {
        var el = root.Element(name);
        return el?.Value;
    }
}
