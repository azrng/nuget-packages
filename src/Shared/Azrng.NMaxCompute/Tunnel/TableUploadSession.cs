using System.Text.Json;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// 表级 Tunnel 上传会话：创建 session → 分块写入 → 完成提交。
/// <para>对应 PyODPS <c>odps/tunnel/tabletunnel.py::TableUploadSession</c>。</para>
/// <para>创建：<c>POST /projects/&lt;proj&gt;/tables/&lt;table&gt;?uploads</c>；
/// 写块：<c>PUT ...?uploadid&amp;blockid</c>；完成：<c>POST ...?uploadid</c>。
/// 均带 <c>x-odps-tunnel-version: 6</c>。</para>
/// </summary>
public sealed class TableUploadSession
{
    /// <summary>表级 Tunnel 协议版本（与 InstanceDownloadSession 一致）。</summary>
    public const int TunnelVersion = 6;

    private readonly OdpsRestClient _client;
    private readonly string _project;
    private readonly string _table;
    private readonly string? _partitionSpec;
    private List<string>? _tags;

    internal TableUploadSession(OdpsRestClient client, string project, string table, string? partitionSpec)
    {
        _client = client;
        _project = project;
        _table = table;
        _partitionSpec = string.IsNullOrWhiteSpace(partitionSpec) ? null : partitionSpec;
    }

    /// <summary>UploadID（由服务端分配）。</summary>
    public string Id { get; private set; } = string.Empty;

    public TunnelSessionStatus Status { get; private set; } = TunnelSessionStatus.Unknown;

    /// <summary>表 schema（来自创建响应，决定写入列）。</summary>
    public TableSchema Schema { get; private set; } = new();

    /// <summary>创建上传 session。</summary>
    public static async Task<TableUploadSession> CreateAsync(
        OdpsRestClient client, string project, string table,
        string? partitionSpec = null, IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var session = new TableUploadSession(client, project, table, partitionSpec);
        await session.InitAsync(tags, cancellationToken).ConfigureAwait(false);
        return session;
    }

    private async Task InitAsync(IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        _tags = tags?.Where(t => !string.IsNullOrEmpty(t)).ToList();
        var request = NewBaseRequest();
        request.WithQuery("uploads", string.Empty);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        ParseResponse(response.BodyText ?? string.Empty);
    }

    /// <summary>
    /// 写入一个数据块（blockId 由调用方分配，通常从 0 递增）。
    /// </summary>
    public async Task WriteBlockAsync(int blockId, byte[] block, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Id))
            throw new InvalidOperationException("Session has no UploadID; create first.");
        if (block is null || block.Length == 0)
            throw new ArgumentException("Block data is empty.", nameof(block));

        var request = NewBaseRequest("PUT");
        request.WithQuery("uploadid", Id);
        request.WithQuery("blockid", blockId.ToString());
        request.WithHeader("Content-Type", "application/octet-stream");
        request.Body = block;

        await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 便捷：用 <see cref="TunnelRecordWriter"/> 把记录编码为一个块并上传。返回写入的记录数。
    /// </summary>
    public async Task<int> WriteRecordsAsync(int blockId, IEnumerable<object?[]> rows, CancellationToken cancellationToken = default)
    {
        var writer = new TunnelRecordWriter(Schema);
        foreach (var row in rows)
            writer.Write(row!);
        await WriteBlockAsync(blockId, writer.ToBlockBytes(), cancellationToken).ConfigureAwait(false);
        return writer.Count;
    }

    /// <summary>
    /// 完成（提交）上传。服务端校验已写块数与客户端一致后落盘。
    /// </summary>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Id))
            throw new InvalidOperationException("Session has no UploadID; create first.");

        var request = NewBaseRequest();
        request.WithQuery("uploadid", Id);

        await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private OdpsRequest NewBaseRequest(string method = "POST")
    {
        var request = new OdpsRequest
        {
            Method = method,
            Path = $"/projects/{Uri.EscapeDataString(_project)}/tables/{Uri.EscapeDataString(_table)}"
        };
        request.WithHeader("Content-Length", "0");
        request.WithHeader("x-odps-tunnel-version", TunnelVersion.ToString());
        if (_partitionSpec is not null)
            request.WithQuery("partition", _partitionSpec);
        if (_tags is { Count: > 0 })
            request.WithHeader("odps-tunnel-tags", string.Join(",", _tags));
        return request;
    }

    internal void ParseResponse(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new OdpsException("Tunnel upload session create returned empty body", 0);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("UploadID", out var idEl))
                Id = idEl.GetString() ?? string.Empty;
            if (root.TryGetProperty("Status", out var statusEl))
                Status = InstanceDownloadSession.ParseStatus(statusEl.GetString());
            if (root.TryGetProperty("Schema", out var schemaEl) && schemaEl.ValueKind == JsonValueKind.Object)
                Schema = TableSchema.Parse(schemaEl.GetRawText());
        }
        catch (JsonException ex)
        {
            throw new OdpsException($"Failed to parse tunnel upload session response: {ex.Message}. Body: {body}", 0);
        }
    }
}
