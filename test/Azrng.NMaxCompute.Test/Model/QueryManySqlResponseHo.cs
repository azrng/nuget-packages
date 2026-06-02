using Newtonsoft.Json;

namespace Azrng.NMaxCompute.Test.Model;

public class QueryManySqlResponseHo
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    [JsonProperty("total_execution_time")]
    public string TotalExecutionTime { get; set; }
    public List<QueryManySqlResultHo> Results { get; set; }
}

public class QueryManySqlResultHo
{
    public int Index { get; set; }
    public string Sql { get; set; }
    public string Status { get; set; }
    public string[] Columns { get; set; }
    public object[][] Rows { get; set; }

    [JsonProperty("row_count")] public int RowCount { get; set; }
    [JsonProperty("execution_time")] public string ExecutionTime { get; set; }
    public object Error { get; set; }
}
