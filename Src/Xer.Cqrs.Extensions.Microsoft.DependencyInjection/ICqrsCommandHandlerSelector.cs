using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsCommandHandlerSelector
    {
        ICqrsCommandHandlerSelector ByInterface(params Assembly[] assemblies);
        ICqrsCommandHandlerSelector ByInterface(ServiceLifetime lifetime, params Assembly[] assemblies);
        ICqrsCommandHandlerSelector ByAttribute(params Assembly[] assemblies);
        ICqrsCommandHandlerSelector ByAttribute(ServiceLifetime lifetime, params Assembly[] assemblies);
    }
}