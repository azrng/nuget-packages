using Newtonsoft.Json;

namespace Azrng.NMaxCompute.Test.Model;

public class QueryManySqlRequestHo : QuerySqlBase
{
    public QueryManySqlRequestHo() { }

    public QueryManySqlRequestHo(QuerySqlBase sqlBase, string sql)
    {
        Url = sqlBase.Url;
        AccessId = sqlBase.AccessId;
        JdbcUrl = sqlBase.JdbcUrl;
        SecretKey = sqlBase.SecretKey;
        MaxRows = sqlBase.MaxRows;
        SqlList = sql.Split(";").ToList();
    }

    [JsonProperty("sql_list")]
    public List<string> SqlList { get; set; }
}
