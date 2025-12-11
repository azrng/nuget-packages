using Microsoft.Extensions.DependencyInjection;

namespace Common.QRCode
{
    public static class ServiceCollectionExtensions
    {
        public static void AddQrCode(this IServiceCollection services)
        {
            services.AddTransient<IQrCodeHelp, QrCodeHelp>();
        }
    }
}