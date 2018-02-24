using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsCommandHandlerSelector
    {
        ICqrsCommandHandlerSelector ByInterface(Assembly assembly);
        ICqrsCommandHandlerSelector ByInterface(Assembly assembly, ServiceLifetime lifetime);
        ICqrsCommandHandlerSelector ByInterface(IEnumerable<Assembly> assemblies);
        ICqrsCommandHandlerSelector ByInterface(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime);
        ICqrsCommandHandlerSelector ByAttribute(Assembly assembly);
        ICqrsCommandHandlerSelector ByAttribute(Assembly assembly, ServiceLifetime lifetime);
        ICqrsCommandHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies);
        ICqrsCommandHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime);
    }
}