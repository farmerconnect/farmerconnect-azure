using Microsoft.Extensions.DependencyInjection;

namespace FarmerConnect.Azure.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            return services;
        }
    }
}
