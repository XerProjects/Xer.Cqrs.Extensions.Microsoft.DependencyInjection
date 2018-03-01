using System.Linq;
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

        public ICqrsEventHandlerSelector ByInterface(params Assembly[] assemblies)
        {
            return ByInterface(ServiceLifetime.Transient, assemblies);
        }

        public ICqrsEventHandlerSelector ByInterface(ServiceLifetime lifetime, params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            _serviceCollection.Scan(scan => scan
                // Scan distinct assemblies.
                .FromAssemblies(assemblies.Distinct())
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

        public ICqrsEventHandlerSelector ByAttribute(params Assembly[] assemblies)
        {
            return ByAttribute(ServiceLifetime.Transient, assemblies);
        }

        public ICqrsEventHandlerSelector ByAttribute(ServiceLifetime lifetime, params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

            _serviceCollection.Scan(scan => scan
                // Scan distinct assemblies.
                .FromAssemblies(assemblies.Distinct())
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