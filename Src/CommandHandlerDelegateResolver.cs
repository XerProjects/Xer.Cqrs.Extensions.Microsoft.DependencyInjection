using System;
using Xer.Delegator;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public class CommandHandlerDelegateResolver : IMessageHandlerResolver
    {
        private readonly IMessageHandlerResolver _messageHandlerResolver;

        public CommandHandlerDelegateResolver(IMessageHandlerResolver messageHandlerResolver)
        {
            _messageHandlerResolver = messageHandlerResolver;
        }

        public MessageHandlerDelegate ResolveMessageHandler(Type messageType) => _messageHandlerResolver.ResolveMessageHandler(messageType);
    }
}