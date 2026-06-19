using System.Xml.Linq;
using System.Globalization;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Core;

/// <summary>
/// ODPS Instance 句柄：状态轮询、取结果
/// </summary>
public sealed class Instance
{
    private readonly OdpsRestClient _client;
    private readonly string _project;
    private readonly TimeSpan _pollingInitialInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _pollingMaxInterval = TimeSpan.FromSeconds(30);

    public string Id { get; }

    public InstanceStatus Status { get; private set; } = InstanceStatus.Unknown;

    internal Instance(OdpsRestClient client, string project, string id)
    {
        _client = client;
        _project = project;
        Id = id;
    }

    /// <summary>
    /// 单次刷新状态
    /// </summary>
    public async Task<InstanceStatus> ReloadStatusAsync(CancellationToken cancellationToken = default)
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = $"/projects/{Uri.EscapeDataString(_project)}/instances/{Uri.EscapeDataString(Id)}"
        };

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var status = ExtractStatus(response.BodyText ?? string.Empty);
        Status = status;
        return status;
    }

    /// <summary>
    /// 阻塞等待 instance 进入 Terminated 状态（指数退避，超时抛异常）
    /// </summary>
    public async Task WaitForTerminationAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var deadline = timeout.HasValue ? DateTimeOffset.UtcNow.Add(timeout.Value) : (DateTimeOffset?)null;
        var interval = _pollingInitialInterval;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await ReloadStatusAsync(cancellationToken).ConfigureAwait(false);
            if (status == InstanceStatus.Terminated)
                return;
            if (status == InstanceStatus.Suspended)
                throw new OdpsException($"Instance {Id} is suspended (likely awaiting approval).", 0, instanceId: Id);

            if (deadline.HasValue && DateTimeOffset.UtcNow >= deadline.Value)
                throw new OdpsException($"Instance {Id} wait timeout after {timeout}.", 0, instanceId: Id);

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            interval = TimeSpan.FromTicks(Math.Min(_pollingMaxInterval.Ticks, interval.Ticks * 2));
        }
    }

    /// <summary>
    /// 走 Result API 取 task 结果（CSV/文本，限 10000 行，S0 MVP 路径）
    /// </summary>
    public async Task<InstanceResult> GetResultAsync(string taskName = JobXmlBuilder.DefaultTaskName, CancellationToken cancellationToken = default)
    {
        var request = new OdpsRequest
        {
            Method = "GET",
            Path = $"/projects/{Uri.EscapeDataString(_project)}/instances/{Uri.EscapeDataString(Id)}"
        };
        request.WithQuery("instance", "result");

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return InstanceResultParser.Parse(response.BodyText ?? string.Empty, taskName, Id);
    }

    private static InstanceStatus ExtractStatus(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return InstanceStatus.Unknown;

        try
        {
            var doc = XDocument.Parse(body);
            var statusEl = doc.Root?.Element("Status");
            if (statusEl == null)
                return InstanceStatus.Unknown;

            return statusEl.Value.Trim().ToLowerInvariant() switch
            {
                "running" => InstanceStatus.Running,
                "suspended" => InstanceStatus.Suspended,
                "terminated" => InstanceStatus.Terminated,
                _ => InstanceStatus.Unknown
            };
        }
        catch
        {
            return InstanceStatus.Unknown;
        }
    }
}
