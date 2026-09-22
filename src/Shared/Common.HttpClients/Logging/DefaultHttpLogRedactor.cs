using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// 脱敏 JSON 写出选项：非 ASCII 不转义，保持日志可读（与库内 JsonHelper 行为一致）
        /// </summary>
        private static readonly JsonWriterOptions RedactingWriterOptions = new()
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

        /// <summary>
        /// 敏感字段名出现形态（"name": 或 name=）正则缓存，用于 RedactContent 快速预检
        /// </summary>
        private static readonly ConcurrentDictionary<string, Regex> FieldNamePatternCache = new();

        private readonly HashSet<string> _sensitiveHeaderNames;
        private readonly HashSet<string> _sensitiveFieldNames;
        private readonly Regex _sensitiveFieldNamePattern;
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
            _sensitiveFieldNamePattern = FieldNamePatternCache.GetOrAdd(sensitiveFieldPattern,
                p => new Regex("(?:\"(?:" + p + ")\"\\s*:\\s*|\\b(?:" + p + ")=)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled));
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

            // 快速预检：敏感字段名与 Bearer Token 均未出现时直接原样返回，跳过 JSON 解析与重排；
            // 字段名形态（"name": 或 name=）不能要求值为字符串，否则会漏掉 "token":12345 这类数字值
            if (!_sensitiveFieldNamePattern.IsMatch(content)
                && !_kvSensitiveValuePattern.IsMatch(content)
                && !BearerValuePattern.IsMatch(content))
            {
                return content;
            }

            if (LooksLikeJson(content) && TryRedactJson(content, out var json))
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

        /// <summary>
        /// 首字符快查：仅 JSON 可能的起始字符才尝试解析，XML、普通文本直接走正则路径，
        /// 避免非 JSON 内容每次都经历"解析抛异常再回退"
        /// </summary>
        private static bool LooksLikeJson(string content)
        {
            foreach (var ch in content)
            {
                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                return ch is '{' or '[' or '"' or '-' or 't' or 'f' or 'n' || char.IsDigit(ch);
            }

            return false;
        }

        private bool TryRedactJson(string content, out string? redacted)
        {
            redacted = null;

            try
            {
                using var document = JsonDocument.Parse(content);
                var buffer = new ArrayBufferWriter<byte>();
                using (var writer = new Utf8JsonWriter(buffer, RedactingWriterOptions))
                {
                    WriteRedactedJson(writer, document.RootElement, null);
                    writer.Flush();
                }

                redacted = Encoding.UTF8.GetString(buffer.WrittenSpan);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// 单次遍历写出脱敏 JSON：敏感字段的值统一写为 ***，其余元素经 <see cref="JsonElement.WriteTo"/> 原样透传，
        /// 避免"反序列化装箱再序列化"的中间分配
        /// </summary>
        private void WriteRedactedJson(Utf8JsonWriter writer, JsonElement element, string? propertyName)
        {
            if (IsSensitiveField(propertyName))
            {
                writer.WriteStringValue("***");
                return;
            }

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject())
                    {
                        writer.WritePropertyName(property.Name);
                        WriteRedactedJson(writer, property.Value, property.Name);
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        WriteRedactedJson(writer, item, propertyName);
                    }
                    writer.WriteEndArray();
                    break;

                case JsonValueKind.String:
                    var value = element.GetString();
                    if (value == null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        writer.WriteStringValue(BearerValuePattern.Replace(value, "$1***"));
                    }
                    break;

                default:
                    // 数字 / true / false / null 原样写出（WriteTo 保留原始词法形式）
                    element.WriteTo(writer);
                    break;
            }
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
