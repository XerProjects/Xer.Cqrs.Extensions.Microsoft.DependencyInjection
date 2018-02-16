using System;
using System.Linq;
using System.Threading.Tasks;
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
            public async Task ShouldRegisterAllCommandHandlersAndEventHandlersInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrs(_handlerAssembly);
                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var commandHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>();
                var eventHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>();
                var commandDelegator = serviceProvider.GetRequiredService<CommandDelegator>();
                var eventDelegator = serviceProvider.GetRequiredService<EventDelegator>();

                // Two Resolvers:
                // 1. Command handler resolver
                // 2. Command handler attribute resolver
                commandHandlerResolvers.Should().HaveCount(2);
                
                // Two Resolvers:
                // 1. Event handler resolver
                // 2. Event handler attribute resolver
                eventHandlerResolvers.Should().HaveCount(2);

                await commandDelegator.SendAsync(new TestCommand());
                await eventDelegator.SendAsync(new TestEvent());
            }

            [Fact]
            public async Task ShouldRegisterAllCommandHandlersInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddCommandHandlers(_handlerAssembly);

                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var commandHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>();
                var commandDelegator = serviceProvider.GetRequiredService<CommandDelegator>();

                commandHandlerResolvers.Should().HaveCount(1);

                await commandDelegator.SendAsync(new TestCommand());
            }

            [Fact]
            public async Task ShouldRegisterAllCommandHandlerAttributesInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddCommandHandlersAttributes(_handlerAssembly);

                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var commandHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>();
                var commandDelegator = serviceProvider.GetRequiredService<CommandDelegator>();

                commandHandlerResolvers.Should().HaveCount(1);

                await commandDelegator.SendAsync(new TestCommand());
            }

            [Fact]
            public async Task ShouldRegisterAllEventHandlersInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddEventHandlers(_handlerAssembly);

                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var eventHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>();
                var eventDelegator = serviceProvider.GetRequiredService<EventDelegator>();

                eventHandlerResolvers.Should().HaveCount(1);

                await eventDelegator.SendAsync(new TestEvent());
            }

            [Fact]
            public async Task ShouldRegisterAllEventHandlerAttributesInAssembly()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddCqrsCore()
                                 .AddEventHandlersAttributes(_handlerAssembly);

                serviceCollection.AddSingleton(_outputHelper);

                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                var eventHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>();
                var eventDelegator = serviceProvider.GetRequiredService<EventDelegator>();

                eventHandlerResolvers.Should().HaveCount(1);

                await eventDelegator.SendAsync(new TestEvent());
            }
        }
    }
}
