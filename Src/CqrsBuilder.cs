using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Attributes;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Delegator.Registrations;
using Xer.Delegator.Resolvers;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    internal class CqrsBuilder : ICqrsBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        internal CqrsBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public ICqrsBuilder AddCommandDelegator()
        {
            _serviceCollection.AddSingleton<CommandDelegator>(serviceProvider => 
            {
                CommandHandlerDelegateResolver[] messageHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>().ToArray();
                
                if (messageHandlerResolvers.Length > 1)
                {
                    return new CommandDelegator(CompositeMessageHandlerResolver.Compose(messageHandlerResolvers));
                }
                else if (messageHandlerResolvers.Length == 1)
                {
                    return new CommandDelegator(messageHandlerResolvers[0]);
                }

                // Empty delegator.
                return new CommandDelegator(new SingleMessageHandlerRegistration().BuildMessageHandlerResolver());
            });

            return this;
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

        public ICqrsBuilder AddCommandHandlersAttributes(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddCommandHandlersAttributes(new[] { assembly }, lifetime);
        }

        public ICqrsBuilder AddCommandHandlersAttributes(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _serviceCollection.Scan(scan => scan
                .FromAssemblies(assemblies)
                // Register classes that has a method marked with [CommandHandler]
                .AddClasses(classes => classes.Where(type => CommandHandlerAttributeMethod.IsFoundInType(type)))
                .AsSelf()
                .WithLifetime(lifetime));
            
            _serviceCollection.AddSingleton<CommandHandlerDelegateResolver>(serviceProvider => 
            {
                var singleMessageHandlerRegistration = new SingleMessageHandlerRegistration();
                singleMessageHandlerRegistration.RegisterCommandHandlerAttributes(assemblies, serviceProvider.GetRequiredService);

                return new CommandHandlerDelegateResolver(singleMessageHandlerRegistration.BuildMessageHandlerResolver());
            });

            return this;
        }

        public ICqrsBuilder AddEventDelegator()
        {
            _serviceCollection.AddSingleton<EventDelegator>(serviceProvider => 
            {
                EventHandlerDelegateResolver[] messageHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>().ToArray();
                
                if (messageHandlerResolvers.Length > 1)
                {
                    return new EventDelegator(CompositeMessageHandlerResolver.Compose(messageHandlerResolvers));
                }
                else if (messageHandlerResolvers.Length == 1)
                {
                    return new EventDelegator(messageHandlerResolvers[0]);
                }

                // Empty delegator.
                return new EventDelegator(new MultiMessageHandlerRegistration().BuildMessageHandlerResolver());
            });

            return this;
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

        public ICqrsBuilder AddEventHandlersAttributes(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return AddEventHandlersAttributes(new[] { assembly }, lifetime);
        }

        public ICqrsBuilder AddEventHandlersAttributes(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
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