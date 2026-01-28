using Newtonsoft.Json;

namespace Azrng.NMaxCompute.Adapter.Ho;

public class QuerySqlBase
{
    /// <summary>
    /// 接口地址
    /// </summary>
    public string Url { get; set; }

    [JsonProperty("access_id")]
    public string AccessId { get; set; }

    [JsonProperty("jdbc_url")]
    public string JdbcUrl { get; set; }

    [JsonProperty("secret_key")]
    public string SecretKey { get; set; }

    [JsonProperty("max_rows")]
    public int MaxRows { get; set; } = 1000;
}

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

    [JsonProperty("sql")]
    public string Sql { get; set; }
}
