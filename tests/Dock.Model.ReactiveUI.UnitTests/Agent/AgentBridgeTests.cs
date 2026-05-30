using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dock.Model.ReactiveUI.Services.Agent;
using ReactiveUI;
using System.Reactive;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Agent;

public sealed class AgentBridgeTests
{
    [Fact]
    public async Task Registry_lists_and_invokes_registered_reactive_command()
    {
        var viewModel = new TestViewModel();
        var tracker = new UiActivityTracker();
        var registry = new AgentCommandRegistry(tracker);
        registry.Register<TestViewModel, string, string>(
            "test.echo",
            "Echo",
            "Echoes input for an agent.",
            vm => vm.Echo,
            requiresIdleAfterInvoke: true);

        var commands = await registry.GetAvailableCommandsAsync(viewModel);
        var input = JsonSerializer.SerializeToElement("hello");
        var result = await registry.InvokeAsync("test.echo", viewModel, input);

        Assert.Single(commands);
        Assert.Equal("test.echo", commands[0].Id);
        Assert.True(commands[0].CanExecute);
        Assert.False(commands[0].IsExecuting);
        Assert.Equal("hello", result);
        Assert.Equal("hello", viewModel.LastEcho);
    }

    [Fact]
    public async Task Script_runner_records_evidence_and_stops_on_assertion_failure()
    {
        var viewModel = new TestViewModel();
        var current = new TestCurrentViewModelProvider(viewModel);
        var tracker = new UiActivityTracker();
        var registry = new AgentCommandRegistry(tracker);
        registry.Register<TestViewModel, Unit, Unit>(
            "test.mark",
            "Mark",
            "Marks the view model.",
            vm => vm.Mark);

        var state = new DelegateAgentStateProvider(
            () => "TestViewModel",
            () => new Dictionary<string, JsonElement>
            {
                ["marked"] = JsonSerializer.SerializeToElement(viewModel.Marked)
            },
            tracker);
        var recorder = new AgentExecutionRecorder();
        var screenshots = new TestScreenshotService();
        var runner = new AgentScriptRunner(
            registry,
            current,
            tracker,
            state,
            screenshots,
            recorder,
            new ScreenshotPolicy(AfterEachStep: false, OnFailure: true));
        var expected = new Dictionary<string, JsonElement>
        {
            ["marked"] = JsonSerializer.SerializeToElement(false)
        };
        var script = new AgentScript(
            "failing-script",
            TimeSpan.FromSeconds(10),
            new AgentScriptStep[]
            {
                new InvokeCommandStep("test.mark"),
                new AssertStateStep("TestViewModel", expected)
            });

        var result = await runner.RunAsync(script);

        Assert.False(result.Success);
        Assert.NotNull(result.ExecutionId);
        Assert.Equal(2, result.Steps.Count);
        Assert.False(result.Steps[1].Success);
        Assert.Single(screenshots.Captured);
        Assert.Contains("failure-step-1", screenshots.Captured[0], StringComparison.Ordinal);
        Assert.Contains(recorder.Get(result.ExecutionId!).Artifacts, artifact => artifact.Kind == "screenshot");
    }

    [Fact]
    public async Task Scenario_recorder_and_replayer_use_semantic_steps_and_automation_ids()
    {
        var viewModel = new TestViewModel();
        var current = new TestCurrentViewModelProvider(viewModel);
        var tracker = new UiActivityTracker();
        var registry = new AgentCommandRegistry(tracker);
        registry.Register<TestViewModel, string, string>(
            "test.echo",
            "Echo",
            "Echoes input for an agent.",
            vm => vm.Echo);
        var state = new DelegateAgentStateProvider(
            () => viewModel.Route,
            () => new Dictionary<string, JsonElement>(),
            tracker);
        var automation = new TestAutomationInvoker();
        var recorder = new AgentScenarioRecorder(TimeSpan.FromMilliseconds(1));
        recorder.Start();
        recorder.RecordSetText("SearchTextBox", "abc");
        recorder.RecordCommand("test.echo", JsonSerializer.SerializeToElement("from-command"));
        recorder.Record(new RecordedAssertRouteStep("Ready"));
        var scenario = recorder.Stop("recorded");
        var replayer = new AgentScenarioReplayer(registry, current, automation, tracker, state);

        await replayer.ReplayAsync(scenario);

        Assert.Equal("abc", automation.TextValues["SearchTextBox"]);
        Assert.Equal("from-command", viewModel.LastEcho);
        Assert.False(recorder.IsRecording);
    }

    private sealed class TestViewModel
    {
        public TestViewModel()
        {
            Echo = ReactiveCommand.Create<string, string>(value =>
            {
                LastEcho = value;
                return value;
            });
            Mark = ReactiveCommand.Create(() =>
            {
                Marked = true;
                return Unit.Default;
            });
        }

        public ReactiveCommand<string, string> Echo { get; }

        public ReactiveCommand<Unit, Unit> Mark { get; }

        public string Route { get; } = "Ready";

        public string? LastEcho { get; private set; }

        public bool Marked { get; private set; }
    }

    private sealed class TestCurrentViewModelProvider : ICurrentViewModelProvider
    {
        public TestCurrentViewModelProvider(object? currentViewModel)
        {
            CurrentViewModel = currentViewModel;
        }

        public object? CurrentViewModel { get; }
    }

    private sealed class TestScreenshotService : IAgentScreenshotService
    {
        public List<string> Captured { get; } = new();

        public Task<string> CaptureMainWindowAsync(string executionId, string name, CancellationToken cancellationToken = default)
        {
            var path = $"/{executionId}/{name}.png";
            Captured.Add(path);
            return Task.FromResult(path);
        }
    }

    private sealed class TestAutomationInvoker : IAutomationControlInvoker
    {
        public Dictionary<string, string> TextValues { get; } = new(StringComparer.Ordinal);

        public Task ClickAsync(string automationId, CancellationToken cancellationToken = default)
        {
            TextValues[automationId] = "clicked";
            return Task.CompletedTask;
        }

        public Task SetTextAsync(string automationId, string value, CancellationToken cancellationToken = default)
        {
            TextValues[automationId] = value;
            return Task.CompletedTask;
        }
    }
}
