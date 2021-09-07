using System;
using Microsoft.Extensions.DependencyInjection;

namespace FarmerConnect.Azure.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingConsumer(this IServiceCollection services, Action<AzureServiceBusOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<ServiceBusQueueConsumer>();
            services.AddSingleton<EventBusSubscriptionManager>();

            services.AddHostedService<ServiceBusQueueConsumerBackgroundService>();
            return services;
        }

        public static IServiceCollection AddMessagingSender(this IServiceCollection services, Action<AzureServiceBusOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<ServiceBusQueueSender>();
            return services;
        }
    }
}
