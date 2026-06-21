using System.Text.Json;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel.Types;

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
    private string? _quotaName;
    private List<string>? _tags;

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
        await session.ReloadSessionAsync(quotaName, tags, cancellationToken).ConfigureAwait(false);
        return session;
    }

    private async Task InitAsync(string? quotaName, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        _quotaName = quotaName;
        _tags = NormalizeTags(tags);
        var request = BuildSessionRequest("POST", quotaName, tags);
        request.WithQuery("downloads", string.Empty);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        ParseResponse(response.BodyText ?? string.Empty);
    }

    private async Task ReloadSessionAsync(string? quotaName, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        _quotaName = quotaName;
        _tags = NormalizeTags(tags);
        var request = BuildSessionRequest("GET", quotaName, tags);
        request.WithQuery("downloadid", Id);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        ParseResponse(response.BodyText ?? string.Empty);
    }

    private static List<string>? NormalizeTags(IEnumerable<string>? tags)
    {
        var list = tags?.Where(t => !string.IsNullOrEmpty(t)).ToList();
        return list is { Count: > 0 } ? list : null;
    }

    /// <summary>
    /// 打开记录读取器，拉取 <paramref name="start"/> 起的 <paramref name="count"/> 行。
    /// <para>对应 PyODPS <c>InstanceDownloadSession._build_input_stream</c> + <c>open_record_reader</c>。</para>
    /// <para>返回的 <see cref="TunnelRecordReader"/> 持有底层流，调用方负责 dispose。</para>
    /// </summary>
    /// <param name="start">起始行下标（0-based）</param>
    /// <param name="count">拉取行数</param>
    /// <param name="columns">只拉取指定列名（null = 全部）</param>
    public async Task<TunnelRecordReader> OpenRecordReaderAsync(
        long start,
        long count,
        IEnumerable<string>? columns = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Id))
            throw new InvalidOperationException("Session has no DownloadID; create or reload first.");
        if (Schema is null || Schema.Columns.Count == 0)
            throw new InvalidOperationException("Session schema is empty; cannot build decoders.");

        var request = new OdpsRequest
        {
            Method = "GET",
            Path = $"/projects/{Uri.EscapeDataString(_project)}/instances/{Uri.EscapeDataString(_instanceId)}"
        };
        request.WithQuery("data", string.Empty);
        request.WithQuery("downloadid", Id);
        request.WithQuery("rowrange", $"({start},{count})");
        request.WithHeader("x-odps-tunnel-version", TunnelVersion.ToString());
        request.WithHeader("Content-Length", "0");

        if (!string.IsNullOrEmpty(_quotaName))
            request.WithQuery("quotaName", _quotaName);
        if (_tags is { Count: > 0 })
            request.WithHeader("odps-tunnel-tags", string.Join(",", _tags));

        if (columns != null)
        {
            var cols = columns.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            if (cols.Count > 0)
                request.WithQuery("columns", string.Join(",", cols));
        }

        var streamResponse = await _client.SendStreamingAsync(request, cancellationToken).ConfigureAwait(false);

        var decoders = new ITypeDecoder[Schema.Columns.Count];
        for (var i = 0; i < Schema.Columns.Count; i++)
            decoders[i] = TypeDecoderFactory.GetDecoder(Schema.Columns[i].Type);

        // TunnelRecordReader 接管 streamResponse.Stream 的读取；streamResponse 本身由调用方 dispose
        return new TunnelRecordReader(streamResponse.Stream, decoders, streamResponse);
    }

    /// <summary>
    /// 以 Arrow 格式打开原始下载流（GET ?data&amp;downloadid&amp;rowrange&amp;arrow）。
    /// <para>返回的 <see cref="OdpsStreamResponse"/> 是 MaxCompute Arrow 分帧流（[4B BE chunk_size][data][4B crc32c]...），
    /// 由 <c>Azrng.NMaxCompute.Arrow</c> 包负责分帧解码与 Apache.Arrow IPC 解析。调用方负责 dispose。</para>
    /// </summary>
    public async Task<OdpsStreamResponse> OpenArrowStreamAsync(
        long start, long count, IEnumerable<string>? columns = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Id))
            throw new InvalidOperationException("Session has no DownloadID; create or reload first.");

        var request = new OdpsRequest
        {
            Method = "GET",
            Path = $"/projects/{Uri.EscapeDataString(_project)}/instances/{Uri.EscapeDataString(_instanceId)}"
        };
        request.WithQuery("data", string.Empty);
        request.WithQuery("downloadid", Id);
        request.WithQuery("rowrange", $"({start},{count})");
        request.WithQuery("arrow", string.Empty);
        request.WithHeader("x-odps-tunnel-version", TunnelVersion.ToString());
        request.WithHeader("Content-Length", "0");

        if (!string.IsNullOrEmpty(_quotaName))
            request.WithQuery("quotaName", _quotaName);
        if (_tags is { Count: > 0 })
            request.WithHeader("odps-tunnel-tags", string.Join(",", _tags));

        if (columns != null)
        {
            var cols = columns.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            if (cols.Count > 0)
                request.WithQuery("columns", string.Join(",", cols));
        }

        return await _client.SendStreamingAsync(request, cancellationToken).ConfigureAwait(false);
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
