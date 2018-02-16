using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Xer.Cqrs.CommandStack;
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
            return new CqrsBuilder(serviceCollection, GetEntryAndReferenceAssemblies())
                .AddCommandDelegator()
                .AddEventDelegator();
        }
        
        private static IEnumerable<Assembly> GetEntryAndReferenceAssemblies()
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach (AssemblyName assemblyName in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                try
                {
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch
                {
                    // Ignore any assemblies that cannot be loaded.
                }
            }

            return assemblies;
        }
    }
}
