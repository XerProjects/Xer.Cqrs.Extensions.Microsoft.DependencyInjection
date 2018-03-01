using System.Reflection;
using Xer.Cqrs.Extensions.Microsoft.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCqrs(this IServiceCollection serviceCollection, params Assembly[] assemblies)
        {
            AddCqrsCore(serviceCollection)
                .AddCommandHandlers(select => 
                    select.ByInterface(assemblies)
                          .ByAttribute(assemblies))
                .AddEventHandlers(select => 
                    select.ByInterface(assemblies)
                          .ByAttribute(assemblies));

            return serviceCollection;
        }

        public static ICqrsBuilder AddCqrsCore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new System.ArgumentNullException(nameof(serviceCollection));
            }

            return new CqrsBuilder(serviceCollection);
        }
    }
}
