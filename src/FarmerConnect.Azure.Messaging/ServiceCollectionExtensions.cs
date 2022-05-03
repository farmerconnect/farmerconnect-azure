using System;
using FarmerConnect.Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace FarmerConnect.Azure.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceBusConsumer(this IServiceCollection services, Action<MessagingOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<ServiceBusQueueConsumer>();
            services.AddSingleton<EventSubscriptionManager>();
            services.AddHostedService<ServiceBusQueueConsumerBackgroundService>();

            return services;
        }

        public static IServiceCollection AddServiceBusSender(this IServiceCollection services, Action<MessagingOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<IQueueSender, ServiceBusQueueSender>();

            return services;
        }
    }
}
