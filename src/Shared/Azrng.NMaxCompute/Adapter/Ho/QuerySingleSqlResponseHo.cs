using Newtonsoft.Json;

namespace Azrng.NMaxCompute.Adapter.Ho;

public class QuerySingleSqlResponseHo
{
    public string[] Columns { get; set; }
    public object[][] Rows { get; set; }
    [JsonProperty("row_count")] public int RowCount { get; set; }
    [JsonProperty("execution_time")] public string ExecutionTime { get; set; }
}
