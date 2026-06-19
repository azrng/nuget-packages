using System.Text;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Core;

/// <summary>
/// ODPS 顶层入口：提交 SQL、管理 Instance
/// </summary>
public sealed class Odps
{
    private readonly OdpsRestClient _client;
    private readonly string _project;

    public string Project => _project;

    public Odps(OdpsRestClient client, string project)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _project = project ?? throw new ArgumentNullException(nameof(project));
    }

    /// <summary>
    /// 提交 SQL 任务，立即返回 Instance 句柄（不阻塞）
    /// </summary>
    public async Task<Instance> RunSqlAsync(string sql, SqlHints? hints = null, int priority = 9, CancellationToken cancellationToken = default)
    {
        var xml = JobXmlBuilder.Build(sql, hints, priority);

        var request = new OdpsRequest
        {
            Method = "POST",
            Path = $"/projects/{Uri.EscapeDataString(_project)}/instances"
        };
        request.WithHeader("Content-Type", "application/xml");
        request.WithStringBody(xml);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var instanceId = response.TryGetInstanceId();
        if (string.IsNullOrEmpty(instanceId))
            throw new OdpsException("ODPS submission response missing Location header.", response.StatusCode, instanceId: null);

        return new Instance(_client, _project, instanceId);
    }

    /// <summary>
    /// 提交 SQL 并阻塞等待执行完毕，返回 task 结果（Result API 路径，限 10000 行）
    /// </summary>
    public async Task<InstanceResult> ExecuteSqlAsync(
        string sql,
        SqlHints? hints = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var instance = await RunSqlAsync(sql, hints, cancellationToken: cancellationToken).ConfigureAwait(false);
        await instance.WaitForTerminationAsync(timeout, cancellationToken).ConfigureAwait(false);
        return await instance.GetResultAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
