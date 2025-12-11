using System.Net.Http;
using CommonCollect.InfluxDb.Interface;
using CommonCollect.InfluxDb.Options;
using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using Microsoft.Extensions.Options;

namespace CommonCollect.InfluxDb
{
    public class InfluxDbClientFactory : IInfluxDbClientFactory
    {
        private readonly InfluxDbClientOptions _options;

        public InfluxDbClientFactory(IOptions<InfluxDbClientOptions> optionsAccesser)
        {
            _options = optionsAccesser.Value;
        }

        public InfluxDbClientDecorator CreateClient()
        {
            //IL_0025: Unknown result type (might be due to invalid IL or missing references)
            //IL_0035: Expected O, but got Unknown
            return new InfluxDbClientDecorator(new InfluxDbClient(_options.Url, _options.User, _options.Pwd, (InfluxDbVersion)0, (QueryLocation)1, (HttpClient)null, false), _options);
        }
    }
}
