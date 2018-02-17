using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Attributes;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.Extensions.Microsoft.DependencyInjection.Src;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xer.Delegator.Resolvers;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public class CqrsBuilder : ICqrsBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IEnumerable<Assembly> _entryAndReferenceAssemblies;

        internal CqrsBuilder(IServiceCollection serviceCollection, IEnumerable<Assembly> appDependencies)
        {
            _serviceCollection = serviceCollection;
            _entryAndReferenceAssemblies = appDependencies;
        }

        public ICqrsBuilder AddCommandHandlers(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddCommandHandlers(_entryAndReferenceAssemblies, lifetime);
        }

        public ICqrsBuilder AddCommandHandlers(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddCommandHandlers(new[] { assembly }, lifetime);
        }

        public ICqrsBuilder AddCommandHandlers(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _serviceCollection.Scan(scan => scan
                .FromAssemblies(assemblies)
                // Register async and sync command handlers
                .AddClasses(classes => classes.AssignableToAny(typeof(ICommandAsyncHandler<>), typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            _serviceCollection.AddSingleton<CommandHandlerDelegateResolver>(serviceProvider =>
            {
                return new CommandHandlerDelegateResolver(
                    CompositeMessageHandlerResolver.Compose(
                        new ContainerCommandAsyncHandlerResolver(new ServiceProviderAdapter(serviceProvider)),
                        new ContainerCommandHandlerResolver(new ServiceProviderAdapter(serviceProvider))));
            });

            return this;
        }

        public ICqrsBuilder AddCommandHandlersAttributes(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddCommandHandlersAttributes(_entryAndReferenceAssemblies, lifetime);
        }

        public ICqrsBuilder AddCommandHandlersAttributes(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddCommandHandlersAttributes(new[] { assembly }, lifetime);
        }

        public ICqrsBuilder AddCommandHandlersAttributes(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _serviceCollection.Scan(scan => scan
                .FromAssemblies(assemblies)
                // Register classes that has a method marked with [CommandHandler]
                .AddClasses(classes => classes.Where(type => hasHandlerAttributeMethods<CommandHandlerAttribute>(type)))
                .AsSelf()
                .WithLifetime(lifetime));

            // Get all types that has methods marked with [CommandHandler] attribute.
            Type[] allTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                                                    .Where(type => hasHandlerAttributeMethods<CommandHandlerAttribute>(type))
                                                    .ToArray();
            
            _serviceCollection.AddSingleton<CommandHandlerDelegateResolver>(serviceProvider => 
            {
                var singleMessageHandlerRegistration = new SingleMessageHandlerRegistration();

                foreach(Type type in allTypes)
                {
                    singleMessageHandlerRegistration.RegisterCommandHandlerAttributes(() => serviceProvider.GetRequiredService(type));
                }

                return new CommandHandlerDelegateResolver(singleMessageHandlerRegistration.BuildMessageHandlerResolver());
            });

            return this;
        }

        public ICqrsBuilder AddCommandDelegator()
        {
            _serviceCollection.AddSingleton<CommandDelegator>(serviceProvider => 
            {
                var messageHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>().ToArray();

                if(messageHandlerResolvers.Length == 1)
                {
                    return new CommandDelegator(messageHandlerResolvers[0]);
                }

                return new CommandDelegator(CompositeMessageHandlerResolver.Compose(messageHandlerResolvers));
            });

            return this;
        }
        public ICqrsBuilder AddEventHandlers(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddEventHandlers(_entryAndReferenceAssemblies, lifetime);
        }

        public ICqrsBuilder AddEventHandlers(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddEventHandlers(new[] { assembly }, lifetime);
        }

        public ICqrsBuilder AddEventHandlers(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
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

        public ICqrsBuilder AddEventHandlersAttributes(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddEventHandlersAttributes(_entryAndReferenceAssemblies, lifetime);
        }

        public ICqrsBuilder AddEventHandlersAttributes(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddEventHandlersAttributes(new[] { assembly }, lifetime);
        }

        public ICqrsBuilder AddEventHandlersAttributes(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _serviceCollection.Scan(scan => scan
                .FromAssemblies(assemblies)
                // Register classes that has a method marked with [EventHandler]
                .AddClasses(classes => classes.Where(type => hasHandlerAttributeMethods<EventHandlerAttribute>(type)))
                .AsSelf()
                .WithLifetime(lifetime));

            // Get all types that has methods marked with [EventHandler] attribute.
            Type[] allTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                                                    .Where(type => hasHandlerAttributeMethods<EventHandlerAttribute>(type))
                                                    .ToArray();

            _serviceCollection.AddSingleton<EventHandlerDelegateResolver>(serviceProvider => 
            {
                var multiMessageHandlerRegistration = new MultiMessageHandlerRegistration();

                foreach(Type type in allTypes)
                {
                    multiMessageHandlerRegistration.RegisterEventHandlerAttributes(() => serviceProvider.GetRequiredService(type));
                }

                return new EventHandlerDelegateResolver(multiMessageHandlerRegistration.BuildMessageHandlerResolver());
            });

            return this;
        }

        public ICqrsBuilder AddEventDelegator()
        {
            _serviceCollection.AddSingleton<EventDelegator>(serviceProvider => 
            {
                var messageHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>().ToArray();

                if(messageHandlerResolvers.Length == 1)
                {
                    return new EventDelegator(messageHandlerResolvers[0]);
                }

                return new EventDelegator(CompositeMessageHandlerResolver.Compose(messageHandlerResolvers));
            });

            return this;
        }

        private bool hasHandlerAttributeMethods<TAttribute>(Type type) where TAttribute : Attribute
        {
            return type.GetMethods().Any(method => method.GetCustomAttributes(typeof(TAttribute), true).Any());
        }
    }
}