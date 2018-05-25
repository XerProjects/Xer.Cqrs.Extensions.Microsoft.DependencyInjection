# Xer.Cqrs.Extensions.Microsoft.DependencyInjection

Extension for `IServiceCollection` to allow easy registration of command handlers and event handlers.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register all CQRS components.
    services.AddCqrs(typeof(CommandHandler).Assembly, 
                     typeof(EventHandler).Assembly);
}
```
