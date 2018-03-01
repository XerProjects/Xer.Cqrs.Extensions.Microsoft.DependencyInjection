using System;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsBuilder
    {
        ICqrsBuilder AddCommandHandlers(Action<ICqrsCommandHandlerSelector> selector);
        ICqrsBuilder AddEventHandlers(Action<ICqrsEventHandlerSelector> selector);
    }
}