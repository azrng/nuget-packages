using Newtonsoft.Json;

namespace CommonCollect.DbConnection.Options
{
    public class MySqlOptions
    {
        [JsonProperty("connectionString")]
        public string ConnectionString
        {
            get;
            set;
        }
    }
}
