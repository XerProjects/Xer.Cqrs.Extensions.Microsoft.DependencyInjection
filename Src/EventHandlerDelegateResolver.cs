using System;
using Xer.Delegator;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public class EventHandlerDelegateResolver : IMessageHandlerResolver
    {
        private readonly IMessageHandlerResolver _messageHandlerResolver;

        public EventHandlerDelegateResolver(IMessageHandlerResolver messageHandlerResolver)
        {
            _messageHandlerResolver = messageHandlerResolver;
        }

        public MessageHandlerDelegate ResolveMessageHandler(Type messageType) => _messageHandlerResolver.ResolveMessageHandler(messageType);
    }
}