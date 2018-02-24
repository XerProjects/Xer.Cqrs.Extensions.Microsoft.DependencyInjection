using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsEventHandlerSelector
    {
        ICqrsEventHandlerSelector ByInterface(Assembly assembly);
        ICqrsEventHandlerSelector ByInterface(Assembly assembly, ServiceLifetime lifetime);
        ICqrsEventHandlerSelector ByInterface(IEnumerable<Assembly> assemblies);
        ICqrsEventHandlerSelector ByInterface(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime);
        ICqrsEventHandlerSelector ByAttribute(Assembly assembly);
        ICqrsEventHandlerSelector ByAttribute(Assembly assembly, ServiceLifetime lifetime);
        ICqrsEventHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies);
        ICqrsEventHandlerSelector ByAttribute(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime);
    }
}