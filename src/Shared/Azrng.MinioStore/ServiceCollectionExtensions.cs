using Microsoft.Extensions.DependencyInjection;
using Minio;
using System.Text.RegularExpressions;

namespace Azrng.MinioStore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加s3存储器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"> 格式：http(s)://ACCESS_KEY:SECRET_KEY@SERVER_ADDRESS:PORT </param>
        /// <remarks>http://admin:123456789@localhost:9000</remarks>
        public static void AddMinioStore(this IServiceCollection services, string connectionString)
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
            var s3Config = new S3StoreConfig();
            s3Config.AccessKey = match.Groups[2].Value;
            s3Config.SecretKey = match.Groups[3].Value;
            s3Config.Url = httpStr + match.Groups[4].Value;

            s3Config.ParamVerify();

            services.AddMinioStore(config =>
            {
                config.AccessKey = s3Config.AccessKey;
                config.SecretKey = s3Config.SecretKey;
                config.Url = s3Config.Url;
            });
        }

        /// <summary>
        /// 添加s3存储器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static void AddMinioStore(this IServiceCollection services, Action<S3StoreConfig> config)
        {
            ArgumentNullException.ThrowIfNull(services);

            var s3Config = new S3StoreConfig();
            config.Invoke(s3Config);

            var useHttps = s3Config.Url.Contains("https");

            services.AddMinio(configureClient =>
            {
                configureClient.WithEndpoint(s3Config.Url)
                               .WithCredentials(s3Config.AccessKey, s3Config.SecretKey)
                               .WithSSL(useHttps)
                               .Build();
            });

            services.AddScoped<IStore, S3Store>();
            services.AddSingleton<GlobalConfig>(p => new GlobalConfig { UseHttps = useHttps });
        }

        // /// <summary>
        // /// 添加Minio存储器
        // /// </summary>
        // /// <param name="services"></param>
        // /// <param name="configuration">配置Minio的Section</param>
        // public static void AddMinioStore(this IServiceCollection services, IConfiguration configuration)
        // {
        //     ArgumentNullException.ThrowIfNull(configuration);
        //
        //     services.AddOptions<MinioConfig>()
        //         .Bind(configuration.GetSection("Minio"))
        //         .ValidateOnStart();
        //     services.AddScoped<IStore, MinioStore>();
        // }
    }

//     /// <summary>
//     /// httpClient 接受所有ssl
//     /// </summary>
//     public class HttpClientFactoryAcceptAllSsl : HttpClientFactory
//     {
//         public override HttpClient CreateHttpClient(IClientConfig clientConfig)
//         {
// #pragma warning disable CA2000 // 丢失范围之前释放对象
//             return new HttpClient(new HttpClientHandler()
//             {
//                 ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
//             });
// #pragma warning restore CA2000 // 丢失范围之前释放对象
//         }
//     }
}