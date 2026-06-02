using Azrng.NMaxCompute.Models;
using Newtonsoft.Json;

namespace Azrng.NMaxCompute.Test.Model;

/// <summary>
/// 查询基础配置
/// </summary>
public class QuerySqlBase
{
    /// <summary>
    /// 接口地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Access ID
    /// </summary>
    [JsonProperty("access_id")]
    public string AccessId { get; set; } = string.Empty;

    /// <summary>
    /// JDBC URL
    /// </summary>
    [JsonProperty("jdbc_url")]
    public string JdbcUrl { get; set; } = string.Empty;

    /// <summary>
    /// Secret Key
    /// </summary>
    [JsonProperty("secret_key")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 最大返回行数
    /// </summary>
    [JsonProperty("max_rows")]
    public int MaxRows { get; set; } = 1000;
}

/// <summary>
/// 单个 SQL 查询请求
/// </summary>
public class QuerySingleSqlRequestHo : QuerySqlBase
{
    public QuerySingleSqlRequestHo() { }

    public QuerySingleSqlRequestHo(QuerySqlBase sqlBase)
    {
        Url = sqlBase.Url;
        AccessId = sqlBase.AccessId;
        JdbcUrl = sqlBase.JdbcUrl;
        SecretKey = sqlBase.SecretKey;
        MaxRows = sqlBase.MaxRows;
        Sql = "SELECT 1";
    }

    /// <summary>
    /// 从 MaxComputeConfig 创建请求
    /// </summary>
    public QuerySingleSqlRequestHo(MaxComputeConfig config)
    {
        Url = config.ServerUrl;
        AccessId = config.AccessId;
        JdbcUrl = config.JdbcUrl;
        SecretKey = config.SecretKey;
        MaxRows = config.MaxRows;
    }

    [JsonProperty("sql")]
    public string Sql { get; set; } = string.Empty;
}
