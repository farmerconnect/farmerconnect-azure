using System;
using FarmerConnect.Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace FarmerConnect.Azure.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingConsumer(this IServiceCollection services, Action<ServiceBusOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<ServiceBusQueueConsumer>();
            services.AddSingleton<EventBusSubscriptionManager>();
            services.AddHostedService<ServiceBusQueueConsumerBackgroundService>();

            return services;
        }

        public static IServiceCollection AddMessagingSender(this IServiceCollection services, Action<ServiceBusOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<IServiceBusQueueSender, ServiceBusQueueSender>();

            return services;
        }
    }
}
