using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// 表级 Tunnel 入口：创建上传/下载 session。
/// <para>对应 PyODPS <c>odps/tunnel/tabletunnel.py::TableTunnel</c>。</para>
/// <para>客户端应指向 Tunnel 端点（如 <c>dt.cn-shanghai.maxcompute.aliyun.com</c>）。</para>
/// </summary>
public sealed class TableTunnel
{
    private readonly OdpsRestClient _client;

    public TableTunnel(OdpsRestClient client)
        => _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>创建表（或分区）上传 session。</summary>
    public Task<TableUploadSession> CreateUploadSessionAsync(
        string project, string table, string? partitionSpec = null, string? schema = null,
        IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
        => TableUploadSession.CreateAsync(_client, project, table, partitionSpec, schema, tags, cancellationToken);

    /// <summary>创建表（或分区）下载 session。</summary>
    public Task<TableDownloadSession> CreateDownloadSessionAsync(
        string project, string table, string? partitionSpec = null, string? schema = null,
        string? quotaName = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
        => TableDownloadSession.CreateAsync(_client, project, table, partitionSpec, schema, quotaName, tags, cancellationToken);
}
