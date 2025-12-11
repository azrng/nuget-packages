using CommonCollect.InfluxDb.Options;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.ClientModules;
using InfluxData.Net.InfluxDb.Models.Responses;
using InfluxData.Net.InfluxDb.RequestClients;
using System;
using System.Linq;

namespace CommonCollect.InfluxDb
{
    public class InfluxDbClientDecorator : IInfluxDbClient
    {
        private readonly IInfluxDbClient _influxDbClient;

        public IBasicClientModule Client
        {
            get;
        }

        public ISerieClientModule Serie => _influxDbClient.Serie;

        public IDatabaseClientModule Database => _influxDbClient.Database;

        public IRetentionClientModule Retention => _influxDbClient.Retention;

        public ICqClientModule ContinuousQuery => _influxDbClient.ContinuousQuery;

        public IDiagnosticsClientModule Diagnostics => _influxDbClient.Diagnostics;

        public IUserClientModule User => _influxDbClient.User;

        public IInfluxDbRequestClient RequestClient => _influxDbClient.RequestClient;

        public InfluxDbClientDecorator(IInfluxDbClient influxDbClient, InfluxDbClientOptions options)
        {
            _influxDbClient = influxDbClient;
            Client = (IBasicClientModule)(object)new BasicClientModuleDecorator(_influxDbClient.Client, options);
            if (!string.IsNullOrEmpty(options.DbName))
            {
                EnsureDatabaseCreated(options.DbName);
            }
            if (options.RetentionPolicies == null)
            {
                return;
            }
            foreach (RetentionPolicy retentionPolicy in options.RetentionPolicies)
            {
                EnsureRetentionPolicyCreated(retentionPolicy, options.DbName);
            }
        }

        private void EnsureDatabaseCreated(string dbName)
        {
            if (!Database.GetDatabasesAsync().Result.Any((r) => r.Name == dbName))
            {
                Database.CreateDatabaseAsync(dbName);
            }
        }

        private void EnsureRetentionPolicyCreated(RetentionPolicy defaultPolicy, string dbName)
        {
            if (!Retention.GetRetentionPoliciesAsync(dbName).Result.Any((r) => r.Name == defaultPolicy.Name && !Retention.CreateRetentionPolicyAsync(dbName, defaultPolicy.Name, defaultPolicy.Duration, defaultPolicy.ReplicationCopies).Result.Success))
            {
                throw new InvalidOperationException("初始化策略失败");
            }
        }
    }
}