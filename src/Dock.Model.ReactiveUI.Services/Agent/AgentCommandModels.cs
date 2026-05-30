using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Describes a command that can be discovered and invoked by agent clients.
/// </summary>
/// <param name="Id">The stable command identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="ViewModel">The view model type name that owns the command.</param>
/// <param name="Description">A planner-friendly command description.</param>
/// <param name="InputType">The CLR input type name.</param>
/// <param name="OutputType">The CLR output type name.</param>
/// <param name="InputSchema">The JSON schema for command input.</param>
/// <param name="OutputSchema">The JSON schema for command output.</param>
/// <param name="CanExecute">A value indicating whether the command can execute now.</param>
/// <param name="IsExecuting">A value indicating whether the command is currently executing.</param>
/// <param name="Destructive">A value indicating whether the command may mutate or delete user data.</param>
/// <param name="RequiresConfirmation">A value indicating whether clients should confirm before invoking.</param>
/// <param name="RequiresIdleAfterInvoke">A value indicating whether clients should wait for UI idle after invoking.</param>
public sealed record AgentCommandDescriptor(
    string Id,
    string Name,
    string ViewModel,
    string Description,
    string InputType,
    string OutputType,
    JsonElement InputSchema,
    JsonElement OutputSchema,
    bool CanExecute,
    bool IsExecuting,
    bool Destructive,
    bool RequiresConfirmation,
    bool RequiresIdleAfterInvoke);

/// <summary>
/// Represents an agent command invocation request.
/// </summary>
/// <param name="Input">The optional JSON input payload.</param>
public sealed record AgentInvokeRequest(JsonElement? Input);

/// <summary>
/// Represents an agent command invocation response.
/// </summary>
/// <param name="Success">A value indicating whether invocation completed successfully.</param>
/// <param name="Result">The command result.</param>
/// <param name="Error">The invocation error, when any.</param>
public sealed record AgentInvokeResponse(bool Success, object? Result, string? Error)
{
    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="result">The command result.</param>
    /// <returns>A successful response.</returns>
    public static AgentInvokeResponse Ok(object? result) => new(true, result, null);

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed response.</returns>
    public static AgentInvokeResponse Fail(string error) => new(false, null, error);
}

/// <summary>
/// Provides access to the current view model exposed by the host application.
/// </summary>
public interface ICurrentViewModelProvider
{
    /// <summary>
    /// Gets the current view model, or <see langword="null"/> when no route is active.
    /// </summary>
    object? CurrentViewModel { get; }
}

/// <summary>
/// Defines a registry of strongly typed agent commands.
/// </summary>
public interface IAgentCommandRegistry
{
    /// <summary>
    /// Gets command descriptors that are valid for the supplied current view model.
    /// </summary>
    /// <param name="currentViewModel">The current view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The available command descriptors.</returns>
    Task<IReadOnlyList<AgentCommandDescriptor>> GetAvailableCommandsAsync(object? currentViewModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a registered command on the supplied current view model.
    /// </summary>
    /// <param name="id">The command identifier.</param>
    /// <param name="currentViewModel">The current view model.</param>
    /// <param name="input">The optional JSON input payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<object?> InvokeAsync(string id, object? currentViewModel, JsonElement? input, CancellationToken cancellationToken = default);
}
