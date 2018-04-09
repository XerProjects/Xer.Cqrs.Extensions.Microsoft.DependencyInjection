using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Extensions.Attributes;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Delegator.Registration;
using Xer.Delegator.Resolvers;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    internal class CqrsCommandHandlerSelector : ICqrsCommandHandlerSelector
    {
        private readonly IServiceCollection _serviceCollection;

        internal CqrsCommandHandlerSelector(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public ICqrsCommandHandlerSelector ByInterface(params Assembly[] assemblies)
        {
            return ByInterface(ServiceLifetime.Transient, assemblies);
        }

        public ICqrsCommandHandlerSelector ByInterface(ServiceLifetime lifetime, params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (assemblies.Length == 0)
            {
                throw new ArgumentException("No assemblies were provided.", nameof(assemblies));
            }

            _serviceCollection.Scan(scan => scan
                // Scan distinct assemblies.
                .FromAssemblies(assemblies.Distinct())
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

        public ICqrsCommandHandlerSelector ByAttribute(params Assembly[] assemblies)
        {
            return ByAttribute(ServiceLifetime.Transient, assemblies);
        }

        public ICqrsCommandHandlerSelector ByAttribute(ServiceLifetime lifetime, params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (assemblies.Length == 0)
            {
                throw new ArgumentException("No assemblies were provided.", nameof(assemblies));
            }

            _serviceCollection.Scan(scan => scan
                // Scan distinct assemblies.
                .FromAssemblies(assemblies.Distinct())
                // Register classes that has a method marked with [CommandHandler]
                .AddClasses(classes => classes.Where(type => CommandHandlerAttributeMethod.IsFoundInType(type)))
                .AsSelf()
                .WithLifetime(lifetime));
            
            _serviceCollection.AddSingleton<CommandHandlerDelegateResolver>(serviceProvider => 
            {
                var singleMessageHandlerRegistration = new SingleMessageHandlerRegistration();
                singleMessageHandlerRegistration.RegisterCommandHandlersByAttribute(assemblies, serviceProvider.GetRequiredService);

                return new CommandHandlerDelegateResolver(singleMessageHandlerRegistration.BuildMessageHandlerResolver());
            });

            return this;
        }
    }
}