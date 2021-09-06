<img align="left" width="170" height="100" src=".github/fc-logo.png" />

# FarmerConnect.Azure
[![CI-Build](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-ci.yml/badge.svg)](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-ci.yml)

<br />

This repository holds all the packages that have been standardized inside of FarmerConnect SA on how to use different Azure resources and patterns.

## FarmerConnect.Azure.Storage

[![NuGet Package Build](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-storage.yml/badge.svg?branch=main)](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-storage.yml)

```powershell
dotnet add package FarmerConnect.Azure.Storage
```

### Blob storage

Inside the `Startup.cs` add the following line of code to the `ConfigureServices` method to add the blob storage services:

```csharp

```

### Table Storage

## FarmerConnect.Azure.Messaging

[![NuGet Package Build](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-messaging.yml/badge.svg?branch=main)](https://github.com/farmerconnect/FarmerConnect.Azure/actions/workflows/workflow-messaging.yml)

```powershell
dotnet add package FarmerConnect.Azure.Messaging
```

This package has all you need to start of implementing a messaging pattern with Azure Service Bus. When adding the package and configuring the messaging using the extension method a background worker will register the consumer service.

### Configuration

### Events

### Event handlers
Event handlers need to be registered with the DI System so that they can be instantiated during runtime.

```csharp
```

# Source Link

This repository has source link enabled to enable stepping into the code of this NuGet Package. Follow the instructions [here](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink) if required.

# License

Check the [LICENSE](LICENSE) file
