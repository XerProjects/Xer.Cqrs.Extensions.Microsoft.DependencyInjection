using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Delegator.Registrations;
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

        public ICqrsCommandHandlerSelector ByInterface(Assembly assembly)
        {
            return ByInterface(assembly, ServiceLifetime.Transient);
        }

        public ICqrsCommandHandlerSelector ByInterface(Assembly assembly, ServiceLifetime lifetime)
        {
            return ByInterface(new[] { assembly }, lifetime);
        }

        public ICqrsCommandHandlerSelector ByInterface(IEnumerable<Assembly> assemblies)
        {
            return ByInterface(assemblies, ServiceLifetime.Transient);
        }

        public ICqrsCommandHandlerSelector ByInterface(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
        {
            if (assemblies == null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

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

        public ICqrsCommandHandlerSelector ByAttribute(Assembly assembly)
        {
            return ByAttribute(assembly, ServiceLifetime.Transient);
        }

        public ICqrsCommandHandlerSelector ByAttribute(Assembly assembly, ServiceLifetime lifetime)
        {
            return ByAttribute(new[] { assembly }, lifetime);
        }

        public ICqrsCommandHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies)
        {
            return ByAttribute(assemblies, ServiceLifetime.Transient);
        }

        public ICqrsCommandHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
        {
            if (assemblies == null)
            {
                throw new System.ArgumentNullException(nameof(assemblies));
            }

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
    }
}