using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public class ServiceProviderAdapter : CommandStack.Resolvers.IContainerAdapter,
                                          EventStack.Resolvers.IContainerAdapter
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderAdapter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;    
        }

        public T Resolve<T>() where T : class => _serviceProvider.GetService<T>();
        public IEnumerable<T> ResolveMultiple<T>() where T : class => _serviceProvider.GetServices<T>();
    }
}