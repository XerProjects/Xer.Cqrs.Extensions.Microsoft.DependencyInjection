using System;
using Xer.Delegator;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    internal class EventHandlerDelegateResolver : IMessageHandlerResolver
    {
        private readonly IMessageHandlerResolver _messageHandlerResolver;

        internal EventHandlerDelegateResolver(IMessageHandlerResolver messageHandlerResolver)
        {
            _messageHandlerResolver = messageHandlerResolver;
        }

        public MessageHandlerDelegate ResolveMessageHandler(Type messageType) => _messageHandlerResolver.ResolveMessageHandler(messageType);
    }
}