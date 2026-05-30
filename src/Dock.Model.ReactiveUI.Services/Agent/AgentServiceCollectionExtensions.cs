using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Registers agent bridge services for ReactiveUI applications.
/// </summary>
public static class AgentServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core agent bridge services: command registry, activity tracker, execution recorder, and scenario recorder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureRegistry">An optional registry configuration callback.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDockAgentBridge(
        this IServiceCollection services,
        Action<IServiceProvider, AgentCommandRegistry>? configureRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<UiActivityTracker>();
        services.AddSingleton<AgentExecutionRecorder>();
        services.AddSingleton<AgentScenarioRecorder>();
        services.AddSingleton<AgentCommandRegistry>(provider =>
        {
            var registry = new AgentCommandRegistry(provider.GetRequiredService<UiActivityTracker>());
            configureRegistry?.Invoke(provider, registry);
            return registry;
        });
        services.AddSingleton<IAgentCommandRegistry>(provider => provider.GetRequiredService<AgentCommandRegistry>());

        return services;
    }
}
