using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsBuilder
    {
        ICqrsBuilder AddCommandHandlers(Action<ICqrsCommandHandlerSelector> selector);
        ICqrsBuilder AddEventHandlers(Action<ICqrsEventHandlerSelector> selector);
    }
}