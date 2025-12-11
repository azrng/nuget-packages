using Microsoft.Extensions.DependencyInjection;

namespace Common.RestSharpClient
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加HttpClients
        /// </summary>
        /// <param name="services"></param>
        public static void AddHttpClientService(this IServiceCollection services)
        {
            services.AddTransient<IHttpClientHelper, HttpClientHelper>();
        }
    }
}