using System.Collections.Generic;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace CommonCollect.InfluxDb.Options
{
    public class InfluxDbClientOptions
    {
        public string Url
        {
            get;
            set;
        }

        public string User
        {
            get;
            set;
        }

        public string Pwd
        {
            get;
            set;
        }

        public string DbName
        {
            get;
            set;
        }

        public List<RetentionPolicy> RetentionPolicies
        {
            get;
            set;
        }
    }
}
