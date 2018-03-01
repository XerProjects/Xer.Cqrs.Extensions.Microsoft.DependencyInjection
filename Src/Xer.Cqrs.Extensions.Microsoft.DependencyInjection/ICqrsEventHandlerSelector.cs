using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsEventHandlerSelector
    {
        ICqrsEventHandlerSelector ByInterface(params Assembly[] assemblies);
        ICqrsEventHandlerSelector ByInterface(ServiceLifetime lifetime, params Assembly[] assemblies);
        ICqrsEventHandlerSelector ByAttribute(params Assembly[] assemblies);
        ICqrsEventHandlerSelector ByAttribute(ServiceLifetime lifetime, params Assembly[] assemblies);
    }
}