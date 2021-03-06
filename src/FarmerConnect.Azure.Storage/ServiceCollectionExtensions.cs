using System;
using FarmerConnect.Azure.Storage.Blob;
using FarmerConnect.Azure.Storage.Table;
using Microsoft.Extensions.DependencyInjection;

namespace FarmerConnect.Azure.Storage
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlobStorage(this IServiceCollection services, Action<BlobStorageOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddScoped<BlobStorageService>();

            return services;
        }

        public static IServiceCollection AddTableStorage(this IServiceCollection services, Action<TableStorageOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddScoped<TableStorageService>();

            return services;
        }
    }
}
