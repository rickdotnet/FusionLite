# FusionLite: SQLite-Persisted Cache for FusionCache

FusionLite is a persistent caching solution for .NET applications that integrates with the powerful FusionCache library. It uses SQLite to store cache data, providing a robust and reliable caching mechanism. Although FusionLite implements the `IDistributedCache` interface for compatibility and ease of use, it is essentially a persisted cache ideal for scenarios where data durability and resilience are key.

## Features

- **Persistence**: Uses SQLite for durable cache storage, ensuring that cached data survives application restarts and crashes.
- **FusionCache Integration**: Built to work in tandem with [ZiggyCreatures' FusionCache](https://github.com/ZiggyCreatures/FusionCache), benefiting from its advanced caching features and policies.
- **SQLite Flexibility**: Supports both file-based and in-memory SQLite databases for cache storage, catering to different application needs.
- **Configurable**: Offers customizable options, including cache file location, connection pooling, and more.
- **Asynchronous Support**: Embraces the async/await pattern in .NET, allowing for efficient, non-blocking cache operations.
- **Ease of Setup**: Can be easily added to the service collection and configured with a fluent API, simplifying integration into your .NET projects.

## Installation

To begin using FusionLite in your application, install the package via your preferred NuGet package manager:

```
Install-Package RickDotNet.FusionLite
```

## Configuration

In your application's startup configuration, add FusionLite to the service collection as follows:

```csharp
// wherever you register DI services
var fusion = builder.Services  
    .AddFusionCache()  
    .WithFusionLite(builder.Services) // SystemTextJsonSerializer is included
    .WithDefaultEntryOptions(new FusionCacheEntryOptions(TimeSpan.FromMinutes(2)));
```

## Usage

After setting up FusionLite, you can use FusionCache as normal.

```csharp
var id = 42;

cache.GetOrSet<Product>(
	$"product:{id}",
	_ => GetProductFromDb(id),
	TimeSpan.FromSeconds(30)
```

## License

FusionLite is released under the [MIT License](https://opensource.org/licenses/MIT), giving you the freedom to use, modify, and distribute the software as you see fit within the license's parameters.
