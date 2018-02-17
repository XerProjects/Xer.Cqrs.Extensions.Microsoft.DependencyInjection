using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xer.Cqrs.Extensions.Microsoft.DependencyInjection
{
    public interface ICqrsBuilder
    {
        ICqrsBuilder AddCommandHandlers(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddCommandHandlersAttributes(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddCommandHandlers(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddCommandHandlersAttributes(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddCommandDelegator();
        ICqrsBuilder AddEventHandlers(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddEventHandlersAttributes(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddEventHandlers(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddEventHandlersAttributes(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient);
        ICqrsBuilder AddEventDelegator();
    }
}