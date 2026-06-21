using System.Data.Common;
using System.Text;
using Azrng.NMaxCompute.Models;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 连接字符串构建器（直连格式）
/// <para>
/// 连接字符串示例：
/// <code>
/// Endpoint=http://service.cn-hangzhou.maxcompute.aliyun.com/api;
/// AccessId=...;SecretAccessKey=...;Project=my_proj;
/// Region=cn-hangzhou;Schema=...;MaxRows=10000
/// </code>
/// </para>
/// </summary>
public class MaxComputeConnectionStringBuilder : DbConnectionStringBuilder
{
    private const string EndpointKey = "Endpoint";
    private const string AccessIdKey = "AccessId";
    private const string SecretAccessKeyKey = "SecretAccessKey";
    private const string ProjectKey = "Project";
    private const string SchemaKey = "Schema";
    private const string RegionKey = "Region";
    private const string SecurityTokenKey = "SecurityToken";
    private const string TunnelEndpointKey = "TunnelEndpoint";
    private const string MaxRowsKey = "MaxRows";
    private const string UseV4SignatureKey = "UseV4Signature";
    private const string UseLocalTimeZoneKey = "UseLocalTimeZone";
    private const string HintsKey = "Hints";

    /// <summary>
    /// Hints 值内部的键值分隔符（<c>key=value</c>），多个 hint 之间用 <see cref="HintsItemSeparator"/> 分隔。
    /// </summary>
    private const char HintsKvSeparator = '=';
    private const char HintsItemSeparator = ',';

    public MaxComputeConnectionStringBuilder() { }

    public MaxComputeConnectionStringBuilder(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        ConnectionString = connectionString;
    }

    public string Endpoint
    {
        get => GetStringValue(EndpointKey);
        set => this[EndpointKey] = value;
    }

    public string AccessId
    {
        get => GetStringValue(AccessIdKey);
        set => this[AccessIdKey] = value;
    }

    public string SecretAccessKey
    {
        get => GetStringValue(SecretAccessKeyKey);
        set => this[SecretAccessKeyKey] = value;
    }

    public string Project
    {
        get => GetStringValue(ProjectKey);
        set => this[ProjectKey] = value;
    }

    public string? Schema
    {
        get => GetStringValue(SchemaKey);
        set => this[SchemaKey] = value;
    }

    public string? Region
    {
        get => GetStringValue(RegionKey);
        set => this[RegionKey] = value;
    }

    public string? SecurityToken
    {
        get => GetStringValue(SecurityTokenKey);
        set => this[SecurityTokenKey] = value;
    }

    public string? TunnelEndpoint
    {
        get => GetStringValue(TunnelEndpointKey);
        set => this[TunnelEndpointKey] = value;
    }

    public int MaxRows
    {
        get => TryGetValue(MaxRowsKey, out var value) ? Convert.ToInt32(value) : 10000;
        set => this[MaxRowsKey] = value;
    }

    public bool UseV4Signature
    {
        get => TryGetValue(UseV4SignatureKey, out var value) ? Convert.ToBoolean(value) : true;
        set => this[UseV4SignatureKey] = value;
    }

    /// <summary>
    /// datetime / timestamp 是否按本地时区返回（默认 true，对齐 PyODPS <c>local_timezone</c>）；false 返回 UTC。
    /// </summary>
    public bool UseLocalTimeZone
    {
        get => TryGetValue(UseLocalTimeZoneKey, out var value) ? Convert.ToBoolean(value) : true;
        set => this[UseLocalTimeZoneKey] = value;
    }

    /// <summary>
    /// SQL hints，原始字符串形式（逗号分隔的 <c>key=value</c>）。
    /// 例：<c>odps.sql.mapper.split.size=256,odps.sql.mapper.cpu=100</c>
    /// </summary>
    public string? Hints
    {
        get => TryGetValue(HintsKey, out var value) ? value?.ToString() : null;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Remove(HintsKey);
            }
            else
            {
                this[HintsKey] = value;
            }
        }
    }

    /// <summary>
    /// 解析 <see cref="Hints"/> 为键值字典；无 hints 时返回 null。
    /// </summary>
    public IDictionary<string, string>? GetHintsDictionary()
    {
        var raw = Hints;
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in raw.Split(HintsItemSeparator))
        {
            var trimmed = item.Trim();
            if (trimmed.Length == 0)
                continue;
            var sepIdx = trimmed.IndexOf(HintsKvSeparator);
            if (sepIdx <= 0)
                continue;
            var k = trimmed[..sepIdx].Trim();
            var v = trimmed[(sepIdx + 1)..].Trim();
            dict[k] = v;
        }
        return dict.Count > 0 ? dict : null;
    }

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Endpoint)
        && !string.IsNullOrWhiteSpace(AccessId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey)
        && !string.IsNullOrWhiteSpace(Project);

    /// <summary>
    /// 转换为 <see cref="MaxComputeConfig"/>
    /// </summary>
    public MaxComputeConfig ToConfig()
    {
        return new MaxComputeConfig
        {
            Endpoint = Endpoint,
            AccessId = AccessId,
            SecretAccessKey = SecretAccessKey,
            Project = Project,
            Schema = string.IsNullOrEmpty(Schema) ? null : Schema,
            Region = string.IsNullOrEmpty(Region) ? null : Region,
            SecurityToken = string.IsNullOrEmpty(SecurityToken) ? null : SecurityToken,
            TunnelEndpoint = string.IsNullOrEmpty(TunnelEndpoint) ? null : TunnelEndpoint,
            MaxRows = MaxRows,
            UseV4Signature = UseV4Signature,
            UseLocalTimeZone = UseLocalTimeZone,
            Hints = GetHintsDictionary()
        };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        AppendIfNotEmpty(sb, EndpointKey, Endpoint);
        AppendIfNotEmpty(sb, AccessIdKey, AccessId);
        AppendIfNotEmpty(sb, SecretAccessKeyKey, SecretAccessKey);
        AppendIfNotEmpty(sb, ProjectKey, Project);
        AppendIfNotEmpty(sb, SchemaKey, Schema);
        AppendIfNotEmpty(sb, RegionKey, Region);
        AppendIfNotEmpty(sb, SecurityTokenKey, SecurityToken);
        AppendIfNotEmpty(sb, TunnelEndpointKey, TunnelEndpoint);
        if (MaxRows > 0) sb.Append($"{MaxRowsKey}={MaxRows};");
        if (!UseV4Signature) sb.Append($"{UseV4SignatureKey}=false;");
        if (!UseLocalTimeZone) sb.Append($"{UseLocalTimeZoneKey}=false;");
        AppendIfNotEmpty(sb, HintsKey, Hints);
        return sb.ToString().TrimEnd(';');
    }

    private void AppendIfNotEmpty(StringBuilder sb, string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            sb.Append($"{key}={value};");
    }

    private string GetStringValue(string key) =>
        TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
}
