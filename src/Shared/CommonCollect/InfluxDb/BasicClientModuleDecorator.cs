using System.Collections.Generic;
using System.Threading.Tasks;
using CommonCollect.InfluxDb.Options;
using InfluxData.Net.Common.Infrastructure;
using InfluxData.Net.InfluxDb.ClientModules;
using InfluxData.Net.InfluxDb.Models;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace CommonCollect.InfluxDb
{
    public class BasicClientModuleDecorator : IBasicClientModule
    {
        private readonly IBasicClientModule _basicClientModule;

        private readonly InfluxDbClientOptions _clientOptions;

        public BasicClientModuleDecorator(IBasicClientModule basicClientModule, InfluxDbClientOptions clientOptions)
        {
            _basicClientModule = basicClientModule;
            _clientOptions = clientOptions;
        }

        public async Task<IEnumerable<IEnumerable<Serie>>> MultiQueryAsync(IEnumerable<string> queries, string dbName = null, string epochFormat = null, long? chunkSize = null)
        {
            if (dbName == null)
            {
                dbName = _clientOptions.DbName;
            }
            return await _basicClientModule.MultiQueryAsync(queries, dbName, epochFormat, chunkSize);
        }

        public async Task<IEnumerable<Serie>> QueryAsync(string query, string dbName = null, string epochFormat = null, long? chunkSize = null)
        {
            if (dbName == null)
            {
                dbName = _clientOptions.DbName;
            }
            return await _basicClientModule.QueryAsync(query, dbName, epochFormat, chunkSize);
        }

        public async Task<IEnumerable<Serie>> QueryAsync(IEnumerable<string> queries, string dbName = null, string epochFormat = null, long? chunkSize = null)
        {
            if (dbName == null)
            {
                dbName = _clientOptions.DbName;
            }
            return await _basicClientModule.QueryAsync(queries, dbName, epochFormat, chunkSize);
        }

        public async Task<IEnumerable<Serie>> QueryAsync(string queryTemplate, object parameters, string dbName = null, string epochFormat = null, long? chunkSize = null)
        {
            if (dbName == null)
            {
                dbName = _clientOptions.DbName;
            }
            return await _basicClientModule.QueryAsync(queryTemplate, parameters, dbName, epochFormat, chunkSize);
        }

        public async Task<IInfluxDataApiResponse> WriteAsync(Point point, string dbName = null, string retentionPolicy = null, string precision = "ms")
        {
            if (dbName == null)
            {
                dbName = _clientOptions.DbName;
            }
            return await _basicClientModule.WriteAsync(point, dbName, retentionPolicy, precision);
        }

        public async Task<IInfluxDataApiResponse> WriteAsync(IEnumerable<Point> points, string dbName = null, string retentionPolicy = null, string precision = "ms")
        {
            if (dbName == null)
            {
                dbName = _clientOptions.DbName;
            }
            return await _basicClientModule.WriteAsync(points, dbName, retentionPolicy, precision);
        }
    }
}
