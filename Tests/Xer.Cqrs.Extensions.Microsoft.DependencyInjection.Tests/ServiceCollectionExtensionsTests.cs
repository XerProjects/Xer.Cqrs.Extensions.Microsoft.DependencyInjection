using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tests.Entities.CommandHandlers;
using Tests.Entities.EventHandlers;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.Extensions.Microsoft.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class ServiceCollectionExtensionsTests
    {
        public class AddCqrsMethods
        {
            private readonly System.Reflection.Assembly _handlerAssembly = typeof(TestCommand).Assembly;
            private readonly ITestOutputHelper _outputHelper;

            public AddCqrsMethods(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;  
            }

            [Fact]
            public void ShouldRegisterAllCommandHandlersAndEventHandlersInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrs(_handlerAssembly);
                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var commandHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>();
                var eventHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>();

                // Two Resolvers:
                // 1. Command handler resolver
                // 2. Command handler attribute resolver
                commandHandlerResolvers.Should().HaveCount(2);
                
                // Two Resolvers:
                // 1. Event handler resolver
                // 2. Event handler attribute resolver
                eventHandlerResolvers.Should().HaveCount(2);

                serviceProvider.GetService<CommandDelegator>().Should().NotBeNull();
                serviceProvider.GetService<EventDelegator>().Should().NotBeNull();
                serviceProvider.GetService<TestCommandHandler>().Should().NotBeNull();
                serviceProvider.GetService<TestEventHandler>().Should().NotBeNull();
                serviceProvider.GetService<ICommandAsyncHandler<TestCommand>>().Should().NotBeNull();
                serviceProvider.GetService<ICommandHandler<TestCommand>>().Should().NotBeNull();
                serviceProvider.GetService<IEventAsyncHandler<TestEvent>>().Should().NotBeNull();
                serviceProvider.GetService<IEventHandler<TestEvent>>().Should().NotBeNull();
            }

            [Fact]
            public void ShouldRegisterAllCommandHandlersInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddCommandHandlers(select => select.ByInterface(_handlerAssembly));
                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var commandHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>();

                // One Resolver
                // 1. Command handler resolver
                commandHandlerResolvers.Should().HaveCount(1);

                serviceProvider.GetService<CommandDelegator>().Should().NotBeNull();
                serviceProvider.GetService<ICommandAsyncHandler<TestCommand>>().Should().NotBeNull();
                serviceProvider.GetService<ICommandHandler<TestCommand>>().Should().NotBeNull();

                // Null because attributes were not registered.
                serviceProvider.GetService<TestCommandHandler>().Should().BeNull();
            }

            [Fact]
            public void ShouldRegisterAllCommandHandlerAttributesInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddCommandHandlers(select => select.ByAttribute(ServiceLifetime.Transient, _handlerAssembly));
                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var commandHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>();

                // One Resolver
                // 1. Command handler attribute resolver
                commandHandlerResolvers.Should().HaveCount(1);

                serviceProvider.GetService<CommandDelegator>().Should().NotBeNull();
                serviceProvider.GetService<TestCommandHandler>().Should().NotBeNull();

                // Null because interfaces were not registered.
                serviceProvider.GetService<ICommandAsyncHandler<TestCommand>>().Should().BeNull();
                serviceProvider.GetService<ICommandHandler<TestCommand>>().Should().BeNull();
            }

            [Fact]
            public void ShouldRegisterAllEventHandlersInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddEventHandlers(select => select.ByInterface(_handlerAssembly));
                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var eventHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>();

                // One Resolver
                // 1. Event handler resolver
                eventHandlerResolvers.Should().HaveCount(1);

                serviceProvider.GetService<EventDelegator>().Should().NotBeNull();
                serviceProvider.GetService<IEventAsyncHandler<TestEvent>>().Should().NotBeNull();
                serviceProvider.GetService<IEventHandler<TestEvent>>().Should().NotBeNull();

                // Null because attributes were not registered.
                serviceProvider.GetService<TestEventHandler>().Should().BeNull();
            }

            [Fact]
            public void ShouldRegisterAllEventHandlerAttributesInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddEventHandlers(select => select.ByAttribute(_handlerAssembly));
                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var eventHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>();

                // One Resolver
                // 1. Event handler attribute resolver
                eventHandlerResolvers.Should().HaveCount(1);

                serviceProvider.GetService<EventDelegator>().Should().NotBeNull();
                serviceProvider.GetService<TestEventHandler>().Should().NotBeNull();

                // Null because interfaces were not registered.
                serviceProvider.GetService<IEventAsyncHandler<TestEvent>>().Should().BeNull();
                serviceProvider.GetService<IEventHandler<TestEvent>>().Should().BeNull();
            }
        }
    }
}
