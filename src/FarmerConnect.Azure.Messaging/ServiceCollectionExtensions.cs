using System;
using FarmerConnect.Azure.Messaging.ServiceBus;
using FarmerConnect.Azure.Messaging.StorageQueue;
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
            services.AddHostedService<ServiceBusHostedService>();

            return services;
        }

        public static IServiceCollection AddServiceBusSender(this IServiceCollection services, Action<MessagingOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<IQueueSender, ServiceBusQueueSender>();

            return services;
        }

        public static IServiceCollection AddStorageQueueConsumer(this IServiceCollection services, Action<MessagingOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<EventSubscriptionManager>();
            services.AddHostedService<StorageQueueConsumer>();

            return services;
        }

        public static IServiceCollection AddStorageQueueSender(this IServiceCollection services, Action<MessagingOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton<IQueueSender, StorageQueueSender>();

            return services;
        }
    }
}
