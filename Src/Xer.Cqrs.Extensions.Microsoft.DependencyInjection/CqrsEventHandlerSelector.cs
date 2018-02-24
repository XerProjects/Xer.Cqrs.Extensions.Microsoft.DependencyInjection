using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Delegator.Registrations;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    internal class CqrsEventHandlerSelector : ICqrsEventHandlerSelector
    {
        private readonly IServiceCollection _serviceCollection;

        internal CqrsEventHandlerSelector(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public ICqrsEventHandlerSelector ByInterface(Assembly assembly)
        {
            return ByInterface(assembly, ServiceLifetime.Transient);
        }

        public ICqrsEventHandlerSelector ByInterface(Assembly assembly, ServiceLifetime lifetime)
        {
            return ByInterface(new[] { assembly }, lifetime);
        }

        public ICqrsEventHandlerSelector ByInterface(IEnumerable<Assembly> assemblies)
        {
            return ByInterface(assemblies, ServiceLifetime.Transient);
        }

        public ICqrsEventHandlerSelector ByInterface(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
        {
            if (assemblies == null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            _serviceCollection.Scan(scan => scan
                .FromAssemblies(assemblies)
                // Register async and sync event handlers
                .AddClasses(classes => classes.AssignableToAny(typeof(IEventAsyncHandler<>), typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            _serviceCollection.AddSingleton<EventHandlerDelegateResolver>(serviceProvider =>
            {
                return new EventHandlerDelegateResolver(new ContainerEventHandlerResolver(new ServiceProviderAdapter(serviceProvider)));
            });

            return this;
        }
        
        public ICqrsEventHandlerSelector ByAttribute(Assembly assembly)
        {
            return ByAttribute(assembly, ServiceLifetime.Transient);
        }

        public ICqrsEventHandlerSelector ByAttribute(Assembly assembly, ServiceLifetime lifetime)
        {
            return ByAttribute(new[] { assembly }, lifetime);
        }

        public ICqrsEventHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies)
        {
            return ByAttribute(assemblies, ServiceLifetime.Transient);
        }

        public ICqrsEventHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
        {
            if (assemblies == null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            _serviceCollection.Scan(scan => scan
                .FromAssemblies(assemblies)
                // Register classes that has a method marked with [EventHandler]
                .AddClasses(classes => classes.Where(type => EventHandlerAttributeMethod.IsFoundInType(type)))
                .AsSelf()
                .WithLifetime(lifetime));

            _serviceCollection.AddSingleton<EventHandlerDelegateResolver>(serviceProvider => 
            {
                var multiMessageHandlerRegistration = new MultiMessageHandlerRegistration();
                multiMessageHandlerRegistration.RegisterEventHandlerAttributes(assemblies, serviceProvider.GetRequiredService);
                
                return new EventHandlerDelegateResolver(multiMessageHandlerRegistration.BuildMessageHandlerResolver());
            });

            return this;
        }
    }
}