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
        private readonly CqrsCommandHandlerSelector _commandHandlerSelector;
        private readonly CqrsEventHandlerSelector _eventHandlerSelector;

        internal CqrsBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _commandHandlerSelector = new CqrsCommandHandlerSelector(serviceCollection);
            _eventHandlerSelector = new CqrsEventHandlerSelector(serviceCollection);
        }

        public ICqrsBuilder AddCommandHandlers(Action<ICqrsCommandHandlerSelector> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            selector(_commandHandlerSelector);

            _serviceCollection.AddSingleton<CommandDelegator>(serviceProvider => 
            {
                CommandHandlerDelegateResolver[] messageHandlerResolvers = serviceProvider.GetServices<CommandHandlerDelegateResolver>().ToArray();
                
                if (messageHandlerResolvers.Length == 1)
                {
                    return new CommandDelegator(messageHandlerResolvers[0]);
                }
                else if (messageHandlerResolvers.Length > 1)
                {
                    return new CommandDelegator(CompositeMessageHandlerResolver.Compose(messageHandlerResolvers));
                }
                
                // Empty delegator.
                return new CommandDelegator(new SingleMessageHandlerRegistration().BuildMessageHandlerResolver());
            });

            return this;
        }

        public ICqrsBuilder AddEventHandlers(Action<ICqrsEventHandlerSelector> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            selector(_eventHandlerSelector);

            _serviceCollection.AddSingleton<EventDelegator>(serviceProvider => 
            {
                EventHandlerDelegateResolver[] messageHandlerResolvers = serviceProvider.GetServices<EventHandlerDelegateResolver>().ToArray();
                
                if (messageHandlerResolvers.Length == 1)
                {
                    return new EventDelegator(messageHandlerResolvers[0]);
                }
                else if (messageHandlerResolvers.Length > 1)
                {
                    return new EventDelegator(CompositeMessageHandlerResolver.Compose(messageHandlerResolvers));
                }

                // Empty delegator.
                return new EventDelegator(new MultiMessageHandlerRegistration().BuildMessageHandlerResolver());
            });

            return this;
        }
    }
}