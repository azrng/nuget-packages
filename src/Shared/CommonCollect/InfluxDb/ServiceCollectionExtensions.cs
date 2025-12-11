using CommonCollect.InfluxDb.Interface;
using CommonCollect.InfluxDb.Options;
using InfluxData.Net.InfluxDb;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CommonCollect.InfluxDb
{
    public static class ServiceCollectionExtensions
    {
		public static IServiceCollection AddInfluxDbClient(this IServiceCollection services, Action<InfluxDbClientOptions> optionsBuilder)
		{
			if (optionsBuilder == null)
			{
				throw new ArgumentNullException("InfluxDbClientOptions");
			}
			services.Configure(optionsBuilder);
			services.AddSingleton<IInfluxDbClientFactory, InfluxDbClientFactory>();
			services.AddScoped<IInfluxDbClient, InfluxDbClientDecorator>((Func<IServiceProvider, InfluxDbClientDecorator>)((IServiceProvider serviceProvider) => serviceProvider.GetService<IInfluxDbClientFactory>().CreateClient()));
			return services;
		}
	}
}