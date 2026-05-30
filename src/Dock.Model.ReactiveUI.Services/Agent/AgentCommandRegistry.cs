using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Stores source-generated or manually registered ReactiveUI commands for deterministic agent invocation.
/// </summary>
public sealed class AgentCommandRegistry : IAgentCommandRegistry
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, IAgentCommandBinding> _bindings = new(StringComparer.Ordinal);
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly UiActivityTracker? _activityTracker;
    private readonly TimeSpan _idleTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentCommandRegistry"/> class.
    /// </summary>
    /// <param name="activityTracker">The optional activity tracker used to wait for idle after invocations.</param>
    /// <param name="jsonOptions">The JSON options used for input conversion.</param>
    /// <param name="idleTimeout">The idle timeout used after commands that require idle.</param>
    public AgentCommandRegistry(
        UiActivityTracker? activityTracker = null,
        JsonSerializerOptions? jsonOptions = null,
        TimeSpan? idleTimeout = null)
    {
        _activityTracker = activityTracker;
        _jsonOptions = jsonOptions ?? DefaultJsonOptions;
        _idleTimeout = idleTimeout ?? TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Registers a strongly typed ReactiveUI command binding.
    /// </summary>
    /// <typeparam name="TViewModel">The owning view model type.</typeparam>
    /// <typeparam name="TInput">The command input type.</typeparam>
    /// <typeparam name="TOutput">The command output type.</typeparam>
    /// <param name="id">The stable command identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The planner-friendly description.</param>
    /// <param name="getCommand">A function that returns the command from the view model.</param>
    /// <param name="inputSchema">The optional input schema.</param>
    /// <param name="outputSchema">The optional output schema.</param>
    /// <param name="destructive">A value indicating whether the command may mutate or delete data.</param>
    /// <param name="requiresConfirmation">A value indicating whether invocation should be confirmed first.</param>
    /// <param name="requiresIdleAfterInvoke">A value indicating whether invocation waits for idle.</param>
    public void Register<TViewModel, TInput, TOutput>(
        string id,
        string name,
        string description,
        Func<TViewModel, ReactiveCommand<TInput, TOutput>> getCommand,
        JsonElement? inputSchema = null,
        JsonElement? outputSchema = null,
        bool destructive = false,
        bool requiresConfirmation = false,
        bool requiresIdleAfterInvoke = true)
        where TViewModel : class
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Command id is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(getCommand);

        _bindings.Add(
            id,
            new AgentCommandBinding<TViewModel, TInput, TOutput>(
                id,
                name,
                description,
                getCommand,
                inputSchema ?? AgentJsonSchemas.CreateFor<TInput>(),
                outputSchema ?? AgentJsonSchemas.CreateFor<TOutput>(),
                destructive,
                requiresConfirmation,
                requiresIdleAfterInvoke));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgentCommandDescriptor>> GetAvailableCommandsAsync(
        object? currentViewModel,
        CancellationToken cancellationToken = default)
    {
        var descriptors = new List<AgentCommandDescriptor>(_bindings.Count);

        foreach (var binding in _bindings.Values)
        {
            if (!binding.CanBind(currentViewModel))
            {
                continue;
            }

            descriptors.Add(await binding.CreateDescriptorAsync(currentViewModel!, cancellationToken).ConfigureAwait(false));
        }

        return descriptors;
    }

    /// <inheritdoc />
    public async Task<object?> InvokeAsync(
        string id,
        object? currentViewModel,
        JsonElement? input,
        CancellationToken cancellationToken = default)
    {
        if (!_bindings.TryGetValue(id, out var binding))
        {
            throw new InvalidOperationException($"Agent command '{id}' is not registered.");
        }

        if (!binding.CanBind(currentViewModel))
        {
            var actual = currentViewModel?.GetType().FullName ?? "<none>";
            throw new InvalidOperationException($"Agent command '{id}' is not available for current view model '{actual}'.");
        }

        using var activity = _activityTracker?.Begin(id);
        var result = await binding.InvokeAsync(currentViewModel!, input, _jsonOptions, cancellationToken).ConfigureAwait(false);

        if (binding.RequiresIdleAfterInvoke && _activityTracker is not null)
        {
            await _activityTracker.WaitForIdleAsync(_idleTimeout, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private interface IAgentCommandBinding
    {
        bool RequiresIdleAfterInvoke { get; }

        bool CanBind(object? viewModel);

        Task<AgentCommandDescriptor> CreateDescriptorAsync(object viewModel, CancellationToken cancellationToken);

        Task<object?> InvokeAsync(object viewModel, JsonElement? input, JsonSerializerOptions options, CancellationToken cancellationToken);
    }

    private sealed class AgentCommandBinding<TViewModel, TInput, TOutput> : IAgentCommandBinding
        where TViewModel : class
    {
        private readonly string _id;
        private readonly string _name;
        private readonly string _description;
        private readonly Func<TViewModel, ReactiveCommand<TInput, TOutput>> _getCommand;
        private readonly JsonElement _inputSchema;
        private readonly JsonElement _outputSchema;
        private readonly bool _destructive;
        private readonly bool _requiresConfirmation;

        public AgentCommandBinding(
            string id,
            string name,
            string description,
            Func<TViewModel, ReactiveCommand<TInput, TOutput>> getCommand,
            JsonElement inputSchema,
            JsonElement outputSchema,
            bool destructive,
            bool requiresConfirmation,
            bool requiresIdleAfterInvoke)
        {
            _id = id;
            _name = string.IsNullOrWhiteSpace(name) ? id : name;
            _description = description;
            _getCommand = getCommand;
            _inputSchema = inputSchema;
            _outputSchema = outputSchema;
            _destructive = destructive;
            _requiresConfirmation = requiresConfirmation;
            RequiresIdleAfterInvoke = requiresIdleAfterInvoke;
        }

        public bool RequiresIdleAfterInvoke { get; }

        public bool CanBind(object? viewModel) => viewModel is TViewModel;

        public async Task<AgentCommandDescriptor> CreateDescriptorAsync(object viewModel, CancellationToken cancellationToken)
        {
            var command = _getCommand((TViewModel)viewModel);
            var canExecute = await command.CanExecute.Take(1).ToTask(cancellationToken).ConfigureAwait(false);
            var isExecuting = await command.IsExecuting.Take(1).ToTask(cancellationToken).ConfigureAwait(false);

            return new AgentCommandDescriptor(
                _id,
                _name,
                typeof(TViewModel).Name,
                _description,
                typeof(TInput).FullName ?? typeof(TInput).Name,
                typeof(TOutput).FullName ?? typeof(TOutput).Name,
                _inputSchema,
                _outputSchema,
                canExecute,
                isExecuting,
                _destructive,
                _requiresConfirmation,
                RequiresIdleAfterInvoke);
        }

        public async Task<object?> InvokeAsync(
            object viewModel,
            JsonElement? input,
            JsonSerializerOptions options,
            CancellationToken cancellationToken)
        {
            var command = _getCommand((TViewModel)viewModel);
            var typedInput = AgentJsonInput.Deserialize<TInput>(input, options);
            var result = await command.Execute(typedInput).Take(1).ToTask(cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}

/// <summary>
/// Provides small built-in JSON schema documents for common command payloads.
/// </summary>
public static class AgentJsonSchemas
{
    /// <summary>
    /// Creates a JSON schema for the supplied type parameter.
    /// </summary>
    /// <typeparam name="T">The CLR type.</typeparam>
    /// <returns>A JSON schema element.</returns>
    public static JsonElement CreateFor<T>() => CreateFor(typeof(T));

    /// <summary>
    /// Creates a JSON schema for the supplied type.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <returns>A JSON schema element.</returns>
    public static JsonElement CreateFor(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        JsonObject schema = type == typeof(Unit)
            ? new JsonObject { ["type"] = "null" }
            : type == typeof(string)
                ? new JsonObject { ["type"] = "string" }
                : type == typeof(bool)
                    ? new JsonObject { ["type"] = "boolean" }
                    : IsNumber(type)
                        ? new JsonObject { ["type"] = "number" }
                        : new JsonObject { ["type"] = "object", ["dotnetType"] = type.FullName ?? type.Name };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static bool IsNumber(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(byte)
            || underlyingType == typeof(short)
            || underlyingType == typeof(int)
            || underlyingType == typeof(long)
            || underlyingType == typeof(float)
            || underlyingType == typeof(double)
            || underlyingType == typeof(decimal);
    }
}

internal static class AgentJsonInput
{
    public static T Deserialize<T>(JsonElement? input, JsonSerializerOptions options)
    {
        if (typeof(T) == typeof(Unit))
        {
            return (T)(object)Unit.Default;
        }

        if (input is null)
        {
            throw new InvalidOperationException($"Agent command requires input of type '{typeof(T).FullName}'.");
        }

        var value = input.Value.Deserialize<T>(options);
        if (value is null && Nullable.GetUnderlyingType(typeof(T)) is null && typeof(T).IsValueType)
        {
            throw new InvalidOperationException($"Agent command input could not be converted to '{typeof(T).FullName}'.");
        }

        return value!;
    }
}
