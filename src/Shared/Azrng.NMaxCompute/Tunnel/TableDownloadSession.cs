using System.Text.Json;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// 表级 Tunnel 下载会话：创建 session → 拉取表（或分区）数据。
/// <para>对应 PyODPS <c>odps/tunnel/tabletunnel.py::TableDownloadSession</c>。</para>
/// <para>创建：<c>POST /projects/&lt;proj&gt;/tables/&lt;table&gt;?downloads</c>（分区用 <c>?partition=</c>）；
/// 读数据：<c>GET ...?data&amp;downloadid&amp;rowrange=(start,count)</c>。均带 <c>x-odps-tunnel-version: 6</c>。</para>
/// <para>记录解码与 <see cref="InstanceDownloadSession"/> 完全一致（同一 wire 格式，复用 <see cref="TunnelRecordReader"/>）。</para>
/// </summary>
public sealed class TableDownloadSession
{
    /// <summary>表级 Tunnel 协议版本（与 InstanceDownloadSession / TableUploadSession 一致）。</summary>
    public const int TunnelVersion = 6;

    private readonly OdpsRestClient _client;
    private readonly string _project;
    private readonly string _table;
    private readonly string? _partitionSpec;
    private readonly string? _schema;
    private List<string>? _tags;
    private string? _quotaName;

    internal TableDownloadSession(OdpsRestClient client, string project, string table, string? partitionSpec, string? schema = null)
    {
        _client = client;
        _project = project;
        _table = table;
        _partitionSpec = string.IsNullOrWhiteSpace(partitionSpec) ? null : partitionSpec;
        _schema = string.IsNullOrWhiteSpace(schema) ? null : schema;
    }

    /// <summary>DownloadID（由服务端分配）。</summary>
    public string Id { get; private set; } = string.Empty;

    public TunnelSessionStatus Status { get; private set; } = TunnelSessionStatus.Unknown;

    /// <summary>总行数。</summary>
    public long RecordCount { get; private set; }

    /// <summary>列定义。</summary>
    public TableSchema Schema { get; private set; } = new();

    public string? QuotaName => _quotaName;

    /// <summary>
    /// datetime / timestamp 列是否按本地时区返回（默认 true，与 <see cref="InstanceDownloadSession"/> 一致）。
    /// </summary>
    public bool UseLocalTimeZone { get; set; } = true;

    /// <summary>创建下载 session。</summary>
    public static async Task<TableDownloadSession> CreateAsync(
        OdpsRestClient client, string project, string table,
        string? partitionSpec = null, string? schema = null,
        string? quotaName = null, IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var session = new TableDownloadSession(client, project, table, partitionSpec, schema);
        await session.InitAsync(quotaName, tags, cancellationToken).ConfigureAwait(false);
        return session;
    }

    private async Task InitAsync(string? quotaName, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        _quotaName = quotaName;
        _tags = tags?.Where(t => !string.IsNullOrEmpty(t)).ToList();
        var request = NewBaseRequest("POST");
        request.WithQuery("downloads", string.Empty);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        ParseResponse(response.BodyText ?? string.Empty);
    }

    /// <summary>
    /// 打开记录读取器，拉取 <paramref name="start"/> 起的 <paramref name="count"/> 行。
    /// <para>返回的 <see cref="TunnelRecordReader"/> 持有底层流，调用方负责 dispose。</para>
    /// </summary>
    public async Task<TunnelRecordReader> OpenRecordReaderAsync(
        long start, long count, IEnumerable<string>? columns = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Id))
            throw new InvalidOperationException("Session has no DownloadID; create first.");
        if (Schema is null || Schema.Columns.Count == 0)
            throw new InvalidOperationException("Session schema is empty; cannot build decoders.");

        var request = NewBaseRequest("GET");
        request.WithQuery("data", string.Empty);
        request.WithQuery("downloadid", Id);
        request.WithQuery("rowrange", $"({start},{count})");

        if (!string.IsNullOrEmpty(_quotaName))
            request.WithQuery("quotaName", _quotaName);

        if (columns != null)
        {
            var cols = columns.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            if (cols.Count > 0)
                request.WithQuery("columns", string.Join(",", cols));
        }

        var streamResponse = await _client.SendStreamingAsync(request, cancellationToken).ConfigureAwait(false);

        var decoders = new ITypeDecoder[Schema.Columns.Count];
        for (var i = 0; i < Schema.Columns.Count; i++)
            decoders[i] = TypeDecoderFactory.GetDecoder(Schema.Columns[i].Type, useUtc: !UseLocalTimeZone);

        return new TunnelRecordReader(streamResponse.Stream, decoders, streamResponse);
    }

    /// <summary>
    /// 以分片方式流式读取全部记录（每片一次 HTTP 请求）。对应 PyODPS <c>BufferedRecordReader</c>。
    /// </summary>
    public IAsyncEnumerable<object?[]> ReadRowsAsync(int sliceSize = 10000, CancellationToken cancellationToken = default)
        => BufferedRecordReader.ReadAllAsync(OpenSliceAsync, RecordCount, sliceSize, cancellationToken);

    private async Task<IEnumerable<object?[]>> OpenSliceAsync(long start, int count, CancellationToken cancellationToken)
    {
        using var reader = await OpenRecordReaderAsync(start, count, columns: null, cancellationToken).ConfigureAwait(false);
        var rows = new List<object?[]>(count);
        while (reader.Read() is { } row)
            rows.Add(row);
        return rows;
    }

    private OdpsRequest NewBaseRequest(string method)
    {
        var request = new OdpsRequest
        {
            Method = method,
            Path = $"/projects/{Uri.EscapeDataString(_project)}/tables/{Uri.EscapeDataString(_table)}"
        };
        request.WithHeader("Content-Length", "0");
        request.WithHeader("x-odps-tunnel-version", TunnelVersion.ToString());
        if (_schema is not null)
            request.WithQuery("curr_schema", _schema);
        if (_partitionSpec is not null)
            request.WithQuery("partition", _partitionSpec);
        if (_tags is { Count: > 0 })
            request.WithHeader("odps-tunnel-tags", string.Join(",", _tags));
        return request;
    }

    internal void ParseResponse(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new OdpsException("Tunnel table download session create returned empty body", 0);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("DownloadID", out var idEl))
                Id = idEl.GetString() ?? string.Empty;
            if (root.TryGetProperty("Status", out var statusEl))
                Status = InstanceDownloadSession.ParseStatus(statusEl.GetString());
            if (root.TryGetProperty("RecordCount", out var countEl) && countEl.TryGetInt64(out var count))
                RecordCount = count;
            if (root.TryGetProperty("QuotaName", out var quotaEl))
                _quotaName = quotaEl.GetString();
            if (root.TryGetProperty("Schema", out var schemaEl) && schemaEl.ValueKind == JsonValueKind.Object)
                Schema = TableSchema.Parse(schemaEl.GetRawText());
        }
        catch (JsonException ex)
        {
            throw new OdpsException($"Failed to parse tunnel table download session response: {ex.Message}. Body: {body}", 0);
        }
    }

    /// <summary>仅用于测试：以预构造的 JSON 响应构造 session，不发起网络请求。</summary>
    internal static TableDownloadSession FromResponseJsonForTest(
        string project, string table, string? partitionSpec, string? schema, string body)
    {
        var session = new TableDownloadSession(null!, project, table, partitionSpec, schema);
        session.ParseResponse(body);
        return session;
    }
}
