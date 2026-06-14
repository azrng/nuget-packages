using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Common.HttpClients
{
    /// <summary>
    /// 默认 HTTP 日志脱敏器。按 <see cref="HttpClientOptions"/> 快照构造，
    /// 由 <see cref="LoggingHandler"/> 在请求处理时根据命名客户端配置创建。
    /// </summary>
    internal sealed class DefaultHttpLogRedactor : IHttpLogRedactor
    {
        private static readonly string[] DefaultSensitiveHeaderNames =
        {
            "Authorization",
            "Proxy-Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Api-Key",
            "Api-Key",
            "X-Auth-Token"
        };

        private static readonly string[] DefaultSensitiveFieldNames =
        {
            "password",
            "passwd",
            "pwd",
            "secret",
            "token",
            "access_token",
            "refresh_token",
            "client_secret",
            "api_key",
            "api-key"
        };

        private static readonly JsonSerializerOptions RelaxedJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Bearer Token 脱敏正则，所有实例共享
        /// </summary>
        private static readonly Regex BearerValuePattern = new(
            "(Bearer\\s+)[A-Za-z0-9\\-._~+/]+=*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// JSON 字段脱敏正则缓存（按敏感字段 pattern 字符串索引，避免重复编译）
        /// </summary>
        private static readonly ConcurrentDictionary<string, Regex> JsonPatternCache = new();

        /// <summary>
        /// key=value 形式脱敏正则缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, Regex> KvPatternCache = new();

        private readonly HashSet<string> _sensitiveHeaderNames;
        private readonly HashSet<string> _sensitiveFieldNames;
        private readonly Regex _jsonSensitiveValuePattern;
        private readonly Regex _kvSensitiveValuePattern;

        /// <summary>
        /// 初始化 <see cref="DefaultHttpLogRedactor"/> 的新实例
        /// </summary>
        /// <param name="httpConfig">HTTP 配置快照</param>
        public DefaultHttpLogRedactor(HttpClientOptions httpConfig)
        {
            if (httpConfig == null)
            {
                throw new ArgumentNullException(nameof(httpConfig));
            }

            _sensitiveHeaderNames = BuildSensitiveHeaders(httpConfig.AdditionalSensitiveHeaders);
            _sensitiveFieldNames = BuildSensitiveFields(httpConfig.AdditionalSensitiveFields);

            var sensitiveFieldPattern = BuildSensitiveFieldPattern(_sensitiveFieldNames);
            _jsonSensitiveValuePattern = JsonPatternCache.GetOrAdd(sensitiveFieldPattern,
                p => new Regex("(\"(?:" + p + ")\"\\s*:\\s*\")([^\"]*)(\")",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled));
            _kvSensitiveValuePattern = KvPatternCache.GetOrAdd(sensitiveFieldPattern,
                p => new Regex("\\b(" + p + ")=([^&\\s]+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        /// <inheritdoc />
        public string RedactContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            if (TryRedactJson(content, out var json))
            {
                return json!;
            }

            var redacted = _jsonSensitiveValuePattern.Replace(content, "$1***$3");
            redacted = _kvSensitiveValuePattern.Replace(redacted, "$1=***");
            redacted = BearerValuePattern.Replace(redacted, "$1***");
            return redacted;
        }

        /// <inheritdoc />
        public IDictionary<string, string> RedactHeaders(IDictionary<string, string>? headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            // HTTP header 名称大小写不敏感：直接构造 case-insensitive 目标字典，
            // 同名（忽略大小写）条目按写入顺序覆盖，敏感头最终统一为 "***"。
            var redacted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in headers)
            {
                var value = _sensitiveHeaderNames.Contains(pair.Key) ? "***" : pair.Value;
                redacted[pair.Key] = value;
            }

            return redacted;
        }

        private bool TryRedactJson(string content, out string? redacted)
        {
            redacted = null;

            try
            {
                using var document = JsonDocument.Parse(content);
                var redactedValue = RedactJsonElement(document.RootElement, null);
                redacted = JsonSerializer.Serialize(redactedValue, RelaxedJsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private object? RedactJsonElement(JsonElement element, string? propertyName)
        {
            if (IsSensitiveField(propertyName))
            {
                return "***";
            }

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    return element.EnumerateObject()
                                  .ToDictionary(property => property.Name,
                                      property => RedactJsonElement(property.Value, property.Name));

                case JsonValueKind.Array:
                    return element.EnumerateArray()
                                  .Select(item => RedactJsonElement(item, propertyName))
                                  .ToList();

                case JsonValueKind.String:
                    var value = element.GetString();
                    return value == null ? null : BearerValuePattern.Replace(value, "$1***");

                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    return CloneJsonElement(element);

                default:
                    return CloneJsonElement(element);
            }
        }

        private static object? CloneJsonElement(JsonElement element)
        {
            return JsonSerializer.Deserialize<object>(element.GetRawText());
        }

        private bool IsSensitiveField(string? fieldName)
        {
            return !string.IsNullOrWhiteSpace(fieldName) && _sensitiveFieldNames.Contains(fieldName);
        }

        private static HashSet<string> BuildSensitiveHeaders(ICollection<string>? additionalHeaders)
        {
            var result = new HashSet<string>(DefaultSensitiveHeaderNames, StringComparer.OrdinalIgnoreCase);
            if (additionalHeaders == null)
            {
                return result;
            }

            foreach (var header in additionalHeaders)
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                result.Add(header.Trim());
            }

            return result;
        }

        private static HashSet<string> BuildSensitiveFields(ICollection<string>? additionalFields)
        {
            var result = new HashSet<string>(DefaultSensitiveFieldNames, StringComparer.OrdinalIgnoreCase);
            if (additionalFields == null)
            {
                return result;
            }

            foreach (var field in additionalFields)
            {
                if (string.IsNullOrWhiteSpace(field))
                {
                    continue;
                }

                result.Add(field.Trim());
            }

            return result;
        }

        private static string BuildSensitiveFieldPattern(IEnumerable<string> fields)
        {
            var escaped = fields.Select(Regex.Escape).ToArray();
            return escaped.Length == 0 ? "a^" : string.Join("|", escaped);
        }
    }
}
