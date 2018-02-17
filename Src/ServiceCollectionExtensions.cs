using System.Collections.Generic;
using System.Reflection;
using Xer.Cqrs.Extensions.Microsoft.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCqrs(this IServiceCollection serviceCollection, Assembly assembly)
        {
            return AddCqrs(serviceCollection, new[] { assembly });
        }

        public static IServiceCollection AddCqrs(this IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies)
        {
            AddCqrsCore(serviceCollection)
                .AddCommandHandlers(assemblies)
                .AddCommandHandlersAttributes(assemblies)
                .AddEventHandlers(assemblies)
                .AddEventHandlersAttributes(assemblies);

            return serviceCollection;
        }

        public static ICqrsBuilder AddCqrsCore(this IServiceCollection serviceCollection)
        {
            // Builder will search in entry and reference assemblies if no assembly is provided.
            return new CqrsBuilder(serviceCollection)
                .AddCommandDelegator()
                .AddEventDelegator();
        }
    }
}
