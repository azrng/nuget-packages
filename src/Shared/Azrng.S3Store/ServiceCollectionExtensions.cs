using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Azrng.S3Store
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加s3存储器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"> 格式：http(s)://ACCESS_KEY:SECRET_KEY@SERVER_ADDRESS:PORT </param>
        /// <remarks>http://admin:123456789@localhost:9000</remarks>
        public static IServiceCollection AddS3Store(this IServiceCollection services, string connectionString)
        {
            ArgumentNullException.ThrowIfNull(services);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var match = Regex.Match(connectionString, @"^(https?://)([^:]+):([^@]+)@([^:]+:\d+)$");
            if (!match.Success)
            {
                throw new ArgumentException("无效的连接格式");
            }

            var httpStr = match.Groups[1].Value;
            services.AddS3Store((options) =>
            {
                options.Url = httpStr + match.Groups[4].Value;
                options.AccessKey = match.Groups[2].Value;
                options.SecretKey = match.Groups[3].Value;
            });
            return services;
        }

        /// <summary>
        /// 添加s3存储器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static IServiceCollection AddS3Store(this IServiceCollection services, Action<S3StoreConfig> config)
        {
            ArgumentNullException.ThrowIfNull(services);

            var s3Config = new S3StoreConfig();
            config.Invoke(s3Config);

            s3Config.ParamVerify();

            var useHttps = s3Config.Url!.Contains("https");

            services.AddScoped<IAmazonS3>(_ =>
            {
                //提供awsAccessKeyId和awsSecretAccessKey构造凭证
                var credentials = new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
                var clientConfig = new AmazonS3Config { ServiceURL = s3Config.Url, ForcePathStyle = true, };
                if (useHttps)
                    clientConfig.HttpClientFactory = new HttpClientFactoryAcceptAllSsl();

                return new AmazonS3Client(credentials, clientConfig);
            });

            services.AddScoped<IStore, S3Store>();
            services.AddSingleton<GlobalConfig>(_ => new GlobalConfig { UseHttps = useHttps });
            return services;
        }
    }

    /// <summary>
    /// httpClient 接受所有ssl
    /// </summary>
    public class HttpClientFactoryAcceptAllSsl : HttpClientFactory
    {
        public override HttpClient CreateHttpClient(IClientConfig clientConfig)
        {
#pragma warning disable CA2000 // 丢失范围之前释放对象
            return new HttpClient(new HttpClientHandler() { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });
#pragma warning restore CA2000 // 丢失范围之前释放对象
        }
    }
}