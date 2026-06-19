namespace Azrng.NMaxCompute.Rest;

/// <summary>
/// ODPS 服务端错误异常
/// </summary>
public class OdpsException : Exception
{
    public int StatusCode { get; }

    public string? Code { get; }

    public string? RequestId { get; }

    public string? HostId { get; }

    /// <summary>
    /// 关联的 instance id（如果错误是在 instance 相关操作中抛出）
    /// </summary>
    public string? InstanceId { get; set; }

    public OdpsException(string message, int statusCode, string? code = null, string? requestId = null, string? hostId = null, string? instanceId = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        RequestId = requestId;
        HostId = hostId;
        InstanceId = instanceId;
    }

    public override string Message
    {
        get
        {
            var baseMsg = base.Message ?? string.Empty;
            var extras = new List<string>(4);
            if (!string.IsNullOrEmpty(Code)) extras.Add($"Code={Code}");
            if (!string.IsNullOrEmpty(RequestId)) extras.Add($"RequestId={RequestId}");
            if (!string.IsNullOrEmpty(InstanceId)) extras.Add($"InstanceId={InstanceId}");
            if (extras.Count == 0) return baseMsg;
            return $"{baseMsg} ({string.Join(", ", extras)})";
        }
    }
}
