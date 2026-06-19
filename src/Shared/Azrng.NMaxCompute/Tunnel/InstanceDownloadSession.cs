using System.Text.Json;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// Tunnel Session 状态
/// <para>对应 PyODPS <c>odps/tunnel/instancetunnel.py::InstanceDownloadSession.Status</c>。</para>
/// </summary>
public enum TunnelSessionStatus
{
    Unknown,
    Normal,
    Closed,
    Expired,
    Failed,
    Initiating
}

/// <summary>
/// Instance Tunnel 下载会话
/// <para>
/// 对应 PyODPS <c>odps/tunnel/instancetunnel.py::InstanceDownloadSession</c>。
/// 创建路径：<c>POST /projects/&lt;proj&gt;/instances/&lt;id&gt;?downloads</c>
/// with header <c>x-odps-tunnel-version: 6</c>。
/// </para>
/// </summary>
public sealed class InstanceDownloadSession
{
    /// <summary>
    /// Tunnel 协议版本号，对应 PyODPS <c>odps/tunnel/base.py::TUNNEL_VERSION</c>。
    /// </summary>
    public const int TunnelVersion = 6;

    private readonly OdpsRestClient _client;
    private readonly string _project;
    private readonly string _instanceId;

    /// <summary>
    /// 构造函数仅赋值，不发起请求；真实初始化由 <see cref="CreateAsync"/> 完成。
    /// </summary>
    internal InstanceDownloadSession(OdpsRestClient client, string project, string instanceId)
    {
        _client = client;
        _project = project;
        _instanceId = instanceId;
    }

    /// <summary>
    /// DownloadID（由服务端分配）
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    public TunnelSessionStatus Status { get; private set; } = TunnelSessionStatus.Unknown;

    /// <summary>
    /// 总行数
    /// </summary>
    public long RecordCount { get; private set; }

    /// <summary>
    /// 列定义
    /// </summary>
    public TableSchema Schema { get; private set; } = new();

    public string? QuotaName { get; private set; }

    /// <summary>
    /// 创建 session：向 ODPS 发起 POST <c>?downloads</c>。
    /// </summary>
    public static async Task<InstanceDownloadSession> CreateAsync(
        OdpsRestClient client,
        string project,
        string instanceId,
        string? quotaName = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var session = new InstanceDownloadSession(client, project, instanceId);
        await session.InitAsync(quotaName, tags, cancellationToken).ConfigureAwait(false);
        return session;
    }

    /// <summary>
    /// 用已有的 download_id 重新加载 session。
    /// </summary>
    public static async Task<InstanceDownloadSession> ReloadAsync(
        OdpsRestClient client,
        string project,
        string instanceId,
        string downloadId,
        string? quotaName = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var session = new InstanceDownloadSession(client, project, instanceId)
        {
            Id = downloadId
        };
        await session.ReloadAsync(quotaName, tags, cancellationToken).ConfigureAwait(false);
        return session;
    }

    private async Task InitAsync(string? quotaName, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        var request = BuildSessionRequest("POST", quotaName, tags);
        request.WithQuery("downloads", string.Empty);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        ParseResponse(response.BodyText ?? string.Empty);
    }

    private async Task ReloadAsync(string? quotaName, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        var request = BuildSessionRequest("GET", quotaName, tags);
        request.WithQuery("downloadid", Id);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        ParseResponse(response.BodyText ?? string.Empty);
    }

    private OdpsRequest BuildSessionRequest(string method, string? quotaName, IEnumerable<string>? tags)
    {
        var request = new OdpsRequest
        {
            Method = method,
            Path = $"/projects/{Uri.EscapeDataString(_project)}/instances/{Uri.EscapeDataString(_instanceId)}"
        };
        request.WithHeader("Content-Length", "0");
        request.WithHeader("x-odps-tunnel-version", TunnelVersion.ToString());

        if (!string.IsNullOrEmpty(quotaName))
            request.WithQuery("quotaName", quotaName);

        var tagList = tags?.Where(t => !string.IsNullOrEmpty(t)).ToList();
        if (tagList is { Count: > 0 })
            request.WithHeader("odps-tunnel-tags", string.Join(",", tagList));

        return request;
    }

    internal void ParseResponse(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new OdpsException("Tunnel session create returned empty body", 0, instanceId: _instanceId);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("DownloadID", out var idEl))
                Id = idEl.GetString() ?? string.Empty;
            if (root.TryGetProperty("Status", out var statusEl))
                Status = ParseStatus(statusEl.GetString());
            if (root.TryGetProperty("RecordCount", out var countEl) && countEl.TryGetInt64(out var count))
                RecordCount = count;
            if (root.TryGetProperty("QuotaName", out var quotaEl))
                QuotaName = quotaEl.GetString();
            if (root.TryGetProperty("Schema", out var schemaEl) && schemaEl.ValueKind == JsonValueKind.Object)
                Schema = TableSchema.Parse(schemaEl.GetRawText());
        }
        catch (JsonException ex)
        {
            throw new OdpsException($"Failed to parse tunnel session response: {ex.Message}. Body: {body}", 0, instanceId: _instanceId);
        }
    }

    /// <summary>
    /// 仅用于测试：以预构造的 JSON 响应构造 session，不发起网络请求。
    /// </summary>
    internal static InstanceDownloadSession FromResponseJsonForTest(string project, string instanceId, string body)
    {
        var session = new InstanceDownloadSession(null!, project, instanceId);
        session.ParseResponse(body);
        return session;
    }

    /// <summary>
    /// 解析 session 状态字符串。公开以便测试。
    /// </summary>
    public static TunnelSessionStatus ParseStatus(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "NORMAL" => TunnelSessionStatus.Normal,
            "CLOSES" or "CLOSED" => TunnelSessionStatus.Closed,
            "EXPIRED" => TunnelSessionStatus.Expired,
            "FAILED" => TunnelSessionStatus.Failed,
            "INITIATING" => TunnelSessionStatus.Initiating,
            _ => TunnelSessionStatus.Unknown
        };
    }
}
