using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Represents a scenario recorded from semantic commands or automation identifiers.
/// </summary>
/// <param name="Name">The scenario name.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
/// <param name="Steps">The recorded steps.</param>
public sealed record RecordedScenario(string Name, DateTimeOffset CreatedAt, IReadOnlyList<RecordedStep> Steps);

/// <summary>
/// Base type for recorded scenario steps.
/// </summary>
public abstract record RecordedStep;

/// <summary>
/// Represents a recorded command invocation.
/// </summary>
/// <param name="CommandId">The command id.</param>
/// <param name="Input">The optional serialized input.</param>
public sealed record RecordedCommandStep(string CommandId, JsonElement? Input = null) : RecordedStep;

/// <summary>
/// Represents recorded text entry by automation id.
/// </summary>
/// <param name="AutomationId">The automation id.</param>
/// <param name="Value">The text value.</param>
public sealed record RecordedSetTextStep(string AutomationId, string Value) : RecordedStep;

/// <summary>
/// Represents a recorded click by automation id.
/// </summary>
/// <param name="AutomationId">The automation id.</param>
public sealed record RecordedClickStep(string AutomationId) : RecordedStep;

/// <summary>
/// Represents a recorded idle wait.
/// </summary>
/// <param name="Timeout">The idle timeout.</param>
public sealed record RecordedWaitIdleStep(TimeSpan Timeout) : RecordedStep;

/// <summary>
/// Represents a recorded route assertion.
/// </summary>
/// <param name="Route">The expected route.</param>
public sealed record RecordedAssertRouteStep(string Route) : RecordedStep;

/// <summary>
/// Represents a recorded state snapshot.
/// </summary>
/// <param name="Name">The snapshot name.</param>
/// <param name="Snapshot">The snapshot payload.</param>
public sealed record RecordedSnapshotStep(string Name, JsonElement Snapshot) : RecordedStep;

/// <summary>
/// Records semantic scenarios for later replay or agent guidance.
/// </summary>
public sealed class AgentScenarioRecorder
{
    private readonly object _gate = new();
    private readonly List<RecordedStep> _steps = new();
    private readonly TimeSpan _defaultWait;
    private bool _isRecording;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentScenarioRecorder"/> class.
    /// </summary>
    /// <param name="defaultWait">The wait inserted after user-visible actions.</param>
    public AgentScenarioRecorder(TimeSpan? defaultWait = null)
    {
        _defaultWait = defaultWait ?? TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Gets a value indicating whether recording is active.
    /// </summary>
    public bool IsRecording
    {
        get
        {
            lock (_gate)
            {
                return _isRecording;
            }
        }
    }

    /// <summary>
    /// Starts recording and clears previously recorded steps.
    /// </summary>
    public void Start()
    {
        lock (_gate)
        {
            _steps.Clear();
            _isRecording = true;
        }
    }

    /// <summary>
    /// Stops recording and returns the scenario.
    /// </summary>
    /// <param name="name">The scenario name.</param>
    /// <returns>The recorded scenario.</returns>
    public RecordedScenario Stop(string name)
    {
        lock (_gate)
        {
            _isRecording = false;
            return new RecordedScenario(name, DateTimeOffset.UtcNow, _steps.ToArray());
        }
    }

    /// <summary>
    /// Gets the current in-progress scenario snapshot.
    /// </summary>
    /// <param name="name">The scenario name to use in the snapshot.</param>
    /// <returns>The scenario snapshot.</returns>
    public RecordedScenario Current(string name = "current")
    {
        lock (_gate)
        {
            return new RecordedScenario(name, DateTimeOffset.UtcNow, _steps.ToArray());
        }
    }

    /// <summary>
    /// Records a step when recording is active.
    /// </summary>
    /// <param name="step">The step to record.</param>
    public void Record(RecordedStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        lock (_gate)
        {
            if (!_isRecording)
            {
                return;
            }

            _steps.Add(step);
        }
    }

    /// <summary>
    /// Records a command invocation and inserts an idle wait.
    /// </summary>
    /// <param name="commandId">The command id.</param>
    /// <param name="input">The optional input.</param>
    public void RecordCommand(string commandId, JsonElement? input = null)
    {
        Record(new RecordedCommandStep(commandId, input));
        Record(new RecordedWaitIdleStep(_defaultWait));
    }

    /// <summary>
    /// Records a click and inserts an idle wait.
    /// </summary>
    /// <param name="automationId">The automation id.</param>
    public void RecordClick(string automationId)
    {
        Record(new RecordedClickStep(automationId));
        Record(new RecordedWaitIdleStep(_defaultWait));
    }

    /// <summary>
    /// Records a text edit and inserts an idle wait.
    /// </summary>
    /// <param name="automationId">The automation id.</param>
    /// <param name="value">The text value.</param>
    public void RecordSetText(string automationId, string value)
    {
        Record(new RecordedSetTextStep(automationId, value));
        Record(new RecordedWaitIdleStep(_defaultWait));
    }
}

/// <summary>
/// Invokes UI controls by automation id for scenario replay.
/// </summary>
public interface IAutomationControlInvoker
{
    /// <summary>Clicks a control.</summary>
    /// <param name="automationId">The automation id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes after the click.</returns>
    Task ClickAsync(string automationId, CancellationToken cancellationToken = default);

    /// <summary>Sets text on a control.</summary>
    /// <param name="automationId">The automation id.</param>
    /// <param name="value">The text value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes after the text change.</returns>
    Task SetTextAsync(string automationId, string value, CancellationToken cancellationToken = default);
}

/// <summary>
/// Replays recorded scenarios using semantic commands first and UI automation identifiers as fallback.
/// </summary>
public sealed class AgentScenarioReplayer
{
    private readonly IAgentCommandRegistry _commands;
    private readonly ICurrentViewModelProvider _currentViewModelProvider;
    private readonly IAutomationControlInvoker _automation;
    private readonly UiActivityTracker _activityTracker;
    private readonly IAgentStateProvider _stateProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentScenarioReplayer"/> class.
    /// </summary>
    /// <param name="commands">The command registry.</param>
    /// <param name="currentViewModelProvider">The current view model provider.</param>
    /// <param name="automation">The automation invoker.</param>
    /// <param name="activityTracker">The activity tracker.</param>
    /// <param name="stateProvider">The state provider.</param>
    public AgentScenarioReplayer(
        IAgentCommandRegistry commands,
        ICurrentViewModelProvider currentViewModelProvider,
        IAutomationControlInvoker automation,
        UiActivityTracker activityTracker,
        IAgentStateProvider stateProvider)
    {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _currentViewModelProvider = currentViewModelProvider ?? throw new ArgumentNullException(nameof(currentViewModelProvider));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
    }

    /// <summary>
    /// Replays a recorded scenario.
    /// </summary>
    /// <param name="scenario">The scenario.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes after replay.</returns>
    public async Task ReplayAsync(RecordedScenario scenario, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        foreach (var step in scenario.Steps)
        {
            switch (step)
            {
                case RecordedCommandStep command:
                    await _commands.InvokeAsync(command.CommandId, _currentViewModelProvider.CurrentViewModel, command.Input, cancellationToken).ConfigureAwait(false);
                    break;
                case RecordedClickStep click:
                    await _automation.ClickAsync(click.AutomationId, cancellationToken).ConfigureAwait(false);
                    break;
                case RecordedSetTextStep text:
                    await _automation.SetTextAsync(text.AutomationId, text.Value, cancellationToken).ConfigureAwait(false);
                    break;
                case RecordedWaitIdleStep wait:
                    await _activityTracker.WaitForIdleAsync(wait.Timeout, cancellationToken).ConfigureAwait(false);
                    break;
                case RecordedAssertRouteStep route:
                    await AssertRouteAsync(route.Route, cancellationToken).ConfigureAwait(false);
                    break;
                case RecordedSnapshotStep:
                    break;
                default:
                    throw new NotSupportedException($"Unsupported recorded step '{step.GetType().Name}'.");
            }
        }
    }

    private async Task AssertRouteAsync(string route, CancellationToken cancellationToken)
    {
        var snapshot = await _stateProvider.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (!StringComparer.Ordinal.Equals(snapshot.Route, route))
        {
            throw new InvalidOperationException($"Expected route '{route}', got '{snapshot.Route}'.");
        }
    }
}
