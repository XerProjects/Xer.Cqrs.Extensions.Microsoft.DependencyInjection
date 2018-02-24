﻿using System.Collections.Generic;
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
            return new CqrsBuilder(serviceCollection);
        }
    }
}