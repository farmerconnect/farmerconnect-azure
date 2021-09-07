<img align="left" width="170" height="100" src=".github/fc-logo.png" />

<br />

This repository holds all the packages that have been standardized inside of FarmerConnect SA on how to use different Azure resources and patterns.

# FarmerConnect.Azure.Storage

[![NuGet Package Build](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-storage.yml/badge.svg?branch=main)](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-storage.yml)

```powershell
dotnet add package FarmerConnect.Azure.Storage
```

## Blob storage

We are using the same Storage Account for multiple entities (organizations) we shall separate the files in a container per entity. This means that every entity needs to have their own container. When creating a new container through the `BlobStorageService` a new SAS Token is created and the container address will be returned. When you want to upload or read a blob from that storage you need to pass the proper container address.

### Getting started

Inside the `Startup.cs` add the following line of code to the `ConfigureServices` method to add the blob storage services:

```csharp
services.AddBlobStorage(options =>
{
    options.ConnectionString = Configuration.GetConnectionString("AzureStorage");
});
```

You can now inject the `BlobStorageService` into your business logic to connect to the blob containers.

## Table Storage [WIP]

Coming soon...

# FarmerConnect.Azure.Messaging [WIP]

[![NuGet Package Build](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-messaging.yml/badge.svg?branch=main)](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-messaging.yml)

```powershell
dotnet add package FarmerConnect.Azure.Messaging
```

This package has all you need to start of implementing a messaging pattern with Azure Service Bus. When adding the package and configuring the messaging using the extension method a background worker will register the consumer service.

### Configuration

Register all the event handlers with the DI system of ASP.NET Core that dependencies can be used.

```csharp
// Add event handlers so that they can be resolved (Transient or Scoped)
services.AddTransient<AcceptEventHandler>();
```

After defining the events and event handlers we need to register them with the `EventSubscriptionManager`. The easiest way to do this is by adding a background service that will be executed at the application startup and registers the events and event handlers.

```csharp
public class EventBusRegistrationBackgroundService : IHostedService
{
    private readonly EventBusSubscriptionManager _subscriptionManager;
    private readonly ILogger<EventBusRegistrationBackgroundService> _logger;

    public EventBusRegistrationBackgroundService(EventBusSubscriptionManager subscriptionManager, ILogger<EventBusRegistrationBackgroundService> logger)
    {
        _subscriptionManager = subscriptionManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriptionManager.Subscribe<AcceptEvent, AcceptEventHandler>();

        _logger.LogInformation("Completed event handler registration");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

### Events

### Event handlers
Event handlers need to be registered with the DI System so that they can be instantiated during runtime.

```csharp
```

# Add GitHub to NuGet sources

We are using the GitHub NuGet feed. To be able to access it you need to add this feed to your NuGet Config file. Because the packages might be private you will need to provide a PAT (Personal Access Token). You can get your PAT from your GitHub profile.

Then run the following command: 

```powershell
dotnet nuget add source --username USERNAME --password PERSONAL_ACCESS_TOKEN --name farmerconnect-github "https://nuget.pkg.github.com/farmerconnect/index.json"
```

Replace `USERNAME` and `PERSONAL_ACCESS_TOKEN` with your values.

# Source Link

This repository has source link enabled. This enables stepping into the code of this NuGet Package. Follow the instructions [here](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink) if required.

# License

Check the [LICENSE](LICENSE) file
